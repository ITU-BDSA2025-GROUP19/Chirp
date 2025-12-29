using Chirp.Application.Interfaces;
using Chirp.Application.DTOs;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc;

namespace Chirp.Razor.Areas.Identity.Pages.Account;

public class AboutMeModel : PageModel
{
    private readonly UserManager<IdentityUser> _userManager;
    private readonly ICheepService _cheepService;
    private readonly IAuthorService _authorService; 
    private readonly IFollowService _followService; 
    private readonly SignInManager<IdentityUser> _signInManager;

    public AuthorDto Author { get; set; } = default!;
    public List<CheepDto> Cheeps { get; set; } = new();
    public List<FollowDto> Following { get; set; } = new();

    public AboutMeModel(
        UserManager<IdentityUser> userManager,
        ICheepService cheepService,
        IAuthorService authorService,
        IFollowService followService,
        SignInManager<IdentityUser> signInManager
)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _cheepService = cheepService;
        _authorService = authorService;
        _followService = followService;
    }

    public async Task OnGetAsync()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user != null)
        {
            Author = await _authorService.GetAuthorByUserId(user.Id);
            Cheeps = await _cheepService.GetCheepsByUserId(user.Id, 1);
            Following = await _followService.GetFollowsByAuthorId(Author.AuthorId);
        }
    }
    
//Forget me 
    public async Task<IActionResult> OnPostForgetMeAsync()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
        }

        await _authorService.DeleteAuthor(user.Id);

        var result = await _userManager.DeleteAsync(user);
        if (!result.Succeeded)
        {
            throw new InvalidOperationException($"Unexpected error occurred deleting user.");
        }

        // Logging out the user after deletion
        await _signInManager.SignOutAsync();

        //To main page 
        return Redirect("/");
    }
}