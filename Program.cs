using WeatherHazardApi.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Register Services
builder.Services.AddHttpClient<IWeatherService, WeatherService>();
builder.Services.AddScoped<IFileService, FileService>();
builder.Services.AddScoped<IHazardService, HazardService>();

// Cosmos DB Registration
builder.Services.AddSingleton<Microsoft.Azure.Cosmos.CosmosClient>(sp =>
{
    var configuration = sp.GetRequiredService<IConfiguration>();
    var cosmosSettings = configuration.GetSection("Cosmos");
    return new Microsoft.Azure.Cosmos.CosmosClient(cosmosSettings["ConnectionString"]);
});

// Notification Services
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<ILocalWeatherService, LocalWeatherService>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<INotificationService, NotificationService>();

builder.Services.AddControllersWithViews();

var app = builder.Build();

//// Configure the HTTP request pipeline.
//if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseStaticFiles();

app.UseAuthorization();

app.MapControllers();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=UserCoverage}/{action=Index}/{id?}");

app.Run();
