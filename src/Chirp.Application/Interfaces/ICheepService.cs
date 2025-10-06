using Chirp.Application.DTOs;

namespace Chirp.Application.Interfaces;

public interface ICheepService
{
    public Task<List<CheepDto>> GetCheeps(int page);
    public Task<List<CheepDto>> GetCheepsByAuthor(string author, int page);
}