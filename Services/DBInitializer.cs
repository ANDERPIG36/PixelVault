using MongoDB.Driver;
using PixelVault.Models;

namespace PixelVault.Services;

public static class DbInitializer {
    public static async Task InitializeIndexesAsync(IMongoDatabase database) {
        
        var gamesCollection = database.GetCollection<Game>("Games");
        
        var gameTitleIndex = new CreateIndexModel<Game>(
            Builders<Game>.IndexKeys.Ascending(g => g.Title)
        );

        var gameGenreIndex = new CreateIndexModel<Game>(
            Builders<Game>.IndexKeys.Ascending(g => g.GenreIds)
        );
        
        var gameDevIndex = new CreateIndexModel<Game>(
            Builders<Game>.IndexKeys.Ascending(g => g.DeveloperId)
        );

        await gamesCollection.Indexes.CreateManyAsync(new[] { gameTitleIndex, gameGenreIndex, gameDevIndex });

        var inventoryCollection = database.GetCollection<Inventory>("Inventories");
        var inventoryIndexOptions = new CreateIndexOptions { Unique = true };
        var inventoryCompoundIndex = new CreateIndexModel<Inventory>(
            Builders<Inventory>.IndexKeys
                .Ascending(i => i.GameId)
                .Ascending(i => i.PlatformId),
            inventoryIndexOptions
        );

        await inventoryCollection.Indexes.CreateOneAsync(inventoryCompoundIndex);

        var salesCollection = database.GetCollection<Sale>("Sales");
        var saleDateIndex = new CreateIndexModel<Sale>(
            Builders<Sale>.IndexKeys.Descending(s => s.SaleDate)
        );

        await salesCollection.Indexes.CreateOneAsync(saleDateIndex);
    }
}