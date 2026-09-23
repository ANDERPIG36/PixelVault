using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace PixelVault.Models;

public class Inventory {
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }

    [BsonRepresentation(BsonType.ObjectId)]
    public string GameId { get; set; } = string.Empty;

    [BsonRepresentation(BsonType.ObjectId)]
    public string PlatformId { get; set; } = string.Empty;

    public int StockQuantity { get; set; }

    public int SoldQuantity { get; set; }
}