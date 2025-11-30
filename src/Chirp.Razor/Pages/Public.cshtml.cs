using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Chirp.Application.Interfaces;
using Chirp.Application.DTOs;
using Chirp.Infrastructure.Repositories;
using Microsoft.AspNetCore.Identity;

namespace Chirp.Razor.Pages;

public class PublicModel : PageModel
{
    private readonly CheepRepository _cheepRepository;
    private readonly UserManager<IdentityUser> _userManager;
    private readonly ICheepService _service;
    private readonly IConfiguration _configuration;


    public List<CheepDto> Cheeps { get; set; } = new();
    public int CurrentPage { get; set; } = 1;

    public string? FactCheckResult { get; set; }
    public string? CheckedCheep { get; set; }

    [BindProperty]
    [Required]
    [StringLength(160, ErrorMessage = "Cheep must be max 160 characters")]
    public string Text { get; set; } = string.Empty;

    public PublicModel(
        ICheepService service,
        CheepRepository cheepRepository,
        UserManager<IdentityUser> userManager,
        IConfiguration configuration)
    {
        _service = service;
        _cheepRepository = cheepRepository;
        _userManager = userManager;
        _configuration = configuration;
    }


    public async Task<ActionResult> OnGet([FromQuery] int page)
    {
        return await LoadPageAsync(page);
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!User.Identity?.IsAuthenticated ?? false)
            return Challenge();

        var user = await _userManager.GetUserAsync(User);
        if (user == null)
            return Forbid();

        await _cheepRepository.CreateCheepAsync(user.UserName ?? "Unknown", user.Email ?? "", Text);

        Text = ""; // clear box

        return await LoadPageAsync(CurrentPage);
    }

    public async Task<IActionResult> OnPostFactCheckAsync(string text)
    {
        CheckedCheep = text;
        FactCheckResult = await FactCheckWithOpenAI(text);
        return await LoadPageAsync(CurrentPage);
    }

    private async Task<ActionResult> LoadPageAsync(int page)
    {
        if (page <= 0) page = 1;

        CurrentPage = page;
        Cheeps = await _service.GetCheeps(CurrentPage);

        return Page();
    }

    private async Task<string> FactCheckWithOpenAI(string cheep)
    {
        var apiKey = _configuration["OpenAI:Apikey"];


        if (string.IsNullOrWhiteSpace(apiKey))
            return "AI not configured (no API key).";

        cheep = cheep.Length > 160 ? cheep[..160] : cheep;

        using var client = new HttpClient();
        client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", apiKey);

        var payload = new
        {
            model = "gpt-4.1-mini",
            messages = new[]
            {
                new { role = "system", content = "Classify if a statement is Fact, Opinion, Prediction or Unverifiable. Give short explanation." },
                new { role = "user", content = cheep }
            },
            temperature = 0.1
        };

        var json = System.Text.Json.JsonSerializer.Serialize(payload);
        var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

        var response = await client.PostAsync("https://api.openai.com/v1/chat/completions", content);

        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync();
            return $"AI error ({(int)response.StatusCode}): {error}";
        }
       
        using var stream = await response.Content.ReadAsStreamAsync();
        using var doc = await System.Text.Json.JsonDocument.ParseAsync(stream);

        return doc.RootElement
            .GetProperty("choices")[0]
            .GetProperty("message")
            .GetProperty("content")
            .GetString()!;
    }
}
