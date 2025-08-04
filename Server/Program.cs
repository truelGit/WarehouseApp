using Microsoft.EntityFrameworkCore;
using WarehouseApp.Server.Data;

var builder = WebApplication.CreateBuilder(args);

// Подключение Razor Pages, Controllers и DB
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

builder.Services.AddDbContext<WarehouseDbContext>(options =>
	options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Добавление CORS-политики (если тебе нужно, например, с внешнего клиента обращаться — пока не критично)
var MyAllowSpecificOrigins = "_myAllowSpecificOrigins";
builder.Services.AddCors(options =>
{
	options.AddPolicy(name: MyAllowSpecificOrigins,
		policy =>
		{
			policy.WithOrigins("https://localhost:7200")
				  .AllowAnyHeader()
				  .AllowAnyMethod();
		});
});

var app = builder.Build();

// Конвейер обработки запроса
if (app.Environment.IsDevelopment())
{
	app.UseWebAssemblyDebugging();
}
else
{
	app.UseExceptionHandler("/Error");
	app.UseHsts();
}

app.UseHttpsRedirection();                   // перенаправление на https
app.UseBlazorFrameworkFiles();              // ← для загрузки wasm
app.UseStaticFiles();                       // ← для index.html и .js файлов

app.UseRouting();

app.UseCors(MyAllowSpecificOrigins);        // ← если нужен CORS, оставить

app.MapRazorPages();                        // ← страницы Razor
app.MapControllers();                       // ← твои API /api/*
app.MapFallbackToFile("index.html");        // ← отдаст index.html если путь не найден

app.Run();
