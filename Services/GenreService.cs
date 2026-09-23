using MongoDB.Driver;
using PixelVault.Models;

namespace PixelVault.Services;

public class GenreService {
    private readonly IMongoCollection<Genre> _genresCollection;

    public GenreService(IMongoDatabase database) {
        _genresCollection = database.GetCollection<Genre>("Genres");
    }

    public async Task<List<Genre>> GetAllAsync() =>
        await _genresCollection.Find(_ => true).ToListAsync();

    public async Task<Genre?> GetByIdAsync(string id) =>
        await _genresCollection.Find(g => g.Id == id).FirstOrDefaultAsync();

    public async Task CreateAsync(Genre genre) =>
        await _genresCollection.InsertOneAsync(genre);

    public async Task DeleteAsync(string id) =>
        await _genresCollection.DeleteOneAsync(g => g.Id == id);
}