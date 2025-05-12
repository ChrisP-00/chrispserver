using chrispserver.ResReqModels;
using MySqlConnector;
using SqlKata.Compilers;
using SqlKata.Execution;
using System.Collections.Concurrent;

namespace chrispserver.DbConfigurations;

public class ConnectionManager : IDisposable
{
    private readonly IConfiguration _configuration;
    private readonly ConcurrentDictionary<string, string> _connectionStrings;

    public ConnectionManager(IConfiguration configuration)
    {
        _configuration = configuration;
        _connectionStrings = new ConcurrentDictionary<string, string>
        {
            [DbKeys.MasterDataDB] = _configuration.GetConnectionString(DbKeys.MasterDataDB)!,
            [DbKeys.GameServerDB] = _configuration.GetConnectionString(DbKeys.GameServerDB)!
        };
    }

    /// <summary>
    /// MySQL 연결을 생성하여 QueryFactory를 반환
    /// </summary>
    private QueryFactory CreateQueryFactory(string dbName)
    {
        if (!_connectionStrings.TryGetValue(dbName, out var connectionString) || string.IsNullOrEmpty(connectionString))
        {
            throw new InvalidOperationException($"[ERROR] Connection string for {dbName} is missing or invalid.");
        }

        var connection = new MySqlConnection(connectionString);
        var compiler = new MySqlCompiler();

        Console.WriteLine($"[ConnectionManager] 새로운 연결 생성됨: {dbName}");
        return new QueryFactory(connection, compiler)
        {
            Logger = compiled => Console.WriteLine($"[SQLKata] {compiled.ToString()}") // SQL 로그 출력
        };
    }

    /// <summary>
    /// 데이터베이스별 QueryFactory 가져오기 (각 요청마다 새로운 연결 사용)
    /// </summary>
    public QueryFactory GetSqlQueryFactory(string dbName)
    {
        var queryFactory = CreateQueryFactory(dbName);

        if (queryFactory.Connection.State == System.Data.ConnectionState.Closed)
        {
            queryFactory.Connection.Open();
            Console.WriteLine($"[ConnectionManager] 연결 열림: {dbName}");
        }

        return queryFactory;
    }

    /// <summary>
    /// 주어진 데이터베이스에서 트랜잭션 실행
    /// </summary>
    public async Task<Result> ExecuteInTransactionAsync(string dbName, Func<QueryFactory, MySqlTransaction, Task<Result>> action)
    {
        if (!_connectionStrings.TryGetValue(dbName, out var connectionString) || string.IsNullOrEmpty(connectionString))
        {
            return Result.Fail(ResultCodes.Transaction_Fail_ConnectionStringNull);
        }

        await using var connection = new MySqlConnection(connectionString);
        await connection.OpenAsync();
        Console.WriteLine($"[Transaction] 연결 열림: {dbName} (트랜잭션 시작)");

        await using var transaction = await connection.BeginTransactionAsync();
        // 트랜잭션을 명시적으로 지정함!
        var queryFactory = new QueryFactory(connection, new MySqlCompiler());

        try
        {
            var result = await action(queryFactory, (MySqlTransaction)transaction);

            if (result.ResultCode != ResultCodes.Ok)
            {
                await transaction.RollbackAsync();
                Console.WriteLine($"[Transaction] 롤백됨 (Result.Fail 반환): {dbName}");
                return result;
            }

            await transaction.CommitAsync();
            Console.WriteLine($"[Transaction] 커밋 완료: {dbName}");
            return Result.Success();
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            Console.WriteLine($"[Transaction] 롤백됨 (예외): {dbName}");

            Console.WriteLine($"ConnextionManager Error : {ex.Message}");

            return Result.Fail(ResultCodes.Transaction_Fail_Rollback);
        }
    }

    public async Task<Result<T>> ExecuteInTransactionAsync<T>(string dbName, Func<QueryFactory, MySqlTransaction, Task<Result<T>>> action)
    {
        if (!_connectionStrings.TryGetValue(dbName, out var connectionString) || string.IsNullOrEmpty(connectionString))
        {
            return Result<T>.Fail(ResultCodes.Transaction_Fail_ConnectionStringNull);
        }

        await using var connection = new MySqlConnection(connectionString);
        await connection.OpenAsync();
        Console.WriteLine($"[Transaction<T>] 연결 열림: {dbName} (트랜잭션 시작)");

        await using var transaction = await connection.BeginTransactionAsync();
        var queryFactory = new QueryFactory(connection, new MySqlCompiler());

        try
        {
            var result = await action(queryFactory, (MySqlTransaction)transaction);

            if (result.ResultCode != ResultCodes.Ok)
            {
                await transaction.RollbackAsync();
                Console.WriteLine($"[Transaction<T>] 롤백됨 (Result.Fail 반환): {dbName}");
                return result;
            }

            await transaction.CommitAsync();
            Console.WriteLine($"[Transaction<T>] 커밋 완료: {dbName}");
            return result;
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            Console.WriteLine($"[Transaction<T>] 롤백됨 (예외): {dbName}");
            Console.WriteLine($"[Error] {ex.Message}");

            return Result<T>.Fail(ResultCodes.Transaction_Fail_Rollback);
        }
    }

    public void Dispose()
    {
        Console.WriteLine($"[ConnectionManager] 모든 연결 정리됨.");
    }
}