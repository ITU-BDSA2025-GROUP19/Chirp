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

    public IdentityUser UserInfo { get; set; } = default!;
    public List<CheepDto> MyCheeps { get; set; } = new();
    public List<string> Following { get; set; } = new();

    public AboutMeModel(
        UserManager<IdentityUser> userManager,
        ICheepService cheepService)
    {
        _userManager = userManager;
        _cheepService = cheepService;
    }

    public async Task OnGetAsync()
    {
        var user = await _userManager.GetUserAsync(User);
        var author = await
        MyCheeps = await _cheepService.GetCheepsByUserId(user!.Id, 32);
    }
}