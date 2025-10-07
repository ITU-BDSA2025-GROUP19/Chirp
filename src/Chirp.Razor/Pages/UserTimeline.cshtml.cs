using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Chirp.Application.Interfaces;
using Chirp.Application.DTOs;

namespace Chirp.Razor.Pages;

public class UserTimelineModel : PageModel
{
    private readonly ICheepService _service;
    public List<CheepDto> Cheeps { get; set; } = new();
    public int CurrentPage { get; set; } = 1; 

    public string Author { get; set; } = "";

    public UserTimelineModel(ICheepService service)
    {
        _service = service;
    }

    public async Task<ActionResult> OnGet(string author, [FromQuery] int page)
    {
        if (page == 0) page = 1;

        CurrentPage = page;
        Author = author;

        Cheeps = await _service.GetCheepsByAuthor(author, page);
        
        return Page();
    }
}
