using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PixelVault.Models;
using PixelVault.Services;

namespace PixelVault.Pages.Publishers;

public class IndexModel : PageModel {
    private readonly PublisherService _publisherService;

    public IndexModel(PublisherService publisherService) {
        _publisherService = publisherService;
    }

    public List<Publisher> Publishers { get; set; } = new();

    [BindProperty]
    public Publisher NewPublisher { get; set; } = new();

    public async Task OnGetAsync() {
        Publishers = await _publisherService.GetAllAsync();
    }

    public async Task<IActionResult> OnPostAsync() {
        if (!ModelState.IsValid) {
            Publishers = await _publisherService.GetAllAsync();
            return Page();
        }

        await _publisherService.CreateAsync(NewPublisher);
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostDeleteAsync(string id) {
        await _publisherService.DeleteAsync(id);
        return RedirectToPage();
    }
}