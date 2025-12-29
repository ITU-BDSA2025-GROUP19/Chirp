using Chirp.Application.DTOs;

namespace Chirp.Application.Interfaces;

public interface IAuthorService
{
    public Task<AuthorDto?> GetAuthorByUserId(string userId);    
    public Task DeleteAuthor(string userId);
}