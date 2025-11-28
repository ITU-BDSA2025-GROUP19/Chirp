using Chirp.Application.DTOs;
using Chirp.Application.Interfaces;
using Chirp.Infrastructure.Repositories;
using Chirp.Domain.Entities;

namespace Chirp.Infrastructure.Services;

public class AuthorService : IAuthorService
{
    private readonly AuthorRepository _repository;
    
    public AuthorService(AuthorRepository repository)
    {
        _repository = repository;
    }
    
    public async Task<AuthorDto?> GetAuthorByUserId(string userId)
    {
        var author = await _repository.GetAuthorByUserIdAsync(userId);
        return ToDto(author!);

    }
    
    private static AuthorDto ToDto(Author a)
        => new AuthorDto(a.Name, a.Email);
}
    
