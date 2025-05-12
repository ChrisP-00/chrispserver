using chrispserver.DbConfigurations;
using chrispserver.Middlewares;
using chrispserver.Securities;
using chrispserver.Services;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);

// 의존성 주입
builder.Services.AddSingleton<ConnectionManager>();
builder.Services.AddSingleton<IRedisAuthService, RedisAuthService>();
builder.Services.AddSingleton<IMemoryDb, RedisMemoryDb>();
builder.Services.AddSingleton<IConnectionMultiplexer>(provider =>
{
    var config = provider.GetRequiredService<IConfiguration>();
    var redisConfig = config.GetConnectionString("Redis") ?? "127.0.0.1:6379";
    return ConnectionMultiplexer.Connect(redisConfig);
});
builder.Services.AddSingleton<IMaster, MasterDBService>();
builder.Services.AddSingleton<IMasterHandler, MasterHandler>();
builder.Services.AddTransient<IAccount, AccountService>();
builder.Services.AddTransient<ICharacter, CharacterService>();
builder.Services.AddTransient<IMission, MissionService>();

// Add services to the container.
builder.Services.AddRazorPages();

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
    });

var app = builder.Build();

try
{
    // 마스터 DB 로딩
    var masterHandler = app.Services.GetRequiredService<IMasterHandler>();
    await masterHandler.LoadAllAsync();
    Console.WriteLine($"[Check] LoadAllAsync 대상 MasterHandler 해시: {masterHandler.GetHashCode()}");

}
catch (Exception ex)
{
    Console.WriteLine($"[Error] 마스터 DB 로딩 실패 : {ex.ToString()}");
    throw;
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseRouting();

app.UseMiddleware<UserAuthMiddleware>();

app.UseAuthorization();

// API 요청을 처리할 컨트롤러 등록
app.MapControllers();

app.MapStaticAssets();
app.MapRazorPages()
   .WithStaticAssets();

app.Run();