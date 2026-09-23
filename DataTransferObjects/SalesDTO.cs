namespace PixelVault.DTOs;

public class SaleDto {
    public string Id { get; set; } = string.Empty;
    public string GameTitle { get; set; } = "N/D";
    public string PlatformName { get; set; } = "N/D";
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal DiscountApplied { get; set; }
    public decimal TotalAmount { get; set; }
    public DateTime SaleDate { get; set; }
}