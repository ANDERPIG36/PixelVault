using MongoDB.Driver;
using PixelVault.Models;

namespace PixelVault.Services;

public class DeveloperService {
    private readonly IMongoCollection<Developer> _developersCollection;

    public DeveloperService(IMongoDatabase database) {
        _developersCollection = database.GetCollection<Developer>("Developers");
    }

    public async Task<List<Developer>> GetAllAsync() =>
        await _developersCollection.Find(_ => true).ToListAsync();

    public async Task<Developer?> GetByIdAsync(string id) =>
        await _developersCollection.Find(d => d.Id == id).FirstOrDefaultAsync();

    public async Task CreateAsync(Developer developer) =>
        await _developersCollection.InsertOneAsync(developer);

    public async Task DeleteAsync(string id) =>
        await _developersCollection.DeleteOneAsync(d => d.Id == id);
}