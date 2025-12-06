namespace Panier.API.Models;

public class Article
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Nom { get; set; } = string.Empty;
    public decimal Prix { get; set; }
    public int Quantite { get; set; }
    public DateTime AjouteLe { get; set; } = DateTime.UtcNow;
}