using Chirp.Application.DTOs;
using Chirp.Application.Interfaces;
using Chirp.Infrastructure.Repositories;
using Chirp.Domain.Entities;

namespace Chirp.Infrastructure.Services;

public class CheepService : ICheepService
{
    private readonly CheepRepository _repository;
    private const int PageSize = 32;

    public CheepService(CheepRepository repository)
    {
        _repository = repository;
    }

    public async Task<List<CheepDto>> GetCheeps(int page)
    {
        var cheeps = await _repository.GetCheepsAsync(page, PageSize);
        return cheeps.Select(c => new CheepDto(c.Author.Name, c.Text, c.TimeStamp)).ToList();
    }

    public async Task<List<CheepDto>> GetCheepsByAuthor(string author, int page)
    {
        var cheeps = await _repository.GetCheepsByAuthorAsync(author, page, PageSize);
        return cheeps.Select(c => new CheepDto(c.Author.Name, c.Text, c.TimeStamp)).ToList();
    }
}