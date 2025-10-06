using Chirp.Domain.Entities;
using Chirp.Application.Interfaces;
using Chirp.Infrastructure.Data;
using Chirp.Domain.Entities;
using Chirp.Application;


    //Temp code to satisfy dotnet build for task 1.a until 1.b is implemented:

    
namespace Chirp.Infrastructure.Services;

public class CheepService : ICheepService
{
    public List<Cheep> GetCheeps(int limit = 1000)
    {
        return new List<Cheep>();
    }

    public List<Cheep> GetCheepsByAuthor(string author, int limit = 1000)
    {
        return new List<Cheep>();
    }
}

    //TODO: Refactor for issue 1.b, repository pattern
    /* private readonly DBFacade _dbFacade;
     private const int pageSize = 32; 

     public CheepService(DBFacade dbFacade)
     {
         _dbFacade = dbFacade;
     }
     public List<Cheep> GetCheeps(int page)
     {
         if (page < 1) page = 1;
         int skipPages = (page - 1) * pageSize;

         var cheeps = _dbFacade.GetCheeps();
         return cheeps.Skip(skipPages).Take(pageSize).ToList();
     }

     public List<Cheep> GetCheepsByAuthor(string author, int page)
     {
         if (page < 1) page = 1;
         int skipPages = (page - 1) * pageSize;

         var cheeps = _dbFacade.GetCheepsByAuthor(author);
         return cheeps.Skip(skipPages).Take(pageSize).ToList();
     }

     public void CreateCheep(string message, string author)
     {
         //Not implemented yet
     } */
