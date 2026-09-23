using MongoDB.Driver;
using PixelVault.DTOs;
using PixelVault.Models;

namespace PixelVault.Services;

public class GameService {
    private readonly IMongoCollection<Game> _gamesCollection;
    private readonly IMongoCollection<Genre> _genresCollection;
    private readonly IMongoCollection<Developer> _developersCollection;
    private readonly IMongoCollection<Publisher> _publishersCollection;

    public GameService(IMongoDatabase database) {
        _gamesCollection = database.GetCollection<Game>("Games");
        _genresCollection = database.GetCollection<Genre>("Genres");
        _developersCollection = database.GetCollection<Developer>("Developers");
        _publishersCollection = database.GetCollection<Publisher>("Publishers");
    }

    public async Task<List<GameDTO>> SearchDtosAsync(
        string? titleQuery = null,
        string? genreId = null,
        string? developerId = null,
        string? publisherId = null) {

        var builder = Builders<Game>.Filter;
        var filter = builder.Empty;

        if (!string.IsNullOrWhiteSpace(titleQuery)) {
            filter &= builder.Regex(g => g.Title, new MongoDB.Bson.BsonRegularExpression(titleQuery, "i"));
        }

        if (!string.IsNullOrWhiteSpace(genreId)) {
            filter &= builder.AnyEq(g => g.GenreIds, genreId);
        }

        if (!string.IsNullOrWhiteSpace(developerId)) {
            filter &= builder.Eq(g => g.DeveloperId, developerId);
        }

        if (!string.IsNullOrWhiteSpace(publisherId)) {
            filter &= builder.Eq(g => g.PublisherId, publisherId);
        }

        var games = await _gamesCollection.Find(filter).ToListAsync();

        var genres = (await _genresCollection.Find(_ => true).ToListAsync()).ToDictionary(g => g.Id!);
        var developers = (await _developersCollection.Find(_ => true).ToListAsync()).ToDictionary(d => d.Id!);
        var publishers = (await _publishersCollection.Find(_ => true).ToListAsync()).ToDictionary(p => p.Id!);

        var dtos = new List<GameDTO>();

        foreach (var game in games) {
            var genreNames = game.GenreIds != null && game.GenreIds.Any()
                ? game.GenreIds
                    .Select(id => genres.TryGetValue(id, out var g) ? g.Name : null)
                    .Where(name => !string.IsNullOrEmpty(name))
                    .Cast<string>()
                    .ToList()
                : new List<string>();

            dtos.Add(new GameDTO {
                Id = game.Id ?? string.Empty,
                Title = game.Title,
                GenreNames = genreNames,
                DeveloperName = !string.IsNullOrEmpty(game.DeveloperId) && developers.TryGetValue(game.DeveloperId, out var d) ? d.Name : "N/D",
                PublisherName = !string.IsNullOrEmpty(game.PublisherId) && publishers.TryGetValue(game.PublisherId, out var p) ? p.Name : "N/D",
                ReleaseDate = game.ReleaseDate,
                Rating = game.Rating,
                Price = game.Price,
                Discount = game.Discount
            });
        }

        return dtos;
    }

    public async Task<List<GameDTO>> GetAllDtosAsync() => await SearchDtosAsync();

    public async Task<Game?> GetByIdAsync(string id) =>
        await _gamesCollection.Find(g => g.Id == id).FirstOrDefaultAsync();

    public async Task CreateAsync(Game newGame) =>
        await _gamesCollection.InsertOneAsync(newGame);

    public async Task UpdateAsync(string id, Game updatedGame) =>
        await _gamesCollection.ReplaceOneAsync(g => g.Id == id, updatedGame);

    public async Task DeleteAsync(string id) =>
        await _gamesCollection.DeleteOneAsync(g => g.Id == id);
}