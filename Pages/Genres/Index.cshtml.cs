using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PixelVault.Models;
using PixelVault.Services;

namespace PixelVault.Pages.Genres;

public class IndexModel : PageModel {
    private readonly GenreService _genreService;

    public IndexModel(GenreService genreService) {
        _genreService = genreService;
    }

    public List<Genre> Genres { get; set; } = new();

    [BindProperty]
    public Genre NewGenre { get; set; } = new();

    public async Task OnGetAsync() {
        Genres = await _genreService.GetAllAsync();
    }

    public async Task<IActionResult> OnPostAsync() {
        if (!ModelState.IsValid) {
            Genres = await _genreService.GetAllAsync();
            return Page();
        }

        await _genreService.CreateAsync(NewGenre);
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostDeleteAsync(string id) {
        await _genreService.DeleteAsync(id);
        return RedirectToPage();
    }
}