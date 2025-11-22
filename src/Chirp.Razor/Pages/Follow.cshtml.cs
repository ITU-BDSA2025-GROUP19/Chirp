using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Chirp.Infrastructure.Repositories;
using Microsoft.AspNetCore.Identity;

namespace Chirp.Razor.Pages;

[IgnoreAntiforgeryToken]
public class FollowModel : PageModel
{
    private readonly FollowRepository _followRepo;
    private readonly AuthorRepository _authorRepo;
    private readonly UserManager<IdentityUser> _userManager;

    public FollowModel(
        FollowRepository followRepo,
        AuthorRepository authorRepo,
        UserManager<IdentityUser> userManager)
    {
        _followRepo = followRepo;
        _authorRepo = authorRepo;
        _userManager = userManager;
    }

    public async Task<IActionResult> OnPostAsync(int followeeId)
    {
        Console.WriteLine("FOLLOW POST hit. followeeId: " + followeeId);
        var appUserId = _userManager.GetUserId(User);
        if (appUserId == null)
            return RedirectToPage("/Public");

        var follower = await _authorRepo.GetAuthorByUserIdAsync(appUserId);
        if (follower == null)
            return RedirectToPage("/Public");

        if (follower.AuthorId == followeeId)
            return Redirect(Request.Headers["Referer"].ToString());

        await _followRepo.FollowAsync(follower.AuthorId, followeeId);

        return Redirect(Request.Headers["Referer"].ToString());
    }
}
