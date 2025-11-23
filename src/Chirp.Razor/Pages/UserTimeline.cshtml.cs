using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Chirp.Application.Interfaces;
using Chirp.Application.DTOs;
using Chirp.Infrastructure.Repositories;
using System.ComponentModel.DataAnnotations;

namespace Chirp.Razor.Pages;

public class UserTimelineModel : PageModel
{
    private readonly ICheepService _service;

    private readonly CheepRepository _cheepRepository;
    private readonly UserManager<IdentityUser> _userManager;

    public List<CheepDto> Cheeps { get; set; } = new();
    public int CurrentPage { get; set; } = 1;
    public string Author { get; set; } = "";

    [BindProperty]
    [Required]
    [StringLength(160, ErrorMessage = "Cheep must be max 160 characters")]
    public string Text { get; set; } = string.Empty;

    public UserTimelineModel(ICheepService service, UserManager<IdentityUser> userManager, CheepRepository cheeprepository)
    {
        _service = service;
        _cheepRepository = cheeprepository;
        _userManager = userManager;
    }

    public async Task<ActionResult> OnGet(string author, [FromQuery] int page)
    {
        if (page == 0) page = 1;
        CurrentPage = page;
        Author = author;

        var user = await _userManager.GetUserAsync(User);

        if (user != null)
        {
            // Logged-in user – show their private timeline if matching
            var displayName = author?.ToLowerInvariant();
            if (!string.IsNullOrEmpty(displayName))
            {
                Cheeps = await _service.GetCheepsByUserId(user.Id, page);

            }
        }
        else
        {
            // Public/other user's timeline
            Cheeps = await _service.GetCheepsByAuthor(Author, page);
        }
        return Page();

    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!User.Identity.IsAuthenticated)
        {
            return Challenge();
        }

        var user = await _userManager.GetUserAsync(User);

        if (user == null) {
            return Forbid();
        }

        await _cheepRepository.CreateCheepAsync(user.UserName ?? "Unknown", user.Email ?? "", Text);

        return RedirectToPage();
}
}