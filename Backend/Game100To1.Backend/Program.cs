using Game100To1.Backend.Hubs;
using Game100To1.Backend.Services;
using Game100To1.Backend.Models;

var builder = WebApplication.CreateBuilder(args);

// Configure game questions from appsettings
builder.Services.Configure<GameQuestionsConfiguration>(builder.Configuration);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddSignalR();
builder.Services.AddSingleton<GameService>();

// Configure routing to lowercase
builder.Services.Configure<RouteOptions>(options =>
{
    options.LowercaseUrls = true;
    options.LowercaseQueryStrings = true;
});

// Add Swagger/OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "Game 100 to 1 API",
        Version = "v1",
        Description = "API для игры '100 к 1' с поддержкой SignalR для реального времени",
        Contact = new Microsoft.OpenApi.Models.OpenApiContact
        {
            Name = "Game 100 to 1",
        }
    });
});

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}
else
{
    // Enable Swagger in development
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Game 100 to 1 API v1");
        c.RoutePrefix = "swagger";
    });
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.UseCors();

app.MapControllers();
app.MapHub<GameHub>("/gamehub");

// Serve Vue.js app for any non-API routes
app.MapFallbackToFile("index.html");

// Log startup information
var logger = app.Services.GetRequiredService<ILogger<Program>>();
logger.LogInformation("🎯 Game '100 to 1' API запущен!");
logger.LogInformation("📖 Swagger UI: http://localhost:5000/swagger");
logger.LogInformation("🔗 SignalR Hub: http://localhost:5000/gamehub");
logger.LogInformation("🎮 Игра доступна по адресу: http://localhost:5000");

app.Run();
