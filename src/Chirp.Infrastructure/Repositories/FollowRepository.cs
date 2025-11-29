using Chirp.Application.DTOs;
using Chirp.Domain.Entities;
using Chirp.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Chirp.Infrastructure.Repositories;

public class FollowRepository
{
    private readonly ChirpDbContext _context;

    public FollowRepository(ChirpDbContext context)
    {
        _context = context;
    }

    // Follow a user
    public async Task FollowAsync(int followerAuthorId, int followeeAuthorId)
    {
        if (followerAuthorId == followeeAuthorId)
            return;

        bool alreadyFollowing = await _context.Follows.AnyAsync(f =>
            f.FollowerId == followerAuthorId &&
            f.FolloweeId == followeeAuthorId);

        if (alreadyFollowing)
            return;

        var follow = new Follow
        {
            FollowerId = followerAuthorId,
            FolloweeId = followeeAuthorId
        };

        _context.Follows.Add(follow);
        await _context.SaveChangesAsync();
    }

    // Unfollow
    public async Task UnfollowAsync(int followerAuthorId, int followeeAuthorId)
    {
        var follow = await _context.Follows.FirstOrDefaultAsync(f =>
            f.FollowerId == followerAuthorId &&
            f.FolloweeId == followeeAuthorId);

        if (follow is null)
            return;

        _context.Follows.Remove(follow);
        await _context.SaveChangesAsync();
    }

    // Check if following
    public async Task<bool> IsFollowingAsync(int followerAuthorId, int followeeAuthorId)
    {
        return await _context.Follows.AnyAsync(f =>
            f.FollowerId == followerAuthorId &&
            f.FolloweeId == followeeAuthorId);
    }

    // Get followers of a user 
    public async Task<List<FollowDto>> GetFollowsByAuthorIdAsync(int authorId)
    {
        return await (
            from f in _context.Follows
            join follower in _context.Authors on f.FollowerId equals follower.AuthorId
            join followee in _context.Authors on f.FolloweeId equals followee.AuthorId
            where f.FollowerId == authorId
            select new FollowDto(
                follower.Name, // who follows
                followee.Name // who is followed
            )
        ).ToListAsync();
    }
}
