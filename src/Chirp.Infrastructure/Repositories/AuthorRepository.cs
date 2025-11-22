using Chirp.Domain.Entities;
using Chirp.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;


namespace Chirp.Infrastructure.Repositories;

public class AuthorRepository
{
    private readonly ChirpDbContext _context;

    public AuthorRepository(ChirpDbContext context)
    {
        _context = context;
    }
    
    public async Task<Author?> GetAuthorByName(string name)
    {
        return await _context.Authors
            .FirstOrDefaultAsync(a => a.Name == name);
    }
    
    public async Task<Author?> GetAuthorByEmail(string email)
    {
        return await _context.Authors
            .FirstOrDefaultAsync(a => a.Email == email);
    }
    
    public async Task CreateAuthorAsync(string name, string email)
    {
        bool nameExists = await _context.Authors.AnyAsync(a => a.Name == name);
        bool emailExists = await _context.Authors.AnyAsync(a => a.Email == email);

        if (nameExists || emailExists)
        {
            throw new InvalidOperationException(
                $"An author with the same {(nameExists ? "name" : "email")} already exists.");
        }

        var newAuthor = new Author
        {
            Name = name,
            Email = email
        };

        _context.Authors.Add(newAuthor);
        await _context.SaveChangesAsync();
    }
    
    public async Task<Author?> GetAuthorByUserIdAsync(string appUserId)
    {
        return await _context.Authors
            .FirstOrDefaultAsync(a => a.ApplicationUserId == appUserId);
    }
    
    public async Task<Author?> GetAuthorByNameAsync(string name)
    {
        return await _context.Authors
            .FirstOrDefaultAsync(a => a.Name.ToLower() == name.ToLower());
    }

    public async Task FollowAsync(int followerAuthorId, int followeeAuthorId)
    {
        if (followerAuthorId == followeeAuthorId)
            return;

        bool exists = await _context.Follows
            .AnyAsync(f => f.FollowerId == followerAuthorId && f.FolloweeId == followeeAuthorId);

        if (!exists)
        {
            _context.Follows.Add(new Follow
            {
                FollowerId = followerAuthorId,
                FolloweeId = followeeAuthorId
            });

            await _context.SaveChangesAsync();
        }
    }
    public async Task UnfollowAsync(int followerAuthorId, int followeeAuthorId)
    {
        var relation = await _context.Follows
            .FirstOrDefaultAsync(f => f.FollowerId == followerAuthorId && f.FolloweeId == followeeAuthorId);

        if (relation != null)
        {
            _context.Follows.Remove(relation);
            await _context.SaveChangesAsync();
        }
    }
    
    public Task<bool> IsFollowingAsync(int followerAuthorId, int followeeAuthorId)
    {
        return _context.Follows
            .AnyAsync(f => f.FollowerId == followerAuthorId && f.FolloweeId == followeeAuthorId);
    }

    public async Task<List<int>> GetFollowedAuthorIdsAsync(int followerAuthorId)
    {
        return await _context.Follows
            .Where(f => f.FollowerId == followerAuthorId)
            .Select(f => f.FolloweeId)
            .ToListAsync();
    }
}