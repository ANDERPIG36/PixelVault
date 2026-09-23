using MongoDB.Driver;
using PixelVault.Models;

namespace PixelVault.Services;

public class ExpenseService {
    private readonly IMongoCollection<Expense> _expensesCollection;

    public ExpenseService(IMongoDatabase database) {
        _expensesCollection = database.GetCollection<Expense>("Expenses");
    }

    public async Task<List<Expense>> GetAllAsync() =>
        await _expensesCollection.Find(_ => true).ToListAsync();

    public async Task CreateAsync(Expense expense) =>
        await _expensesCollection.InsertOneAsync(expense);
}