using DotNetEnv;
using Pharmacy.Db;

// Загрузка переменных окружения
Env.Load();

var builder = WebApplication.CreateBuilder(args);

// Добавление переменных окружения
builder.Configuration.AddEnvironmentVariables();

// Считывание переменных окружения
var dbHost = Environment.GetEnvironmentVariable("DB_HOST") ?? throw new Exception("Переменная окружения DB_HOST не найдена");
var dbName = Environment.GetEnvironmentVariable("DB_NAME") ?? throw new Exception("Переменная окружения DB_NAME не найдена");
var dbUser = Environment.GetEnvironmentVariable("DB_USER") ?? throw new Exception("Переменная окружения DB_USER не найдена");
var dbPassword = Environment.GetEnvironmentVariable("DB_PASSWORD") ?? throw new Exception("Переменная окружения DB_PASSWORD не найдена");

// Формирование строки подключения
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new Exception("Шаблон строки подключения не найден");
connectionString = connectionString.Replace("{DB_HOST}", dbHost);
connectionString = connectionString.Replace("{DB_NAME}", dbName);
connectionString = connectionString.Replace("{DB_USER}", dbUser);
connectionString = connectionString.Replace("{DB_PASSWORD}", dbPassword);

// Регистрируем строку подключения в контейнере зависимостей
builder.Services.AddSingleton(connectionString);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Добавляем схему аутентификации через куки
builder.Services.AddAuthentication("Cookies")
    .AddCookie("Cookies", options =>
    {
        options.LoginPath = "/Account/Login";
        options.LogoutPath = "/Account/Logout";
        options.AccessDeniedPath = "/Account/AccessDenied";
        options.ExpireTimeSpan = TimeSpan.FromDays(7);
    });

var app = builder.Build();

// Инициализация бд
var dbInitializer = new DatabaseInitializer(connectionString);
dbInitializer.Initialize();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// Сначала устанавливаем кто пользователь, а потом что ему разрешено
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
