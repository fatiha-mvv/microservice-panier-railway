using System.Text.Json;
using StackExchange.Redis;
using Panier.API.Models;

namespace Panier.API.Services;

public class RedisPanierService
{
    private readonly IConnectionMultiplexer _redis;
    private readonly IDatabase _database;

    public RedisPanierService(IConnectionMultiplexer redis)
    {
        _redis = redis;
        _database = redis.GetDatabase();
    }

    // Récupérer le panier d'un utilisateur
    public async Task<PanierUtilisateur?> ObtenirPanierAsync(string utilisateurId)
    {
        var data = await _database.StringGetAsync($"panier:{utilisateurId}");

        if (data.IsNullOrEmpty)
            return null;

        return JsonSerializer.Deserialize<PanierUtilisateur>(data!);
    }

    // Sauvegarder le panier
    public async Task<bool> SauvegarderPanierAsync(PanierUtilisateur panier)
    {
        panier.DerniereModification = DateTime.UtcNow;
        var json = JsonSerializer.Serialize(panier);

        // Expire après 7 jours d'inactivité
        return await _database.StringSetAsync(
            $"panier:{panier.UtilisateurId}",
            json,
            TimeSpan.FromDays(7)
        );
    }

    // Supprimer le panier
    public async Task<bool> SupprimerPanierAsync(string utilisateurId)
    {
        return await _database.KeyDeleteAsync($"panier:{utilisateurId}");
    }

    // Ajouter un article au panier
    public async Task<PanierUtilisateur> AjouterArticleAsync(string utilisateurId, Article article)
    {
        var panier = await ObtenirPanierAsync(utilisateurId) ?? new PanierUtilisateur
        {
            UtilisateurId = utilisateurId
        };

        // Vérifier si l'article existe déjà
        var articleExistant = panier.Articles.FirstOrDefault(a => a.Nom == article.Nom);

        if (articleExistant != null)
        {
            // Augmenter la quantité
            articleExistant.Quantite += article.Quantite;
        }
        else
        {
            // Ajouter le nouvel article
            panier.Articles.Add(article);
        }

        await SauvegarderPanierAsync(panier);
        return panier;
    }

    // Retirer un article du panier
    public async Task<PanierUtilisateur?> RetirerArticleAsync(string utilisateurId, string articleId)
    {
        var panier = await ObtenirPanierAsync(utilisateurId);

        if (panier == null)
            return null;

        panier.Articles.RemoveAll(a => a.Id == articleId);
        await SauvegarderPanierAsync(panier);

        return panier;
    }
}