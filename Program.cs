using chrispserver.DbConfigurations;
using chrispserver.Middlewares;
using chrispserver.Securities;
using chrispserver.Services;
using chrispserver.Utilities;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);

// 의존성 주입
builder.Services.AddSingleton<ConnectionManager>();

// Redis 연결 예외 방어
builder.Services.AddSingleton<IConnectionMultiplexer?>(provider =>
{
    var config = provider.GetRequiredService<IConfiguration>();
    var redisConfig = config.GetConnectionString("Redis");

    if (!string.IsNullOrWhiteSpace(redisConfig))
    {
        try
        {
            Console.WriteLine($"[Redis] 연결 시도");
            return ConnectionMultiplexer.Connect(redisConfig);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Redis] 연결 실패: {ex.Message}");
        }
    }
    else
    {
        Console.WriteLine("[Redis] 연결 문자열이 비어 있음");
    }

    // Redis 없이도 실행되도록 null 반환
    return null;
});

// Redis 의존 클래스 등록
builder.Services.AddSingleton<IRedisAuthService, RedisAuthService>();
builder.Services.AddSingleton<IMemoryDb, RedisMemoryDb>();

builder.Services.AddSingleton<IMaster, MasterDBService>();
builder.Services.AddSingleton<IMasterHandler, MasterHandler>();
builder.Services.AddTransient<IAccount, AccountService>();
builder.Services.AddTransient<ICharacter, CharacterService>();
builder.Services.AddTransient<IMission, MissionService>();

// 기본 콘솔 로깅 활성화
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

// Add services to the container.
builder.Services.AddRazorPages();

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
    });

var app = builder.Build();

// LogManager에 LoggerFactory 주입
LogManager.SetLoggerFactory(app.Services.GetRequiredService<ILoggerFactory>(), "Server");
// 앱 종료 시 로그 큐 마무리 처리
AppDomain.CurrentDomain.ProcessExit += (_, _) => LogManager.Dispose();

try
{
    // 마스터 DB 로딩
    var masterHandler = app.Services.GetRequiredService<IMasterHandler>();
    await masterHandler.LoadAllAsync();
    Console.WriteLine($"[Check] LoadAllAsync 대상 MasterHandler 해시: {masterHandler.GetHashCode()}");
}
catch (Exception ex)
{
    Console.WriteLine($"[Error] 마스터 DB 로딩 실패 : {ex}");
    // 실행 중단 없이 로그만 남기고 진행하려면 throw 제거 가능
    throw;
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseMiddleware<UserAuthMiddleware>();

app.UseAuthorization();

app.MapControllers();
app.MapStaticAssets();
app.MapRazorPages().WithStaticAssets();

app.Run();
