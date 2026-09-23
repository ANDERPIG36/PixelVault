using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using PixelVault.DTOs;
using PixelVault.Models;
using PixelVault.Services;

namespace PixelVault.Pages.SalesPage;

public class IndexModel : PageModel {
    private readonly SaleService _saleService;
    private readonly GameService _gameService;
    private readonly PlatformService _platformService;

    public IndexModel(
        SaleService saleService,
        GameService gameService,
        PlatformService platformService) {

        _saleService = saleService;
        _gameService = gameService;
        _platformService = platformService;
    }

    public List<SaleDto> SalesList { get; set; } = new();

    [BindProperty]
    public Sale NewSale { get; set; } = new() { Quantity = 1 };

    public SelectList GamesList { get; set; } = null!;
    public SelectList PlatformsList { get; set; } = null!;

    [TempData]
    public string? ErrorMessage { get; set; }

    [TempData]
    public string? SuccessMessage { get; set; }

    public async Task OnGetAsync() {
        await LoadDataAsync();
    }

    public async Task<IActionResult> OnPostAsync() {
        // Recupera le informazioni del gioco per applicare prezzo e sconto corretti
        var game = await _gameService.GetByIdAsync(NewSale.GameId);
        if (game == null) {
            ErrorMessage = "Gioco selezionato non valido.";
            return RedirectToPage();
        }

        NewSale.UnitPrice = game.Price;
        NewSale.DiscountApplied = game.Discount;

        // Calcolo totale: (Prezzo * (1 - Sconto/100)) * Quantità
        decimal discountedPrice = game.Price - (game.Price * (game.Discount / 100m));
        NewSale.TotalAmount = discountedPrice * NewSale.Quantity;
        NewSale.SaleDate = DateTime.UtcNow;

        try {
            await _saleService.RegisterSaleAsync(NewSale);
            SuccessMessage = $"Vendita registrata con successo! Totale: € {NewSale.TotalAmount:0.00}";
        }
        catch (InvalidOperationException ex) {
            ErrorMessage = ex.Message;
        }

        return RedirectToPage();
    }

    private async Task LoadDataAsync() {
        var rawSales = await _saleService.GetAllAsync();
        var games = (await _gameService.SearchDtosAsync()).ToDictionary(g => g.Id);
        var platforms = (await _platformService.GetAllAsync()).ToDictionary(p => p.Id!);

        GamesList = new SelectList(games.Values, "Id", "Title");
        PlatformsList = new SelectList(platforms.Values, "Id", "Name");

        SalesList = rawSales.Select(s => new SaleDto {
            Id = s.Id ?? string.Empty,
            GameTitle = games.TryGetValue(s.GameId, out var g) ? g.Title : "N/D",
            PlatformName = platforms.TryGetValue(s.PlatformId, out var p) ? p.Name : "N/D",
            Quantity = s.Quantity,
            UnitPrice = s.UnitPrice,
            DiscountApplied = s.DiscountApplied,
            TotalAmount = s.TotalAmount,
            SaleDate = s.SaleDate
        }).OrderByDescending(s => s.SaleDate).ToList();
    }
}