using Chirp.Domain.Entities;
using Chirp.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Chirp.Infrastructure.Repositories;

public class CheepRepository
{
    private readonly ChirpDbContext _context;

    public CheepRepository(ChirpDbContext context)
    {
        _context = context;
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
}