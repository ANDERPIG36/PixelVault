using Microsoft.AspNetCore.Mvc.RazorPages;
using PixelVault.DTOs;
using PixelVault.Services;

namespace PixelVault.Pages;

public class IndexModel : PageModel {
    private readonly StatisticsService _statisticsService;
    private readonly GameService _gameService;
    private readonly PlatformService _platformService;

    public IndexModel(
        StatisticsService statisticsService,
        GameService gameService,
        PlatformService platformService) {
        _statisticsService = statisticsService;
        _gameService = gameService;
        _platformService = platformService;
    }

    public StatisticsSummary Summary { get; set; } = new();
    public List<InventoryDto> LowStockItems { get; set; } = new();

    public async Task OnGetAsync() {
        Summary = await _statisticsService.GetSummaryAsync();

        // Recupera gli elementi sotto-scorta e mappa con i DTO
        var lowStock = await _statisticsService.GetLowStockItemsAsync(5);
        var games = (await _gameService.SearchDtosAsync()).ToDictionary(g => g.Id);
        var platforms = (await _platformService.GetAllAsync()).ToDictionary(p => p.Id!);

        LowStockItems = lowStock.Select(i => new InventoryDto {
            Id = i.Id ?? string.Empty,
            GameTitle = games.TryGetValue(i.GameId, out var g) ? g.Title : "N/D",
            PlatformName = platforms.TryGetValue(i.PlatformId, out var p) ? p.Name : "N/D",
            StockQuantity = i.StockQuantity
        }).ToList();
    }
}