using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using PixelVault.DTOs;
using PixelVault.Services;

namespace PixelVault.Pages.Games;

public class IndexModel : PageModel {
    private readonly GameService _gameService;
    private readonly GenreService _genreService;
    private readonly DeveloperService _developerService;
    private readonly PublisherService _publisherService;

    public IndexModel(
        GameService gameService,
        GenreService genreService,
        DeveloperService developerService,
        PublisherService publisherService) {

        _gameService = gameService;
        _genreService = genreService;
        _developerService = developerService;
        _publisherService = publisherService;
    }

    public List<GameDTO> Games { get; set; } = new();

    [BindProperty(SupportsGet = true)]
    public string? SearchTitle { get; set; }

    [BindProperty(SupportsGet = true)]
    public string? SelectedGenreId { get; set; }

    [BindProperty(SupportsGet = true)]
    public string? SelectedDeveloperId { get; set; }

    [BindProperty(SupportsGet = true)]
    public string? SelectedPublisherId { get; set; }

    public SelectList GenresList { get; set; } = null!;
    public SelectList DevelopersList { get; set; } = null!;
    public SelectList PublishersList { get; set; } = null!;

    public async Task OnGetAsync() {
        var genres = await _genreService.GetAllAsync();
        var developers = await _developerService.GetAllAsync();
        var publishers = await _publisherService.GetAllAsync();

        GenresList = new SelectList(genres, "Id", "Name");
        DevelopersList = new SelectList(developers, "Id", "Name");
        PublishersList = new SelectList(publishers, "Id", "Name");

        Games = await _gameService.SearchDtosAsync(SearchTitle, SelectedGenreId, SelectedDeveloperId, SelectedPublisherId);
    }
    public async Task<IActionResult> OnPostDeleteAsync(string id) {
        await _gameService.DeleteAsync(id);
        return RedirectToPage();
    }
}