using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PixelVault.Models;
using PixelVault.Services;

namespace PixelVault.Pages.Developers;

public class IndexModel : PageModel {
    
    private readonly DeveloperService _developerService;

    public IndexModel(DeveloperService developerService) {
        _developerService = developerService;
    }

    public List<Developer> Developers { get; set; } = new();

    [BindProperty]
    public Developer NewDeveloper { get; set; } = new();

    public async Task OnGetAsync() {
        Developers = await _developerService.GetAllAsync();
    }

    public async Task<IActionResult> OnPostAsync() {
        if (!ModelState.IsValid) {
            Developers = await _developerService.GetAllAsync();
            return Page();
        }

        await _developerService.CreateAsync(NewDeveloper);
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostDeleteAsync(string id) {
        await _developerService.DeleteAsync(id);
        return RedirectToPage();
    }
}