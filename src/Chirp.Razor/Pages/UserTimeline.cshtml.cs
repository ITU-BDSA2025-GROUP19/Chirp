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
    private readonly AuthorRepository _authorRepository;
    private readonly UserManager<IdentityUser> _userManager;
    private readonly IConfiguration _configuration;
    private readonly IAiFactCheckService _ai;
    
    public List<CheepDto> Cheeps { get; set; } = new();
    public int CurrentPage { get; set; } = 1;
    public string Author { get; set; } = "";
    
    public string? FactCheckResult { get; set; }
    public string? CheckedCheep { get; set; }

    [BindProperty]
    [Required]
    [StringLength(160, ErrorMessage = "Cheep must be max 160 characters")]
    public string Text { get; set; } = string.Empty;

    public UserTimelineModel(
        ICheepService service,
        UserManager<IdentityUser> userManager,
        CheepRepository cheepRepository,
        AuthorRepository authorRepository,
        IAiFactCheckService ai,
        IConfiguration configuration)
    {
        _service = service;
        _userManager = userManager;
        _cheepRepository = cheepRepository;
        _authorRepository = authorRepository;
        _ai = ai;
        _configuration = configuration;
    }

    public async Task<ActionResult> OnGet(string author, [FromQuery] int page)
    {
        return await LoadPageAsync(author, page);
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
    
    public async Task<IActionResult> OnPostFactCheckAsync(string text)
    {
        CheckedCheep = text;
        FactCheckResult = await _ai.FactCheckAsync(text);

        var author = RouteData.Values["author"]?.ToString() ?? Author;
        var pageQ = Request.Query["page"];
        var page = int.TryParse(pageQ, out var p) ? p : 1;

        return await LoadPageAsync(author, page);
    }
    
    private async Task<ActionResult> LoadPageAsync(string author, int page)
    {
        if (page <= 0) page = 1;

        CurrentPage = page;
        Author = author;

        var profileAuthor = await _authorRepository.GetAuthorByNameAsync(author);
        if (profileAuthor == null)
            return NotFound();

        int profileAuthorId = profileAuthor.AuthorId;

        var userEmail = User.Identity?.Name;
        var appUser = userEmail == null ? null : await _userManager.FindByEmailAsync(userEmail);

        int? loggedInAuthorId = null;
        if (appUser != null)
        {
            var loggedInAuthor = await _authorRepository.GetAuthorByUserIdAsync(appUser.Id);
            loggedInAuthorId = loggedInAuthor?.AuthorId;
        }

        if (loggedInAuthorId != null && loggedInAuthorId == profileAuthorId)
        {
            Cheeps = await _service.GetTimelineCheeps(profileAuthorId, CurrentPage);
        }
        else
        {
            Cheeps = await _service.GetCheepsByAuthor(author, CurrentPage);
        }

        return Page();
    }
}

