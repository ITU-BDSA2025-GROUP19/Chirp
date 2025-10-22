using Microsoft.EntityFrameworkCore;
using Chirp.Domain.Entities;
using Chirp.Infrastructure.Data;
using Chirp.Infrastructure.Repositories;

namespace Chirp.Infrastructure.Tests.Repositories
{
    public class CheepRepositoryTest
    {
        private ChirpDbContext CreateDbContext()
        {
            var options = new DbContextOptionsBuilder<ChirpDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            var context = new ChirpDbContext(options);
            context.Database.EnsureCreated();
            return context;
        }

        private async Task SeedDataAsync(ChirpDbContext context)
        {
            var author = new Author { Name = "Alice", Email = "alice@example.com" };
            context.Authors.Add(author);

            context.Cheeps.AddRange(
                new Cheep { Author = author, Text = "Hello world!", TimeStamp = DateTime.UtcNow.AddMinutes(-1) },
                new Cheep { Author = author, Text = "Second cheep!", TimeStamp = DateTime.UtcNow }
            );

            await context.SaveChangesAsync();
        }

        [Fact]
        public async Task GetCheepsAsync_ShouldReturnAllCheepsInDescendingOrder()
        {
            using var context = CreateDbContext();
            await SeedDataAsync(context);
            var repo = new CheepRepository(context);

            var result = await repo.GetCheepsAsync();

            Assert.Equal(2, result.Count);
            Assert.Equal("Second cheep!", result[0].Text);
            Assert.Equal("Hello world!", result[1].Text);
        }

        [Fact]
        public async Task GetCheepsByAuthorAsync_ShouldReturnOnlyCheepsByThatAuthor()
        {
            using var context = CreateDbContext();
            await SeedDataAsync(context);
            var repo = new CheepRepository(context);

            var result = await repo.GetCheepsByAuthorAsync("Alice");

            Assert.Equal(2, result.Count);
            Assert.All(result, c => Assert.Equal("Alice", c.Author.Name));
        }

        [Fact]
        public async Task CreateCheepAsync_ShouldCreateCheepForExistingAuthor()
        {
            using var context = CreateDbContext();
            await SeedDataAsync(context);
            var repo = new CheepRepository(context);

            await repo.CreateCheepAsync("Alice", "alice@example.com", "New Cheep");

            var cheeps = await context.Cheeps.ToListAsync();

            Assert.Equal(3, cheeps.Count);
            Assert.Contains(cheeps, c => c.Text == "New Cheep" && c.Author.Name == "Alice");
        }

        [Fact]
        public async Task CreateCheepAsync_ShouldCreateNewAuthorIfNotExists()
        {
            using var context = CreateDbContext();
            var repo = new CheepRepository(context);

            await repo.CreateCheepAsync("Bob", "bob@example.com", "First Cheep");

            var author = await context.Authors.FirstOrDefaultAsync(a => a.Name == "Bob");
            var cheep = await context.Cheeps.FirstOrDefaultAsync(c => c.Author.Name == "Bob");

            Assert.NotNull(author);
            Assert.NotNull(cheep);
            Assert.Equal("First Cheep", cheep.Text);
        }


    }
}
