using chrispserver.DbConfigurations;
using chrispserver.Middlewares;
using chrispserver.Securities;
using chrispserver.Services;
using chrispserver.Utilities;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);

// ������ ����
builder.Services.AddSingleton<ConnectionManager>();

// Redis ���� ���� ���
builder.Services.AddSingleton<IConnectionMultiplexer?>(provider =>
{
    var config = provider.GetRequiredService<IConfiguration>();
    var redisConfig = config.GetConnectionString("Redis");

    if (!string.IsNullOrWhiteSpace(redisConfig))
    {
        try
        {
            Console.WriteLine($"[Redis] ���� �õ�");
            return ConnectionMultiplexer.Connect(redisConfig);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Redis] ���� ����: {ex.Message}");
        }
    }
    else
    {
        Console.WriteLine("[Redis] ���� ���ڿ��� ��� ����");
    }

    // Redis ���̵� ����ǵ��� null ��ȯ
    return null;
});

// Redis ���� Ŭ���� ���
builder.Services.AddSingleton<IRedisAuthService, RedisAuthService>();
builder.Services.AddSingleton<IMemoryDb, RedisMemoryDb>();

builder.Services.AddSingleton<IMaster, MasterDBService>();
builder.Services.AddSingleton<IMasterHandler, MasterHandler>();
builder.Services.AddTransient<IAccount, AccountService>();
builder.Services.AddTransient<ICharacter, CharacterService>();
builder.Services.AddTransient<IMission, MissionService>();

// �⺻ �ܼ� �α� Ȱ��ȭ
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

// LogManager�� LoggerFactory ����
LogManager.SetLoggerFactory(app.Services.GetRequiredService<ILoggerFactory>(), "Server");
// �� ���� �� �α� ť ������ ó��
AppDomain.CurrentDomain.ProcessExit += (_, _) => LogManager.Dispose();

try
{
    // ������ DB �ε�
    var masterHandler = app.Services.GetRequiredService<IMasterHandler>();
    await masterHandler.LoadAllAsync();
    Console.WriteLine($"[Check] LoadAllAsync ��� MasterHandler �ؽ�: {masterHandler.GetHashCode()}");
}
catch (Exception ex)
{
    Console.WriteLine($"[Error] ������ DB �ε� ���� : {ex}");
    // ���� �ߴ� ���� �α׸� ����� �����Ϸ��� throw ���� ����
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
