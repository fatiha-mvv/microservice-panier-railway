using StackExchange.Redis;
using Panier.API.Services;

var builder = WebApplication.CreateBuilder(args);

// === CONFIGURATION REDIS ===
var redisConnection = Environment.GetEnvironmentVariable("REDIS_URL");

if (string.IsNullOrEmpty(redisConnection))
{
    redisConnection = builder.Configuration.GetConnectionString("Redis");
}

if (string.IsNullOrEmpty(redisConnection))
{
    Console.WriteLine("⚠️ ERREUR: Aucune configuration Redis trouvée !");
    redisConnection = "localhost:6379";
}

// Afficher l'URL (masquer le mot de passe pour sécurité)
var safeUrl = redisConnection.Contains("@") 
    ? redisConnection.Substring(0, redisConnection.IndexOf("@")) + "@***" 
    : redisConnection;
Console.WriteLine($"🔗 Tentative de connexion à Redis: {safeUrl}");

// Configuration Redis avec options robustes
var configOptions = ConfigurationOptions.Parse(redisConnection);
configOptions.AbortOnConnectFail = false; // Ne pas crasher si Redis n'est pas prêt
configOptions.ConnectTimeout = 10000; // 10 secondes de timeout
configOptions.SyncTimeout = 5000;
configOptions.ConnectRetry = 3;

try
{
    Console.WriteLine("⏳ Connexion à Redis en cours...");
    var redis = ConnectionMultiplexer.Connect(configOptions);
    
    // Vérifier que Redis répond
    var db = redis.GetDatabase();
    db.Ping();
    
    builder.Services.AddSingleton<IConnectionMultiplexer>(redis);
    Console.WriteLine("✅✅✅ Redis connecté avec SUCCÈS ! ✅✅✅");
}
catch (Exception ex)
{
    Console.WriteLine($"❌❌❌ ERREUR de connexion Redis ❌❌❌");
    Console.WriteLine($"Type: {ex.GetType().Name}");
    Console.WriteLine($"Message: {ex.Message}");
    
    if (ex.InnerException != null)
    {
        Console.WriteLine($"Inner Exception: {ex.InnerException.Message}");
    }
    
    Console.WriteLine($"Connection String utilisée: {safeUrl}");
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

var port = Environment.GetEnvironmentVariable("PORT") ?? "5001";
Console.WriteLine($"🚀 Démarrage de l'application sur le port: {port}");

app.Urls.Add($"http://0.0.0.0:{port}");

app.MapControllers();

Console.WriteLine("✅✅✅ Application démarrée avec SUCCÈS ! ✅✅✅");
app.Run();