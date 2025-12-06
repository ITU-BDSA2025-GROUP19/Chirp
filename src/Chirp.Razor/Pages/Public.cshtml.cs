using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Chirp.Application.Interfaces;
using Chirp.Application.DTOs;
using Chirp.Infrastructure.Repositories;
using Microsoft.AspNetCore.Identity;

namespace Chirp.Razor.Pages;

public class PublicModel : PageModel
{
    private readonly CheepRepository _cheepRepository;
    private readonly UserManager<IdentityUser> _userManager;
    private readonly ICheepService _service;
    private readonly IConfiguration _configuration;
    private readonly IAiFactCheckService _ai;


    public List<CheepDto> Cheeps { get; set; } = new();
    public int CurrentPage { get; set; } = 1;

    public string? FactCheckResult { get; set; }
    public string? CheckedCheep { get; set; }

    [BindProperty]
    [Required]
    [StringLength(160, ErrorMessage = "Cheep must be max 160 characters")]
    public string Text { get; set; } = string.Empty;

    public PublicModel(
        ICheepService service,
        CheepRepository cheepRepository,
        UserManager<IdentityUser> userManager,
        IAiFactCheckService ai,
        IConfiguration configuration)
    {
        _service = service;
        _cheepRepository = cheepRepository;
        _userManager = userManager;
        _ai = ai;
        _configuration = configuration;
    }


    public async Task<ActionResult> OnGet([FromQuery] int page)
    {
        return await LoadPageAsync(page);
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!User.Identity?.IsAuthenticated ?? false)
            return Challenge();

        var user = await _userManager.GetUserAsync(User);
        if (user == null)
            return Forbid();

        await _cheepRepository.CreateCheepAsync(user.UserName ?? "Unknown", user.Email ?? "", Text);

        Text = ""; // clear box

        return await LoadPageAsync(CurrentPage);
    }

    public async Task<IActionResult> OnPostFactCheckAsync(string text)
    {
        CheckedCheep = text;
        FactCheckResult = await _ai.FactCheckAsync(text);
        return await LoadPageAsync(1);
    }
    private async Task<ActionResult> LoadPageAsync(int page)
    {
        if (page <= 0) page = 1;

        CurrentPage = page;
        Cheeps = await _service.GetCheeps(CurrentPage);

        return Page();
    }
}
