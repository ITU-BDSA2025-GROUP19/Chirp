using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Chirp.Infrastructure.Repositories;
using Microsoft.AspNetCore.Identity;

namespace Chirp.Razor.Pages;

[IgnoreAntiforgeryToken]
public class UnfollowModel : PageModel
{
    private readonly FollowRepository _followRepo;
    private readonly AuthorRepository _authorRepo;
    private readonly UserManager<IdentityUser> _userManager;

    public UnfollowModel(
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
        var appUserId = _userManager.GetUserId(User);
        if (appUserId == null)
            return RedirectToPage("/Public");

        var follower = await _authorRepo.GetAuthorByUserIdAsync(appUserId);
        if (follower == null)
            return RedirectToPage("/Public");

        await _followRepo.UnfollowAsync(follower.AuthorId, followeeId);

        return Redirect(Request.Headers["Referer"].ToString());
    }
}