// using Microsoft.AspNetCore.Mvc;
// using System.Text.Json;

// namespace Gateway.API.Controllers;

// [ApiController]
// [Route("api")]
// public class GatewayController : ControllerBase
// {
//     private readonly IHttpClientFactory _httpClientFactory;
//     private readonly IConfiguration _configuration;
//     private readonly ILogger<GatewayController> _logger;

//     public GatewayController(
//         IHttpClientFactory httpClientFactory,
//         IConfiguration configuration,
//         ILogger<GatewayController> logger)
//     {
//         _httpClientFactory = httpClientFactory;
//         _configuration = configuration;
//         _logger = logger;
//     }

//     private string GetPanierApiUrl()
//     {
//         return _configuration["Services:PanierApi"] ?? "http://localhost:5001";
//     }

//     // GET: api/panier/{utilisateurId}
//     [HttpGet("panier/{utilisateurId}")]
//     public async Task<IActionResult> ObtenirPanier(string utilisateurId)
//     {
//         _logger.LogInformation("Gateway: R�cup�ration du panier pour {UtilisateurId}", utilisateurId);

//         var client = _httpClientFactory.CreateClient();
//         var url = $"{GetPanierApiUrl()}/api/panier/{utilisateurId}";

//         try
//         {
//             var response = await client.GetAsync(url);
//             var content = await response.Content.ReadAsStringAsync();

//             if (response.IsSuccessStatusCode)
//                 return Ok(JsonSerializer.Deserialize<object>(content));

//             return StatusCode((int)response.StatusCode, content);
//         }
//         catch (Exception ex)
//         {
//             _logger.LogError(ex, "Erreur lors de la r�cup�ration du panier");
//             return StatusCode(500, new { message = "Erreur de communication avec le service Panier" });
//         }
//     }

//     // POST: api/panier/{utilisateurId}/articles
//     [HttpPost("panier/{utilisateurId}/articles")]
//     public async Task<IActionResult> AjouterArticle(string utilisateurId, [FromBody] object article)
//     {
//         _logger.LogInformation("Gateway: Ajout d'un article au panier de {UtilisateurId}", utilisateurId);

//         var client = _httpClientFactory.CreateClient();
//         var url = $"{GetPanierApiUrl()}/api/panier/{utilisateurId}/articles";

//         try
//         {
//             var json = JsonSerializer.Serialize(article);
//             var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

//             var response = await client.PostAsync(url, content);
//             var responseContent = await response.Content.ReadAsStringAsync();

//             if (response.IsSuccessStatusCode)
//                 return Ok(JsonSerializer.Deserialize<object>(responseContent));

//             return StatusCode((int)response.StatusCode, responseContent);
//         }
//         catch (Exception ex)
//         {
//             _logger.LogError(ex, "Erreur lors de l'ajout de l'article");
//             return StatusCode(500, new { message = "Erreur de communication avec le service Panier" });
//         }
//     }

//     // DELETE: api/panier/{utilisateurId}/articles/{articleId}
//     [HttpDelete("panier/{utilisateurId}/articles/{articleId}")]
//     public async Task<IActionResult> RetirerArticle(string utilisateurId, string articleId)
//     {
//         _logger.LogInformation("Gateway: Retrait de l'article {ArticleId} du panier de {UtilisateurId}",
//             articleId, utilisateurId);

//         var client = _httpClientFactory.CreateClient();
//         var url = $"{GetPanierApiUrl()}/api/panier/{utilisateurId}/articles/{articleId}";

//         try
//         {
//             var response = await client.DeleteAsync(url);
//             var content = await response.Content.ReadAsStringAsync();

//             if (response.IsSuccessStatusCode)
//                 return Ok(JsonSerializer.Deserialize<object>(content));

//             return StatusCode((int)response.StatusCode, content);
//         }
//         catch (Exception ex)
//         {
//             _logger.LogError(ex, "Erreur lors du retrait de l'article");
//             return StatusCode(500, new { message = "Erreur de communication avec le service Panier" });
//         }
//     }

//     // DELETE: api/panier/{utilisateurId}
//     [HttpDelete("panier/{utilisateurId}")]
//     public async Task<IActionResult> ViderPanier(string utilisateurId)
//     {
//         _logger.LogInformation("Gateway: Suppression du panier de {UtilisateurId}", utilisateurId);

//         var client = _httpClientFactory.CreateClient();
//         var url = $"{GetPanierApiUrl()}/api/panier/{utilisateurId}";

//         try
//         {
//             var response = await client.DeleteAsync(url);

//             if (response.IsSuccessStatusCode)
//                 return NoContent();

//             var content = await response.Content.ReadAsStringAsync();
//             return StatusCode((int)response.StatusCode, content);
//         }
//         catch (Exception ex)
//         {
//             _logger.LogError(ex, "Erreur lors de la suppression du panier");
//             return StatusCode(500, new { message = "Erreur de communication avec le service Panier" });
//         }
//     }

//     // GET: api/health
//     [HttpGet("health")]
//     public IActionResult VerifierSante()
//     {
//         return Ok(new
//         {
//             status = "healthy",
//             service = "Gateway.API",
//             timestamp = DateTime.UtcNow
//         });
//     }
// }



using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace Gateway.API.Controllers;

[ApiController]
[Route("api")]
public class GatewayController : ControllerBase
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _configuration;
    private readonly ILogger<GatewayController> _logger;

    public GatewayController(
        IHttpClientFactory httpClientFactory,
        IConfiguration configuration,
        ILogger<GatewayController> logger)
    {
        _httpClientFactory = httpClientFactory;
        _configuration = configuration;
        _logger = logger;
    }

    private string GetPanierApiUrl()
    {
        // Essayer variable d'environnement en premier (Railway)
        var url = Environment.GetEnvironmentVariable("Services__PanierApi");
        
        // Sinon essayer configuration
        if (string.IsNullOrEmpty(url))
        {
            url = _configuration["Services:PanierApi"];
        }
        
        // Fallback local
        if (string.IsNullOrEmpty(url))
        {
            _logger.LogWarning("⚠️ Aucune URL Panier API configurée, utilisation du fallback localhost");
            url = "http://localhost:5001";
        }
        
        // Vérifier que l'URL a un protocole
        if (!url.StartsWith("http://") && !url.StartsWith("https://"))
        {
            _logger.LogWarning("⚠️ URL sans protocole détectée: {Url}, ajout de http://", url);
            url = $"http://{url}";
        }
        
        _logger.LogInformation("🔗 URL Panier API utilisée: {Url}", url);
        
        return url;
    }

    // GET: api/panier/{utilisateurId}
    [HttpGet("panier/{utilisateurId}")]
    public async Task<IActionResult> ObtenirPanier(string utilisateurId)
    {
        _logger.LogInformation("Gateway: Récupération du panier pour {UtilisateurId}", utilisateurId);

        var client = _httpClientFactory.CreateClient();
        var baseUrl = GetPanierApiUrl();
        var url = $"{baseUrl}/api/panier/{utilisateurId}";
        
        _logger.LogInformation("📡 Requête GET vers: {Url}", url);

        try
        {
            var response = await client.GetAsync(url);
            var content = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("✅ Panier récupéré avec succès");
                return Ok(JsonSerializer.Deserialize<object>(content));
            }

            _logger.LogWarning("⚠️ Échec récupération panier: {StatusCode}", response.StatusCode);
            return StatusCode((int)response.StatusCode, content);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "❌ Erreur HTTP lors de la récupération du panier - URL: {Url}", url);
            return StatusCode(503, new { message = "Service Panier indisponible", detail = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Erreur lors de la récupération du panier");
            return StatusCode(500, new { message = "Erreur de communication avec le service Panier", detail = ex.Message });
        }
    }

    // POST: api/panier/{utilisateurId}/articles
    [HttpPost("panier/{utilisateurId}/articles")]
    public async Task<IActionResult> AjouterArticle(string utilisateurId, [FromBody] object article)
    {
        _logger.LogInformation("Gateway: Ajout d'un article au panier de {UtilisateurId}", utilisateurId);

        var client = _httpClientFactory.CreateClient();
        var baseUrl = GetPanierApiUrl();
        var url = $"{baseUrl}/api/panier/{utilisateurId}/articles";
        
        _logger.LogInformation("📡 Requête POST vers: {Url}", url);

        try
        {
            var json = JsonSerializer.Serialize(article);
            var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

            var response = await client.PostAsync(url, content);
            var responseContent = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("✅ Article ajouté avec succès");
                return Ok(JsonSerializer.Deserialize<object>(responseContent));
            }

            _logger.LogWarning("⚠️ Échec ajout article: {StatusCode}", response.StatusCode);
            return StatusCode((int)response.StatusCode, responseContent);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "❌ Erreur HTTP lors de l'ajout de l'article - URL: {Url}", url);
            return StatusCode(503, new { message = "Service Panier indisponible", detail = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Erreur lors de l'ajout de l'article");
            return StatusCode(500, new { message = "Erreur de communication avec le service Panier", detail = ex.Message });
        }
    }

    // DELETE: api/panier/{utilisateurId}/articles/{articleId}
    [HttpDelete("panier/{utilisateurId}/articles/{articleId}")]
    public async Task<IActionResult> RetirerArticle(string utilisateurId, string articleId)
    {
        _logger.LogInformation("Gateway: Retrait de l'article {ArticleId} du panier de {UtilisateurId}",
            articleId, utilisateurId);

        var client = _httpClientFactory.CreateClient();
        var baseUrl = GetPanierApiUrl();
        var url = $"{baseUrl}/api/panier/{utilisateurId}/articles/{articleId}";
        
        _logger.LogInformation("📡 Requête DELETE vers: {Url}", url);

        try
        {
            var response = await client.DeleteAsync(url);
            var content = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("✅ Article retiré avec succès");
                return Ok(JsonSerializer.Deserialize<object>(content));
            }

            _logger.LogWarning("⚠️ Échec retrait article: {StatusCode}", response.StatusCode);
            return StatusCode((int)response.StatusCode, content);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "❌ Erreur HTTP lors du retrait de l'article - URL: {Url}", url);
            return StatusCode(503, new { message = "Service Panier indisponible", detail = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Erreur lors du retrait de l'article");
            return StatusCode(500, new { message = "Erreur de communication avec le service Panier", detail = ex.Message });
        }
    }

    // DELETE: api/panier/{utilisateurId}
    [HttpDelete("panier/{utilisateurId}")]
    public async Task<IActionResult> ViderPanier(string utilisateurId)
    {
        _logger.LogInformation("Gateway: Suppression du panier de {UtilisateurId}", utilisateurId);

        var client = _httpClientFactory.CreateClient();
        var baseUrl = GetPanierApiUrl();
        var url = $"{baseUrl}/api/panier/{utilisateurId}";
        
        _logger.LogInformation("📡 Requête DELETE vers: {Url}", url);

        try
        {
            var response = await client.DeleteAsync(url);

            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("✅ Panier vidé avec succès");
                return NoContent();
            }

            var content = await response.Content.ReadAsStringAsync();
            _logger.LogWarning("⚠️ Échec vidage panier: {StatusCode}", response.StatusCode);
            return StatusCode((int)response.StatusCode, content);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "❌ Erreur HTTP lors de la suppression du panier - URL: {Url}", url);
            return StatusCode(503, new { message = "Service Panier indisponible", detail = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Erreur lors de la suppression du panier");
            return StatusCode(500, new { message = "Erreur de communication avec le service Panier", detail = ex.Message });
        }
    }

    // GET: api/health
    [HttpGet("health")]
    public IActionResult VerifierSante()
    {
        var panierApiUrl = GetPanierApiUrl();
        
        return Ok(new
        {
            status = "healthy",
            service = "Gateway.API",
            timestamp = DateTime.UtcNow,
            panierApiUrl = panierApiUrl
        });
    }
}