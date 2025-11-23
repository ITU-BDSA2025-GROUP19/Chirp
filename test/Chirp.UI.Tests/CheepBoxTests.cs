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

    //tester at kun brugere som er logge ind må skrive cheeps
    [Test]
    public async Task CheepBox_Is_Visible_Only_When_LoggedIn() {
        await Page.GotoAsync(BaseUrl);

        //tjekker om cheepbox er skjult for anonym bruger
        var cheepBox = Page.Locator(".cheepbox");
        await Expect(cheepBox).Not.ToBeVisibleAsync();

        //simulerer at bruger kan logge ind og faktisk logger ind
        await Page.GetByRole(AriaRole.Link, new() { Name = "login" }).ClickAsync();
        await Page.FillAsync("input[type='email']", "ropf@itu.dk"); 
        await Page.FillAsync("input[type='password']", "LetM31n!");
        await Page.GetByRole(AriaRole.Button, new() { Name = "Log in", Exact = true }).ClickAsync();

        //tjekker nu at cheepboxen er synlig efter at bruger loggede korrekt ind
        await Expect(cheepBox).ToBeVisibleAsync();
    }

    [Test]
    public async Task CheepBox_Prevents_Messages_Longer_Than_160_Chars()
    {
        //logger ind
        await Page.GotoAsync(BaseUrl + "/Identity/Account/Login");
        await Page.FillAsync("input[type='email']", "ropf@itu.dk");
        await Page.FillAsync("input[type='password']", "LetM31n!");
        await Page.GetByRole(AriaRole.Button, new() { Name = "Log in", Exact = true }).ClickAsync();

        //Find input-feltet
        var inputField = Page.Locator("#CheepText");

        /*
        tjekker at browseren forhindrer for lange beskeder
        */ 
        await Expect(inputField).ToHaveAttributeAsync("maxlength", "160");

        //laver med vilje for lang tekst og ser om den klipper den
        string longMessage = new string('a', 165);
        await inputField.FillAsync(longMessage);
        
        //henter værdien og tjekker om den er 160 lang - altså om den blev klippet
        var value = await inputField.InputValueAsync();
        Assert.That(value.Length, Is.EqualTo(160));
    }

    [Test]
    public async Task UserTimeline_Shows_Validation_Message_For_Overlong_Cheeps()
    {
        await Page.GotoAsync(BaseUrl + "/Identity/Account/Login");
        await Page.FillAsync("input[type='email']", "ropf@itu.dk");
        await Page.FillAsync("input[type='password']", "LetM31n!");
        await Page.GetByRole(AriaRole.Button, new() { Name = "Log in", Exact = true }).ClickAsync();

        await Page.GotoAsync(BaseUrl + "/Helge");

        var inputField = Page.Locator("#UserCheepText");

        await Page.EvaluateAsync("selector => document.querySelector(selector).removeAttribute('maxlength')", "#UserCheepText");

        string longMessage = new string('a', 165);
        await inputField.FillAsync(longMessage);
        await Page.GetByRole(AriaRole.Button, new() { Name = "Share" }).ClickAsync();

        var validationMessage = Page.Locator("span[data-valmsg-for='Text']");
        await Expect(validationMessage).ToHaveTextAsync("Cheep must be max 160 characters");
    }
}