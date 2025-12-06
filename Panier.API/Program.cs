using StackExchange.Redis;
using Panier.API.Services;

var builder = WebApplication.CreateBuilder(args);

// Configuration Redis
var redisConnection = builder.Configuration.GetConnectionString("Redis")
    ?? "localhost:6379";

builder.Services.AddSingleton<IConnectionMultiplexer>(
    ConnectionMultiplexer.Connect(redisConnection));

builder.Services.AddSingleton<RedisPanierService>();

// Configuration API
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// CORS (pour permettre les appels depuis le frontend)
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

// Configuration du pipeline HTTP
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowAll");

// Récupération du port dynamique (pour Railway)
var port = Environment.GetEnvironmentVariable("PORT") ?? "5001";
app.Urls.Add($"http://0.0.0.0:{port}");

app.MapControllers();

app.Run();