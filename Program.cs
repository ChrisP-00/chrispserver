using chrispserver.DbConfigurations;
using chrispserver.Middlewares;
using chrispserver.Securities;
using chrispserver.Services;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);

// ������ ����
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
    // ������ DB �ε�
    var masterHandler = app.Services.GetRequiredService<IMasterHandler>();
    await masterHandler.LoadAllAsync();
    Console.WriteLine($"[Check] LoadAllAsync ��� MasterHandler �ؽ�: {masterHandler.GetHashCode()}");

}
catch (Exception ex)
{
    Console.WriteLine($"[Error] ������ DB �ε� ���� : {ex.ToString()}");
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

// API ��û�� ó���� ��Ʈ�ѷ� ���
app.MapControllers();

app.MapStaticAssets();
app.MapRazorPages()
   .WithStaticAssets();

app.Run();