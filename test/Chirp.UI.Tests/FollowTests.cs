using System.Text.RegularExpressions;
using Microsoft.Playwright;
using Microsoft.Playwright.NUnit;
using NUnit.Framework;

namespace Chirp.UI.Tests;

[TestFixture]
public class FollowTests : PageTest
{
    private const string BaseUrl = "http://localhost:5273";

   [Test]
    public async Task Anna_Can_Follow_Bella_And_Cheryl()
    {
        // --- ANNA logs in ---
        await LoginAsync("anna@itu.dk", "Password123");

        // Make sure we are on the public timeline
        await Page.GotoAsync($"{BaseUrl}/");

        // Anna should NOT see follow/unfollow on herself
        await AssertNoFollowButton("Anna");

        // Anna SHOULD see follow buttons for Bella + Cheryl
        await AssertFollowVisible("Bella");
        await AssertFollowVisible("Cheryl");

        // Follow both
        await ClickFollowAsync("Bella");
        await ClickFollowAsync("Cheryl");

        // Buttons should now be "unfollow"
        await AssertUnfollowVisible("Bella");
        await AssertUnfollowVisible("Cheryl");
        
        // Unfollow both
        await ClickUnfollowAsync("Bella");
        await ClickUnfollowAsync("Cheryl");
    }


    [Test]
    public async Task Anna_PrivateTimeline_Shows_Her_Followees_Cheeps()
    {
        // Anna logs in
        await LoginAsync("anna@itu.dk", "Password123");

        // Ensure she follows Bella & Cheryl
        await Page.GotoAsync($"{BaseUrl}/");
        await ClickFollowAsync("Bella");
        await ClickFollowAsync("Cheryl");

        // Go to Anna's private timeline
        await Page.GotoAsync($"{BaseUrl}/Anna");

        // Should see cheeps from: Anna, Bella, Cheryl
        await Expect(Page.Locator("li[data-author='Anna']")).ToBeVisibleAsync();
        await Expect(Page.Locator("li[data-author='Bella']")).ToBeVisibleAsync();
        await Expect(Page.Locator("li[data-author='Cheryl']")).ToBeVisibleAsync();
        
        // Unfollow both
        await Page.GotoAsync($"{BaseUrl}/");
        await ClickUnfollowAsync("Bella");
        await ClickUnfollowAsync("Cheryl");
    }


    [Test]
    public async Task Anna_Visiting_BellaTimeline_Shows_Only_Bella_Cheeps()
    {
        // Anna logs in
        await LoginAsync("anna@itu.dk", "Password123");

        // Ensure she follows Bella & Cheryl
        await Page.GotoAsync($"{BaseUrl}/");
        await ClickFollowAsync("Bella");
        await ClickFollowAsync("Cheryl");

        // Visit Bella's timeline
        await Page.GotoAsync($"{BaseUrl}/Bella");

        // Bella's cheeps visible
        await Expect(Page.Locator("li[data-author='Bella']")).ToBeVisibleAsync();

        // Cheryl should NOT appear on Bella's timeline
        await Expect(Page.Locator("li[data-author='Cheryl']")).Not.ToBeVisibleAsync();

        // Anna's own cheeps should NOT appear
        await Expect(Page.Locator("li[data-author='Anna']")).Not.ToBeVisibleAsync();
        
        // Unfollow both
        await Page.GotoAsync($"{BaseUrl}/");
        await ClickUnfollowAsync("Bella");
        await ClickUnfollowAsync("Cheryl");
    }


    [Test]
    public async Task Anna_Can_Unfollow_Bella()
    {
        // Anna logs in
        await LoginAsync("anna@itu.dk", "Password123");

        await Page.GotoAsync($"{BaseUrl}/");

        // Ensure following both Bella and Cheryl
        await ClickFollowAsync("Bella");
        await ClickFollowAsync("Cheryl");

        await AssertUnfollowVisible("Bella");

        // Unfollow Bella
        await ClickUnfollowAsync("Bella");

        // Now only Cheryl should remain followed
        await AssertFollowVisible("Bella");
        await AssertUnfollowVisible("Cheryl");
        
        // Unfollow Cheryl
        await Page.GotoAsync($"{BaseUrl}/");
        await ClickUnfollowAsync("Cheryl");
    }
    
    // ------------------------------
    // Helper Methods
    // ------------------------------
   
    private async Task LoginAsync(string email, string password)
    {
        await Page.GotoAsync($"{BaseUrl}/Identity/Account/Login");

        await Page.GetByLabel("Email").FillAsync(email);
        await Page.GetByLabel("Password").FillAsync(password);

        await Page.GetByRole(AriaRole.Button, new() { Name = "Log in", Exact = true })
            .ClickAsync();

        await Expect(Page).ToHaveURLAsync(new Regex("https?://localhost:\\d+/"));
    }

    private ILocator CheepFor(string authorName)
        => Page.Locator($"li[data-author='{authorName}']");

    private async Task ClickFollowAsync(string authorName)
    {
        var cheep = CheepFor(authorName);
        await cheep.GetByRole(AriaRole.Button, new() { Name = "follow", Exact = true })
            .ClickAsync();
    }

    private async Task ClickUnfollowAsync(string authorName)
    {
        var cheep = CheepFor(authorName);
        await cheep.GetByRole(AriaRole.Button, new() { Name = "unfollow", Exact = true })
            .ClickAsync();
    }

    private async Task AssertFollowVisible(string authorName)
    {
        await Expect(
            CheepFor(authorName).GetByRole(AriaRole.Button, new() { Name = "follow", Exact = true })
        ).ToBeVisibleAsync();
    }

    private async Task AssertUnfollowVisible(string authorName)
    {
        await Expect(
            CheepFor(authorName).GetByRole(AriaRole.Button, new() { Name = "unfollow", Exact = true })
        ).ToBeVisibleAsync();
    }

    private async Task AssertNoFollowButton(string authorName)
    {
        await Expect(
            CheepFor(authorName).GetByRole(AriaRole.Button)
        ).Not.ToBeVisibleAsync();
    }
}
