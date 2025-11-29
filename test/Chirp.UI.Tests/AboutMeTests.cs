using System.Text.RegularExpressions;
using Microsoft.Playwright;
using Microsoft.Playwright.NUnit;

namespace Chirp.UI.Tests;

[TestFixture]
public class AboutMeTests : PageTest
{
    private const string BaseUrl = "http://localhost:5273";

    [Test]
    public async Task Anna_AboutMe_Shows_Profile_Follows_And_Cheeps()
    {
        // --- Login as Anna ---
        await LoginAsync("anna@itu.dk", "Password123");

        // Go to public timeline
        await Page.GotoAsync($"{BaseUrl}/");

        // Follow Bella
        await ClickFollowAsync("Bella");

        // Go to AboutMe page
        await Page.GotoAsync($"{BaseUrl}/Identity/Account/AboutMe");

        // ASSERT: Personal Info
        await Expect(
            Page.GetByRole(AriaRole.Cell, new() { Name = "Anna", Exact = true })).ToBeVisibleAsync();
        await Expect(
            Page.GetByRole(AriaRole.Cell, new() { Name = "anna@itu.dk", Exact = true })
        ).ToBeVisibleAsync();

        // ASSERT: Follows
        await Expect(Page.GetByText("Bella")).ToBeVisibleAsync();

        // ASSERT: Cheep exists
        await Expect(Page.GetByText("Anna sender et test-cheep.")).ToBeVisibleAsync();

        // Clean up 
        await Page.GotoAsync($"{BaseUrl}/");
        await ClickUnfollowAsync("Bella");
    }

    // Helpers methods
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
}
