using Chirp.Application.Interfaces;
using Chirp.Application.DTOs;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Chirp.Infrastructure.Repositories;

namespace Chirp.Razor.Areas.Identity.Pages.Account;

public class AboutMeModel : PageModel
{
    private readonly UserManager<IdentityUser> _userManager;
    private readonly ICheepService _cheepService;
    private readonly IAuthorService _authorService; 

    public AuthorDto Author { get; set; } = default!;
    public List<CheepDto> Cheeps { get; set; } = new();
    public List<string> Following { get; set; } = new();

    public AboutMeModel(
        UserManager<IdentityUser> userManager,
        ICheepService cheepService,
        IAuthorService authorService)
    {
        _userManager = userManager;
        _cheepService = cheepService;
        _authorService = authorService;
    }

    public async Task OnGetAsync()
    {
        var user = await _userManager.GetUserAsync(User);
        Author = await _authorService.GetAuthorByUserId(user.Id);
        Cheeps = await _cheepService.GetCheepsByUserId(user!.Id, 32);
       /* Followers = awat _followService.GetFollowingByAuthorName(Author.AuthorName)*/
    }
}