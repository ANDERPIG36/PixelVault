using MongoDB.Driver;
using PixelVault.Models;

namespace PixelVault.Services;

public class SaleService {
    private readonly IMongoCollection<Sale> _salesCollection;
    private readonly IMongoCollection<Inventory> _inventoryCollection;

    public SaleService(IMongoDatabase database) {
        _salesCollection = database.GetCollection<Sale>("Sales");
        _inventoryCollection = database.GetCollection<Inventory>("Inventories");
    }

    public async Task RegisterSaleAsync(Sale sale) {
        var inventoryFilter = Builders<Inventory>.Filter.And(
            Builders<Inventory>.Filter.Eq(i => i.GameId, sale.GameId),
            Builders<Inventory>.Filter.Eq(i => i.PlatformId, sale.PlatformId)
        );

        var inventory = await _inventoryCollection.Find(inventoryFilter).FirstOrDefaultAsync();

        if (inventory == null) {
            throw new InvalidOperationException("Impossibile completare la vendita: il gioco non è presente in magazzino per la piattaforma selezionata.");
        }

        if (inventory.StockQuantity < sale.Quantity) {
            throw new InvalidOperationException($"Quantità insufficiente in magazzino! Disponibili: {inventory.StockQuantity}, Richieste: {sale.Quantity}.");
        }

        await _salesCollection.InsertOneAsync(sale);

        var update = Builders<Inventory>.Update
            .Inc(i => i.StockQuantity, -sale.Quantity)
            .Inc(i => i.SoldQuantity, sale.Quantity);

        await _inventoryCollection.UpdateOneAsync(inventoryFilter, update);
    }

    public async Task<List<Sale>> GetAllAsync() =>
        await _salesCollection.Find(_ => true).ToListAsync();
}