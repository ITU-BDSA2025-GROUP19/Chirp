namespace Chirp.Application.DTOs;

public record CheepDto(string Author, string Text, DateTime TimeStamp)
{
    public string FormattedDate => TimeStamp.ToString("g");
}