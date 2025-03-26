using chrispserver.DBConfigurations;
using chrispserver.Services;

var builder = WebApplication.CreateBuilder(args);

// ������ ����
builder.Services.AddSingleton<ConnectionManager>();


builder.Services.AddSingleton<IMaster, MasterDBService>();
builder.Services.AddTransient<IAccount, AccountService>();
builder.Services.AddTransient<ICharacter, CharacterService>();


// Add services to the container.
builder.Services.AddRazorPages();


var app = builder.Build();

// ������ DB �ε�
var masterService = app.Services.GetRequiredService<IMaster>();
await masterService.LoadAllAsync();


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

// API ��Ʈ�ѷ� ����
app.MapControllers();       // API ��û�� ó���� ��Ʈ�ѷ� ���

app.MapStaticAssets();
app.MapRazorPages()
   .WithStaticAssets();

app.Run();



