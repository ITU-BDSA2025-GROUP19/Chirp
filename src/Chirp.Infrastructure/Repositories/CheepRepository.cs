using Chirp.Domain.Entities;
using Chirp.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Chirp.Infrastructure.Repositories;

public class CheepRepository
{
    private readonly ChirpDbContext _context;
    private readonly AuthorRepository _authorRepository;

    public CheepRepository(ChirpDbContext context)
    {
        _context = context;
        _authorRepository = new AuthorRepository(context);
    }

    public async Task<List<Cheep>> GetCheepsAsync(int page = 1, int pageSize = 32)
    {
        return await _context.Cheeps
            .Include(c => c.Author)
            .OrderByDescending(c => c.TimeStamp)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<List<Cheep>> GetCheepsByAuthorAsync(string authorName, int page = 1, int pageSize = 32)
    {
        return await _context.Cheeps
            .Include(c => c.Author)
            .Where(c => c.Author.Name == authorName)
            .OrderByDescending(c => c.TimeStamp)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }
    
    public async Task CreateCheepAsync(string authorName, string authorEmail, string text)
    {
        if (text.Length > 160)
        {
            throw new ArgumentException("Cheep cannot be longer than 160 characters.");
        }
        var author = await _authorRepository.GetAuthorByName(authorName);

        // If author doesn't exist, create one
        if (author == null)
        {
            await _authorRepository.CreateAuthorAsync(authorName, authorEmail);
            author = await _authorRepository.GetAuthorByName(authorName);
            
            if (author == null)
            {
                throw new InvalidOperationException($"Author '{authorName}' could not be created or found.");
            }
        }

        // Create and add the cheep
        var cheep = new Cheep
        {
            Author = author,
            Text = text,
            TimeStamp = DateTime.UtcNow
        };

        _context.Cheeps.Add(cheep);
        await _context.SaveChangesAsync();
    }
}