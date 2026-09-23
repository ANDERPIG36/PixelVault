using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using PixelVault.Models;
using PixelVault.Services;

namespace PixelVault.Pages.Inventory;

public class IndexModel : PageModel {
    private readonly InventoryService _inventoryService;
    private readonly GameService _gameService;
    private readonly PlatformService _platformService;
    private readonly ExpenseService _expenseService;

    public IndexModel(
        InventoryService inventoryService,
        GameService gameService,
        PlatformService platformService,
        ExpenseService expenseService) {

        _inventoryService = inventoryService;
        _gameService = gameService;
        _platformService = platformService;
        _expenseService = expenseService;
    }

    public List<InventoryItemDto> Items { get; set; } = new();
    public SelectList GameList { get; set; } = null!;
    public SelectList PlatformList { get; set; } = null!;

    [BindProperty]
    public OrderInputModel OrderInput { get; set; } = new();

    public class InventoryItemDto {
        public string Id { get; set; } = string.Empty;
        public string GameId { get; set; } = string.Empty;
        public string GameTitle { get; set; } = string.Empty;
        public string PlatformId { get; set; } = string.Empty;
        public string PlatformName { get; set; } = string.Empty;
        public int StockQuantity { get; set; }
        public bool IsLowStock => StockQuantity <= 5;
    }

    public class OrderInputModel {
        public string GameId { get; set; } = string.Empty;
        public string PlatformId { get; set; } = string.Empty;
        public int Quantity { get; set; } = 1;
        public decimal PurchaseUnitPrice { get; set; }
    }

    public async Task OnGetAsync(string? selectedGameId) {
        await LoadDataAsync();
        if (!string.IsNullOrEmpty(selectedGameId)) {
            OrderInput.GameId = selectedGameId;
        }
    }

    public async Task<IActionResult> OnPostOrderAsync() {
        if (OrderInput.Quantity <= 0 || OrderInput.PurchaseUnitPrice < 0) {
            ModelState.AddModelError("", "Quantità e prezzo di acquisto devono essere validi.");
            await LoadDataAsync();
            return Page();
        }

        // 1. Aggiorna o Crea lo Stock in Magazzino usando i metodi reali di InventoryService
        var existingItem = await _inventoryService.GetByGameAndPlatformAsync(OrderInput.GameId, OrderInput.PlatformId);

        if (existingItem != null && !string.IsNullOrEmpty(existingItem.Id)) {
            await _inventoryService.UpdateStockAsync(existingItem.Id, OrderInput.Quantity);
        }
        else {
            var newItem = new PixelVault.Models.Inventory {
                GameId = OrderInput.GameId,
                PlatformId = OrderInput.PlatformId,
                StockQuantity = OrderInput.Quantity,
                SoldQuantity = 0
            };
            await _inventoryService.CreateAsync(newItem);
        }

        // 2. Recupera informazioni per la voce di spesa
        var game = await _gameService.GetByIdAsync(OrderInput.GameId);
        var platform = await _platformService.GetByIdAsync(OrderInput.PlatformId);
        decimal totalExpense = OrderInput.Quantity * OrderInput.PurchaseUnitPrice;

        // 3. Registra automaticamente la Spesa
        var expense = new Expense {
            Description = $"Riordino magazzino: {game?.Title ?? "Gioco"} ({platform?.Name ?? "Piattaforma"}) x{OrderInput.Quantity}",
            Amount = totalExpense,
            Category = "Fornitori",
            Date = DateTime.UtcNow
        };
        await _expenseService.CreateAsync(expense);

        return RedirectToPage();
    }

    private async Task LoadDataAsync() {
        var rawItems = await _inventoryService.GetAllAsync();
        var games = (await _gameService.SearchDtosAsync()).ToDictionary(g => g.Id);
        var platforms = (await _platformService.GetAllAsync()).ToDictionary(p => p.Id!);

        Items = rawItems.Select(i => new InventoryItemDto {
            Id = i.Id ?? string.Empty,
            GameId = i.GameId,
            GameTitle = games.TryGetValue(i.GameId, out var g) ? g.Title : "N/D",
            PlatformId = i.PlatformId,
            PlatformName = platforms.TryGetValue(i.PlatformId, out var p) ? p.Name : "N/D",
            StockQuantity = i.StockQuantity
        }).ToList();

        GameList = new SelectList(games.Values, "Id", "Title");
        PlatformList = new SelectList(platforms.Values, "Id", "Name");
    }
}