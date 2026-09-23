using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PixelVault.Models;
using PixelVault.Services;

namespace PixelVault.Pages.ExpensesPage;

public class IndexModel : PageModel {
    private readonly ExpenseService _expenseService;

    public IndexModel(ExpenseService expenseService) {
        _expenseService = expenseService;
    }

    public List<Expense> ExpensesList { get; set; } = new();

    [BindProperty]
    public Expense NewExpense { get; set; } = new() { Date = DateTime.Today };

    public decimal TotalExpensesSum { get; set; }

    public async Task OnGetAsync() {
        await LoadDataAsync();
    }

    public async Task<IActionResult> OnPostAsync() {
        if (!ModelState.IsValid) {
            await LoadDataAsync();
            return Page();
        }

        NewExpense.Date = DateTime.SpecifyKind(NewExpense.Date, DateTimeKind.Utc);
        await _expenseService.CreateAsync(NewExpense);

        return RedirectToPage();
    }

    private async Task LoadDataAsync() {
        ExpensesList = (await _expenseService.GetAllAsync())
            .OrderByDescending(e => e.Date)
            .ToList();

        TotalExpensesSum = ExpensesList.Sum(e => e.Amount);
    }
}