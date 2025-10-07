namespace Chirp.Domain.Entities;

public class Author
{
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;

    public int AuthorId { get; set; }

    public List<Cheep> Cheeps { get; set; } = new();
}