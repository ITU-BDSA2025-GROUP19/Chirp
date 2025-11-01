namespace Chirp.Application.DTOs;

public record CheepDto(string AuthorName, string Text, DateTime TimeStamp)
{
    public string FormattedDate => TimeStamp.ToString("g");
}