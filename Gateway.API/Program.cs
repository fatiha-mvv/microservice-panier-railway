// var builder = WebApplication.CreateBuilder(args);

// // Configuration HttpClient
// builder.Services.AddHttpClient();

// // Configuration API
// builder.Services.AddControllers();
// builder.Services.AddEndpointsApiExplorer();
// builder.Services.AddSwaggerGen();

// // CORS
// builder.Services.AddCors(options =>
// {
//     options.AddPolicy("AllowAll", policy =>
//     {
//         policy.AllowAnyOrigin()
//               .AllowAnyMethod()
//               .AllowAnyHeader();
//     });
// });

// var app = builder.Build();

// // Configuration du pipeline HTTP
// if (app.Environment.IsDevelopment())
// {
//     app.UseSwagger();
//     app.UseSwaggerUI();
// }

// app.UseCors("AllowAll");

// // R�cup�ration du port dynamique (pour Railway)
// var port = Environment.GetEnvironmentVariable("PORT") ?? "5000";
// app.Urls.Add($"http://0.0.0.0:{port}");

// app.MapControllers();

// app.Run();


using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = WebApplication.CreateBuilder(args);

// Configuration HttpClient
builder.Services.AddHttpClient();

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

// 🔥 LOGS DE DEBUG - TRÈS IMPORTANT
Console.WriteLine("=== CONFIGURATION GATEWAY API ===");

// Vérifier la configuration Panier API
var panierApiFromConfig = app.Configuration["Services:PanierApi"];
var panierApiFromEnv = Environment.GetEnvironmentVariable("Services__PanierApi");

Console.WriteLine($"📋 Services:PanierApi (Config) = {panierApiFromConfig ?? "NULL"}");
Console.WriteLine($"📋 Services__PanierApi (ENV) = {panierApiFromEnv ?? "NULL"}");

var panierApiUrl = panierApiFromEnv ?? panierApiFromConfig ?? "http://localhost:5001";
Console.WriteLine($"🔗 URL finale Panier API = {panierApiUrl}");

// Vérifier que l'URL est bien formée
if (!panierApiUrl.StartsWith("http://") && !panierApiUrl.StartsWith("https://"))
{
    Console.WriteLine("⚠️ ATTENTION: URL Panier API sans protocole http:// ou https://");
    panierApiUrl = $"http://{panierApiUrl}";
    Console.WriteLine($"🔧 URL corrigée = {panierApiUrl}");
}

// Configuration du pipeline HTTP
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowAll");

// Récupération du port dynamique (Railway injecte automatiquement PORT)
var port = Environment.GetEnvironmentVariable("PORT") ?? "5000";
Console.WriteLine($"🚀 Démarrage du Gateway sur le port: {port}");
app.Urls.Add($"http://0.0.0.0:{port}");

app.MapControllers();

Console.WriteLine("✅✅✅ Gateway API démarré avec SUCCÈS ! ✅✅✅");
app.Run();