using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Chirp.Application.Interfaces;
using Chirp.Application.DTOs;
using Chirp.Infrastructure.Repositories;
using Microsoft.AspNetCore.Identity;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Chirp.Razor.Pages;

public class PublicModel : PageModel
{

    private readonly CheepRepository _cheepRepository;
    private readonly UserManager<IdentityUser> _userManager;
    private readonly ICheepService _service;
    public List<CheepDto> Cheeps { get; set; } = new();
    public int CurrentPage { get; set; } = 1;

    [BindProperty]
    public string Text { get; set; } = string.Empty;

    public PublicModel(ICheepService service, CheepRepository cheepRepository, UserManager<IdentityUser> userManager)
    {
        _service = service;
        _cheepRepository = cheepRepository;
        _userManager = userManager;
    }

    public async Task<ActionResult> OnGet([FromQuery] int page)
    {
        if (page == 0) page = 1;

        CurrentPage = page;

        Cheeps = await _service.GetCheeps(CurrentPage);
        return Page();
    }
    
    public async Task<IActionResult> OnPostAsync()
    {
        if (!User.Identity.IsAuthenticated)
        {
            return Challenge(); //Goes to login page, just extra protection, probably wont ever get called
        }

        var user = await _userManager.GetUserAsync(User);

        if (user == null) {
            return Forbid(); //Shows access denied if user is null
        }

        await _cheepRepository.CreateCheepAsync(user.UserName ?? "Unknown", user.Email ?? "", Text);

        return RedirectToPage();
    }
}
