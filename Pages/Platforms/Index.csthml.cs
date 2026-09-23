using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PixelVault.Models;
using PixelVault.Services;

namespace PixelVault.Pages.Platforms;

public class IndexModel : PageModel {
    
    private readonly PlatformService _platformService;

    public IndexModel(PlatformService platformService) {
        _platformService = platformService;
    }

    public List<Platform> Platforms { get; set; } = new();

    [BindProperty]
    public Platform NewPlatform { get; set; } = new();

    public async Task OnGetAsync() {
        Platforms = await _platformService.GetAllAsync();
    }

    public async Task<IActionResult> OnPostAsync() {
        if (!ModelState.IsValid) {
            Platforms = await _platformService.GetAllAsync();
            return Page();
        }

        await _platformService.CreateAsync(NewPlatform);
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostDeleteAsync(string id) {
        await _platformService.DeleteAsync(id);
        return RedirectToPage();
    }
}