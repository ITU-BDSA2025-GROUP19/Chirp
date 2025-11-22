using System.Threading.Tasks;
using Microsoft.Playwright;
using Microsoft.Playwright.NUnit;
using NUnit.Framework;

namespace Chirp.UI.Tests;

[Parallelizable(ParallelScope.Self)]
[TestFixture]
public class CheepBoxTests : PageTest {
    //kører ovres local host
    private const string BaseUrl = "http://localhost:5273"; 

    [Test]
    public async Task CheepBox_Is_Visible_Only_When_LoggedIn() {
        await Page.GotoAsync(BaseUrl);

        var cheepBox = Page.Locator(".cheepbox");
        await Expect(cheepBox).Not.ToBeVisibleAsync();

        await Page.GetByRole(AriaRole.Link, new() { Name = "login" }).ClickAsync();
        await Page.FillAsync("input[type='email']", "ropf@itu.dk"); 
        await Page.FillAsync("input[type='password']", "LetM31n!");
        await Page.GetByRole(AriaRole.Button, new() { Name = "Log in", Exact = true }).ClickAsync();
        await Expect(cheepBox).ToBeVisibleAsync();
    }

    [Test]
    public async Task CheepBox_Prevents_Messages_Longer_Than_160_Chars()
    {
        // 1. Log ind
        await Page.GotoAsync(BaseUrl + "/Identity/Account/Login");
        await Page.FillAsync("input[type='email']", "ropf@itu.dk");
        await Page.FillAsync("input[type='password']", "LetM31n!");
        await Page.GetByRole(AriaRole.Button, new() { Name = "Log in", Exact = true }).ClickAsync();

        // 2. Find input-feltet
        var inputField = Page.Locator("#CheepText");

        // 3. Assert: Tjek at browseren forhindrer for lange beskeder via attributten
        // Dette bekræfter at din [StringLength(160)] virker som den skal i frontend
        await Expect(inputField).ToHaveAttributeAsync("maxlength", "160");

        // (Ekstra tjek): Prøv at fylde den med for meget tekst og se at den klipper det af
        string longMessage = new string('a', 165);
        await inputField.FillAsync(longMessage);
        
        // Hent værdien ud igen og bekræft at den kun er 160 lang
        var value = await inputField.InputValueAsync();
        Assert.That(value.Length, Is.EqualTo(160));
    }
}