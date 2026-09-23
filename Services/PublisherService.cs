using MongoDB.Driver;
using PixelVault.Models;

namespace PixelVault.Services;

public class PublisherService {
    private readonly IMongoCollection<Publisher> _publishersCollection;

    public PublisherService(IMongoDatabase database) {
        _publishersCollection = database.GetCollection<Publisher>("Publishers");
    }

    public async Task<List<Publisher>> GetAllAsync() =>
        await _publishersCollection.Find(_ => true).ToListAsync();

    public async Task<Publisher?> GetByIdAsync(string id) =>
        await _publishersCollection.Find(p => p.Id == id).FirstOrDefaultAsync();

    public async Task CreateAsync(Publisher publisher) =>
        await _publishersCollection.InsertOneAsync(publisher);

    public async Task DeleteAsync(string id) =>
        await _publishersCollection.DeleteOneAsync(p => p.Id == id);
}