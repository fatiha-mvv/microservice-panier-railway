using Microsoft.AspNetCore.Mvc;
using Panier.API.Models;
using Panier.API.Services;

namespace Panier.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PanierController : ControllerBase
{
    private readonly RedisPanierService _panierService;
    private readonly ILogger<PanierController> _logger;

    public PanierController(RedisPanierService panierService, ILogger<PanierController> logger)
    {
        _panierService = panierService;
        _logger = logger;
    }

    // GET: api/panier/{utilisateurId}
    [HttpGet("{utilisateurId}")]
    public async Task<ActionResult<PanierUtilisateur>> ObtenirPanier(string utilisateurId)
    {
        _logger.LogInformation("Récupération du panier pour l'utilisateur {UtilisateurId}", utilisateurId);

        var panier = await _panierService.ObtenirPanierAsync(utilisateurId);

        if (panier == null)
            return NotFound(new { message = "Panier non trouvé" });

        return Ok(panier);
    }

    // POST: api/panier/{utilisateurId}/articles
    [HttpPost("{utilisateurId}/articles")]
    public async Task<ActionResult<PanierUtilisateur>> AjouterArticle(
        string utilisateurId,
        [FromBody] Article article)
    {
        _logger.LogInformation("Ajout de l'article {ArticleNom} au panier de {UtilisateurId}",
            article.Nom, utilisateurId);

        if (article.Quantite <= 0)
            return BadRequest(new { message = "La quantité doit être supérieure à 0" });

        var panier = await _panierService.AjouterArticleAsync(utilisateurId, article);
        return Ok(panier);
    }

    // DELETE: api/panier/{utilisateurId}/articles/{articleId}
    [HttpDelete("{utilisateurId}/articles/{articleId}")]
    public async Task<ActionResult<PanierUtilisateur>> RetirerArticle(
        string utilisateurId,
        string articleId)
    {
        _logger.LogInformation("Retrait de l'article {ArticleId} du panier de {UtilisateurId}",
            articleId, utilisateurId);

        var panier = await _panierService.RetirerArticleAsync(utilisateurId, articleId);

        if (panier == null)
            return NotFound(new { message = "Panier non trouvé" });

        return Ok(panier);
    }

    // DELETE: api/panier/{utilisateurId}
    [HttpDelete("{utilisateurId}")]
    public async Task<ActionResult> ViderPanier(string utilisateurId)
    {
        _logger.LogInformation("Suppression du panier de {UtilisateurId}", utilisateurId);

        var supprime = await _panierService.SupprimerPanierAsync(utilisateurId);

        if (!supprime)
            return NotFound(new { message = "Panier non trouvé" });

        return NoContent();
    }

    // GET: api/panier/health
    [HttpGet("health")]
    public ActionResult VerifierSante()
    {
        return Ok(new { status = "healthy", service = "Panier.API" });
    }
}