using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Chirp.Application.Interfaces;
using Chirp.Application.DTOs;
using Chirp.Infrastructure.Repositories;

namespace Chirp.Razor.Pages;

public class UserTimelineModel : PageModel
{
    private readonly ICheepService _service;
    private readonly CheepRepository _cheepRepository;
    private readonly AuthorRepository _authorRepository;
    private readonly UserManager<IdentityUser> _userManager;

    public List<CheepDto> Cheeps { get; set; } = new();
    public int CurrentPage { get; set; } = 1;
    public string Author { get; set; } = "";

    [BindProperty]
    public string Text { get; set; } = string.Empty;

    public UserTimelineModel(
        ICheepService service,
        UserManager<IdentityUser> userManager,
        CheepRepository cheepRepository,
        AuthorRepository authorRepository)
    {
        _service = service;
        _userManager = userManager;
        _cheepRepository = cheepRepository;
        _authorRepository = authorRepository;
    }

    public async Task<ActionResult> OnGet(string author, [FromQuery] int page)
    {
        if (page <= 0) page = 1;
        CurrentPage = page;
        Author = author;

        // The profile visiting
        var profileAuthor = await _authorRepository.GetAuthorByNameAsync(author);
        if (profileAuthor == null)
            return NotFound();
        
        int profileAuthorId = profileAuthor.AuthorId;

        // The user currently logged in
        var userEmail = User.Identity?.Name; 
        var appUser = await _userManager.FindByEmailAsync(userEmail);
        
        
        int? loggedInAuthorId = null;
        
        if (appUser != null)
        {
            var loggedInAuthor = await _authorRepository.GetAuthorByUserIdAsync(appUser.Id);
            loggedInAuthorId = loggedInAuthor?.AuthorId;
        }

        // own private timeline
        if (loggedInAuthorId != null && loggedInAuthorId == profileAuthorId)
        {
            Cheeps = await _service.GetTimelineCheeps(loggedInAuthorId.Value, CurrentPage);
        }
        else
        {
            // Someone else's timeline
            Cheeps = await _service.GetCheepsByAuthor(author, CurrentPage);
        }


        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!User.Identity.IsAuthenticated)
            return Challenge();

        var user = await _userManager.GetUserAsync(User);
        if (user == null)
            return Forbid();

        await _cheepRepository.CreateCheepAsync(user.UserName ?? "Unknown", user.Email ?? "", Text);

        return RedirectToPage();
    }
}
