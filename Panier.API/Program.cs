using StackExchange.Redis;
using Panier.API.Services;

var builder = WebApplication.CreateBuilder(args);

// === CONFIGURATION REDIS ===
// Essayer d'abord REDIS_URL (variable Railway native)
var redisConnection = Environment.GetEnvironmentVariable("REDIS_URL");

// Si pas trouvé, essayer ConnectionStrings:Redis
if (string.IsNullOrEmpty(redisConnection))
{
    redisConnection = builder.Configuration.GetConnectionString("Redis");
}

// Si toujours vide, valeur par défaut
if (string.IsNullOrEmpty(redisConnection))
{
    Console.WriteLine("⚠️ ATTENTION: Aucune configuration Redis trouvée !");
    redisConnection = "localhost:6379";
}

Console.WriteLine($"🔗 Connexion à Redis: {redisConnection.Substring(0, Math.Min(40, redisConnection.Length))}...");

try
{
    var redis = ConnectionMultiplexer.Connect(redisConnection);
    builder.Services.AddSingleton<IConnectionMultiplexer>(redis);
    Console.WriteLine("✅ Redis connecté avec succès !");
}
catch (Exception ex)
{
    Console.WriteLine($"❌ ERREUR Redis: {ex.Message}");
    if (ex.InnerException != null)
    {
        Console.WriteLine($"   Inner: {ex.InnerException.Message}");
    }
    throw;
}

builder.Services.AddSingleton<RedisPanierService>();

// Configuration API
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// CORS
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

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowAll");

// Port dynamique Railway
var port = Environment.GetEnvironmentVariable("PORT") ?? "5001";
Console.WriteLine($"🚀 Démarrage sur le port: {port}");

app.Urls.Add($"http://0.0.0.0:{port}");

app.MapControllers();

Console.WriteLine("✅ Application démarrée avec succès !");
app.Run();