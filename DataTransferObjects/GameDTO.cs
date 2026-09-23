namespace PixelVault.DTOs;

public class GameDTO {
    public string Id { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public List<string> GenreNames { get; set; } = new();
    public string DeveloperName { get; set; } = "N/D";
    public string PublisherName { get; set; } = "N/D";
    public DateTime ReleaseDate { get; set; }
    public double Rating { get; set; }
    public decimal Price { get; set; }
    public decimal Discount { get; set; }
    public decimal FinalPrice => Price - (Price * (Discount / 100m));
}