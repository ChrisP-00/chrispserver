using chrispserver.DbConfigurations;
using chrispserver.Services;

var builder = WebApplication.CreateBuilder(args);

// 의존성 주입
builder.Services.AddSingleton<ConnectionManager>();
builder.Services.AddSingleton<IMaster, MasterDBService>();
builder.Services.AddSingleton<IMasterHandler, MasterHandler>();
builder.Services.AddTransient<IAccount, AccountService>();
builder.Services.AddTransient<ICharacter, CharacterService>();
builder.Services.AddTransient<IMission, MissionService>();

// Add services to the container.
builder.Services.AddRazorPages();

var app = builder.Build();

// 마스터 DB 로딩
var masterHandler = app.Services.GetRequiredService<IMasterHandler>();
await masterHandler.LoadAllAsync();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseRouting();

app.UseAuthorization();

// API 컨트롤러 매핑
app.MapControllers();       // API 요청을 처리할 컨트롤러 등록

app.MapStaticAssets();
app.MapRazorPages()
   .WithStaticAssets();

app.Run();



