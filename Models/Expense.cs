using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace PixelVault.Models;

public class Expense {
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }

    public string Description { get; set; } = string.Empty;

    public decimal Amount { get; set; }

    public string Category { get; set; } = string.Empty;

    public DateTime Date { get; set; } = DateTime.UtcNow;
}