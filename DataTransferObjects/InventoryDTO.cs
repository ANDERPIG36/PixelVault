namespace PixelVault.DTOs;

public class InventoryDto {
    public string Id { get; set; } = string.Empty;
    public string GameId { get; set; } = string.Empty;
    public string GameTitle { get; set; } = "N/D";
    public string PlatformId { get; set; } = string.Empty;
    public string PlatformName { get; set; } = "N/D";
    public int StockQuantity { get; set; }
    public int SoldQuantity { get; set; }
}