using StackExchange.Redis;
using Panier.API.Services;

var builder = WebApplication.CreateBuilder(args);

// Configuration Redis - AVEC GESTION D'ERREUR
var redisConnection = builder.Configuration.GetConnectionString("Redis");

// Si vide, utiliser une valeur par défaut pour le développement
if (string.IsNullOrEmpty(redisConnection))
{
    Console.WriteLine("ATTENTION: ConnectionString Redis est vide ! Utilisation de localhost:6379");
    redisConnection = "localhost:6379";
}

Console.WriteLine($"Tentative de connexion à Redis: {redisConnection}");

try
{
    builder.Services.AddSingleton<IConnectionMultiplexer>(
        ConnectionMultiplexer.Connect(redisConnection));
    Console.WriteLine("✓ Connexion Redis réussie");
}
catch (Exception ex)
{
    Console.WriteLine($"✗ ERREUR connexion Redis: {ex.Message}");
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

// Configuration du pipeline HTTP
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowAll");

// Récupération du port dynamique (pour Railway)
var port = Environment.GetEnvironmentVariable("PORT") ?? "5001";
Console.WriteLine($"Démarrage sur le port: {port}");

app.Urls.Add($"http://0.0.0.0:{port}");

app.MapControllers();

Console.WriteLine("Application démarrée avec succès !");
app.Run();