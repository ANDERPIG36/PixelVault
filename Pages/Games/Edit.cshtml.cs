using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using PixelVault.Models;
using PixelVault.Services;

namespace PixelVault.Pages.Games;

public class EditModel : PageModel {
    private readonly GameService _gameService;
    private readonly GenreService _genreService;
    private readonly DeveloperService _developerService;
    private readonly PublisherService _publisherService;

    public EditModel(
        GameService gameService,
        GenreService genreService,
        DeveloperService developerService,
        PublisherService publisherService) {

        _gameService = gameService;
        _genreService = genreService;
        _developerService = developerService;
        _publisherService = publisherService;
    }

    [BindProperty]
    public Game Game { get; set; } = new();

    [BindProperty]
    public List<string> SelectedGenreIds { get; set; } = new();

    public SelectList GenresList { get; set; } = null!;
    public SelectList DevelopersList { get; set; } = null!;
    public SelectList PublishersList { get; set; } = null!;

    public async Task<IActionResult> OnGetAsync(string id) {
        var game = await _gameService.GetByIdAsync(id);
        if (game == null) {
            return NotFound();
        }

        Game = game;
        SelectedGenreIds = game.GenreIds ?? new List<string>();

        await LoadDropdownsAsync();
        return Page();
    }

    public async Task<IActionResult> OnPostAsync() {
        if (!ModelState.IsValid) {
            await LoadDropdownsAsync();
            return Page();
        }

        Game.GenreIds = SelectedGenreIds;
        await _gameService.UpdateAsync(Game.Id!, Game);

        return RedirectToPage("Index");
    }

    private async Task LoadDropdownsAsync() {
        var genres = await _genreService.GetAllAsync();
        var developers = await _developerService.GetAllAsync();
        var publishers = await _publisherService.GetAllAsync();

        GenresList = new SelectList(genres, "Id", "Name");
        DevelopersList = new SelectList(developers, "Id", "Name");
        PublishersList = new SelectList(publishers, "Id", "Name");
    }
}