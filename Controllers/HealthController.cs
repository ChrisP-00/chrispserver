using chrispserver.DbConfigurations;
using chrispserver.ResReqModels;
using chrispserver.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace chrispserver.Controllers;



[ApiController]
[Route("Health")]
public class HealthController : ControllerBase
{

    private readonly ConnectionManager _connectionManager;

    public HealthController(ConnectionManager connectionManager)
    {
        _connectionManager = connectionManager;
    }



    [HttpGet("Ping")]
    [HttpHead("Ping")]
    public async Task<Result> Ping()
    {
        try
        {
            var gameDb = _connectionManager.GetSqlQueryFactory(DbKeys.GameServerDB);
            await gameDb.StatementAsync("SELECT 1");


            var masterDb = _connectionManager.GetSqlQueryFactory(DbKeys.MasterDB);
            await masterDb.StatementAsync("SELECT 1");

            return Result.Success("Pong + DBs");
        }
        catch (Exception ex)
        {
            return Result.Fail(ResultCodes.DBConnectionFail, ex.ToString());
        }

    }

}
