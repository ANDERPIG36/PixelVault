using MongoDB.Driver;
using PixelVault.Models;

namespace PixelVault.Services;

public class InventoryService {
    private readonly IMongoCollection<Inventory> _inventoryCollection;

    public InventoryService(IMongoDatabase database) {
        _inventoryCollection = database.GetCollection<Inventory>("Inventories");
    }

    public async Task<List<Inventory>> GetAllAsync() =>
        await _inventoryCollection.Find(_ => true).ToListAsync();

    public async Task<Inventory?> GetByGameAndPlatformAsync(string gameId, string platformId) =>
        await _inventoryCollection
            .Find(i => i.GameId == gameId && i.PlatformId == platformId)
            .FirstOrDefaultAsync();

    public async Task CreateAsync(Inventory inventory) =>
        await _inventoryCollection.InsertOneAsync(inventory);

    public async Task UpdateStockAsync(string id, int addedStock) {
        var filter = Builders<Inventory>.Filter.Eq(i => i.Id, id);
        var update = Builders<Inventory>.Update.Inc(i => i.StockQuantity, addedStock);
        await _inventoryCollection.UpdateOneAsync(filter, update);
    }
}