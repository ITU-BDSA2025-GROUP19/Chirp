using System;
using System.Text.RegularExpressions;
using Microsoft.Playwright;
using Microsoft.Playwright.NUnit;

namespace Chirp.UI.Tests;

[Parallelizable(ParallelScope.Self)]
[TestFixture]
public class RegisterTests : PageTest
{
    // Generates a new unique email each test run
    private string GenerateEmail()
    {
        return $"testuser_{Guid.NewGuid():N}@example.com";
    }

    [Test]
    public async Task UserCanRegisterSuccessfully()
    {
        // Arrange
        var email = GenerateEmail();
        var username = "TestUser";
        var password = "Test123!";  

        // Act: go to register page
        await Page.GotoAsync("http://localhost:5273/Identity/Account/Register");

        // Fill form
        await Page.FillAsync("#Input_DisplayName", username);
        await Page.FillAsync("#Input_Email", email);
        await Page.FillAsync("#Input_Password", password);
        await Page.FillAsync("#Input_ConfirmPassword", password);

        // Submit
        await Page.ClickAsync("#registerSubmit");

        // Assert: user should be redirected to home page and logged in
        await Expect(Page).ToHaveURLAsync(new Regex(".*/$"));

        // Assert: Ensure logout[username] appears on the page (meaning we are logged in)
        await Expect(
            Page.GetByRole(AriaRole.Button, new() { Name = $"logout [{username}]" })
        ).ToBeVisibleAsync();
    }
}