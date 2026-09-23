using MongoDB.Driver;
using PixelVault.Models;

namespace PixelVault.Services;

public class StatisticsSummary {
    public decimal TotalRevenue { get; set; }
    public decimal TotalExpenses { get; set; }
    public decimal NetProfit => TotalRevenue - TotalExpenses;
    public int TotalCopiesSold { get; set; }
    public int TotalGamesInCatalog { get; set; }
}

public class StatisticsService {
    private readonly IMongoCollection<Sale> _salesCollection;
    private readonly IMongoCollection<Expense> _expensesCollection;
    private readonly IMongoCollection<Game> _gamesCollection;
    private readonly IMongoCollection<Inventory> _inventoryCollection;

    public StatisticsService(IMongoDatabase database) {
        _salesCollection = database.GetCollection<Sale>("Sales");
        _expensesCollection = database.GetCollection<Expense>("Expenses");
        _gamesCollection = database.GetCollection<Game>("Games");
        _inventoryCollection = database.GetCollection<Inventory>("Inventories");
    }

    public async Task<StatisticsSummary> GetSummaryAsync() {
        var sales = await _salesCollection.Find(_ => true).ToListAsync();
        var expenses = await _expensesCollection.Find(_ => true).ToListAsync();
        var gamesCount = await _gamesCollection.CountDocumentsAsync(_ => true);

        return new StatisticsSummary {
            TotalRevenue = sales.Sum(s => s.TotalAmount),
            TotalExpenses = expenses.Sum(e => e.Amount),
            TotalCopiesSold = sales.Sum(s => s.Quantity),
            TotalGamesInCatalog = (int)gamesCount
        };
    }

    public async Task<List<Inventory>> GetLowStockItemsAsync(int threshold = 5) {
        return await _inventoryCollection
            .Find(i => i.StockQuantity <= threshold)
            .ToListAsync();
    }
}