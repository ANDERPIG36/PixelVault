using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace PixelVault.Models;

public class Sale {
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }

    [BsonRepresentation(BsonType.ObjectId)]
    public string GameId { get; set; } = string.Empty;

    [BsonRepresentation(BsonType.ObjectId)]
    public string PlatformId { get; set; } = string.Empty;

    public int Quantity { get; set; }

    public decimal UnitPrice { get; set; }

    public decimal DiscountApplied { get; set; }

    public decimal TotalAmount { get; set; }

    public DateTime SaleDate { get; set; } = DateTime.UtcNow;
}