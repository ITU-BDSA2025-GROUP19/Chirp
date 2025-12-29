namespace Chirp.Application.Interfaces;

public interface IAiFactCheckService
{
    public Task<string> FactCheckAsync(string text);
}