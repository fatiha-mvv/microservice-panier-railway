namespace Panier.API.Models;

public class PanierUtilisateur
{
    public string UtilisateurId { get; set; } = string.Empty;
    public List<Article> Articles { get; set; } = new();
    public DateTime DerniereModification { get; set; } = DateTime.UtcNow;

    public decimal Total => Articles.Sum(a => a.Prix * a.Quantite);
    public int NombreArticles => Articles.Sum(a => a.Quantite);
}