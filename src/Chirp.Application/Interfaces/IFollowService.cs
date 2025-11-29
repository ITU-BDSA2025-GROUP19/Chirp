using Chirp.Application.DTOs;

namespace Chirp.Application.Interfaces;

public interface IFollowService
{
    public Task<List<FollowDto>> GetFollowsByAuthorId(int authorId);
}