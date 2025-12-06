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

Console.WriteLine($"📥 REDIS_URL brut reçu: {MaskPassword(redisConnection)}");

// 🔥 CONVERSION de l'URL Redis au format StackExchange.Redis
string connectionString = ConvertRedisUrl(redisConnection);

Console.WriteLine($"🔗 Connection string converti: {MaskPassword(connectionString)}");

// Configuration Redis avec options robustes
var configOptions = ConfigurationOptions.Parse(connectionString); // ✅ Utilise connectionString converti
configOptions.AbortOnConnectFail = false;
configOptions.ConnectTimeout = 10000;
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
    
    Console.WriteLine($"Connection String utilisée: {MaskPassword(connectionString)}");
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

// === FONCTIONS UTILITAIRES ===

/// <summary>
/// Convertit une URL Redis (redis://...) au format StackExchange.Redis (host:port,password=...)
/// </summary>
static string ConvertRedisUrl(string redisUrl)
{
    // Si ce n'est pas une URL redis://, retourner tel quel (format local)
    if (!redisUrl.StartsWith("redis://") && !redisUrl.StartsWith("rediss://"))
    {
        return redisUrl;
    }

    try
    {
        var uri = new Uri(redisUrl);
        
        // Extraire les composants
        var host = uri.Host;
        var port = uri.Port > 0 ? uri.Port : 6379;
        var password = !string.IsNullOrEmpty(uri.UserInfo) 
            ? uri.UserInfo.Split(':').LastOrDefault() 
            : null;
        
        // Construire la chaîne de connexion au format StackExchange.Redis
        var connectionString = $"{host}:{port}";
        
        if (!string.IsNullOrEmpty(password))
        {
            connectionString += $",password={password}";
        }
        
        // SSL si rediss://
        if (redisUrl.StartsWith("rediss://"))
        {
            connectionString += ",ssl=true,abortConnect=false";
        }
        else
        {
            connectionString += ",abortConnect=false";
        }
        
        return connectionString;
    }
    catch (Exception ex)
    {
        Console.WriteLine($"⚠️ Erreur lors de la conversion de l'URL Redis: {ex.Message}");
        return redisUrl; // Retourner l'original en cas d'erreur
    }
}

/// <summary>
/// Masque le mot de passe dans les logs pour la sécurité
/// </summary>
static string MaskPassword(string connectionString)
{
    if (string.IsNullOrEmpty(connectionString))
        return connectionString;
    
    // Format StackExchange.Redis: host:port,password=xxx
    if (connectionString.Contains("password="))
    {
        var parts = connectionString.Split(',');
        for (int i = 0; i < parts.Length; i++)
        {
            if (parts[i].Trim().StartsWith("password="))
            {
                parts[i] = "password=***";
            }
        }
        return string.Join(",", parts);
    }
    
    // Format URL: redis://user:password@host:port
    if (connectionString.Contains("@"))
    {
        var atIndex = connectionString.IndexOf("@");
        var colonIndex = connectionString.LastIndexOf(":", atIndex);
        if (colonIndex > 0)
        {
            return connectionString.Substring(0, colonIndex + 1) + "***" + connectionString.Substring(atIndex);
        }
    }
    
    return connectionString;
}