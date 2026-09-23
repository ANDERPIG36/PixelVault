using MongoDB.Driver;
using PixelVault.Models;

namespace PixelVault.Services;

public class PlatformService {
    private readonly IMongoCollection<Platform> _platformsCollection;

    public PlatformService(IMongoDatabase database) {
        _platformsCollection = database.GetCollection<Platform>("Platforms");
    }

    public async Task<List<Platform>> GetAllAsync() =>
        await _platformsCollection.Find(_ => true).ToListAsync();

    public async Task<Platform?> GetByIdAsync(string id) =>
        await _platformsCollection.Find(p => p.Id == id).FirstOrDefaultAsync();

    public async Task CreateAsync(Platform platform) =>
        await _platformsCollection.InsertOneAsync(platform);

    public async Task DeleteAsync(string id) =>
        await _platformsCollection.DeleteOneAsync(p => p.Id == id);
}