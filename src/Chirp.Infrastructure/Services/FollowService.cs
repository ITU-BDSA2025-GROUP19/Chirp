using Chirp.Application.DTOs;
using Chirp.Application.Interfaces;
using Chirp.Infrastructure.Repositories;
using Chirp.Domain.Entities;

namespace Chirp.Infrastructure.Services;

public class FollowService: IFollowService
{
    private readonly FollowRepository _repository;
    
    public FollowService(FollowRepository repository)
    {
        _repository = repository;
    }
    
    public async Task<List<FollowDto>> GetFollowsByAuthorId(int authorId)
    {
        return await _repository.GetFollowsByAuthorIdAsync(authorId);
    }
}