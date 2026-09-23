using System.ComponentModel.DataAnnotations;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace PixelVault.Models;

public class Game {
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }

    public string Title { get; set; } = string.Empty;

    [BsonRepresentation(BsonType.ObjectId)]
    public List<string> GenreIds { get; set; } = new();

    [BsonRepresentation(BsonType.ObjectId)]
    public string DeveloperId { get; set; } = string.Empty;

    [BsonRepresentation(BsonType.ObjectId)]
    public string PublisherId { get; set; } = string.Empty;

    [DataType(DataType.Date)]
    public DateTime ReleaseDate { get; set; }

    [Range(1.0, 10.0, ErrorMessage = "La valutazione deve essere compresa tra 1 e 10.")]
    public double Rating { get; set; } = 1.0;

    public decimal Price { get; set; }

    [Range(0, 100, ErrorMessage = "Lo sconto deve essere compreso tra 0 e 100%")]
    public int Discount { get; set; }
}