using Chirp.Application.Interfaces;
using Chirp.Application.DTOs;
using Chirp.Domain.Entities;
using Chirp.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Chirp.Infrastructure.Services;

public class AuthorService : IAuthorService
{
    private readonly ChirpDbContext _context;

    public AuthorService(ChirpDbContext context)
    {
        _context = context;
    }
    public async Task<AuthorDto?> GetAuthorByUserId(string userId)
    {
        var author = await _context.Authors
            .FirstOrDefaultAsync(a => a.ApplicationUserId == userId);

        if (author == null) 
        {
            return null;
        }
        return new AuthorDto(author.AuthorId, author.Name, author.Email);
    }

    public async Task DeleteAuthor(string userId)
    {
        var author = await _context.Authors
            .FirstOrDefaultAsync(a => a.ApplicationUserId == userId);

        if (author != null)
        {
            // Removing  follows
            var follows = _context.Follows.Where(f => f.FollowerId == author.AuthorId || f.FolloweeId == author.AuthorId);
            _context.Follows.RemoveRange(follows);

            // Making the author the Deleted User
            author.Name = "Deleted User";
            author.Email = "deleted@chirp.com";
            author.ApplicationUserId = null;

            await _context.SaveChangesAsync();
        }
    }
}