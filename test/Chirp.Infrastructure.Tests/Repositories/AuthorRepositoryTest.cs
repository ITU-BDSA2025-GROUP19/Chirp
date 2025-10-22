using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Xunit;
using Chirp.Domain.Entities;
using Chirp.Infrastructure.Data;
using Chirp.Infrastructure.Repositories;

namespace Chirp.Infrastructure.Tests.Repositories
{
    public class AuthorRepositoryTest
    {
        private ChirpDbContext CreateDbContext()
        {
            var options = new DbContextOptionsBuilder<ChirpDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString()) // Unique DB per test
                .Options;

            var context = new ChirpDbContext(options);
            context.Database.EnsureCreated();
            return context;
        }

        [Fact]
        public async Task GetAuthorByName_ShouldReturnAuthor_WhenExists()
        {
            using var context = CreateDbContext();
            var repo = new AuthorRepository(context);

            var author = new Author { Name = "Alice", Email = "alice@example.com" };
            context.Authors.Add(author);
            await context.SaveChangesAsync();

            var result = await repo.GetAuthorByName("Alice");

            Assert.NotNull(result);
            Assert.Equal("Alice", result!.Name);
        }

        [Fact]
        public async Task GetAuthorByName_ShouldReturnNull_WhenNotExists()
        {
            using var context = CreateDbContext();
            var repo = new AuthorRepository(context);

            var result = await repo.GetAuthorByName("Unknown");

            Assert.Null(result);
        }

        [Fact]
        public async Task GetAuthorByEmail_ShouldReturnAuthor_WhenExists()
        {
            using var context = CreateDbContext();
            var repo = new AuthorRepository(context);

            var author = new Author { Name = "Bob", Email = "bob@example.com" };
            context.Authors.Add(author);
            await context.SaveChangesAsync();

            var result = await repo.GetAuthorByEmail("bob@example.com");

            Assert.NotNull(result);
            Assert.Equal("bob@example.com", result!.Email);
        }

        [Fact]
        public async Task GetAuthorByEmail_ShouldReturnNull_WhenNotExists()
        {
            using var context = CreateDbContext();
            var repo = new AuthorRepository(context);

            var result = await repo.GetAuthorByEmail("missing@example.com");

            Assert.Null(result);
        }

        [Fact]
        public async Task CreateAuthorAsync_ShouldAddNewAuthor_WhenNotExists()
        {
            using var context = CreateDbContext();
            var repo = new AuthorRepository(context);

            await repo.CreateAuthorAsync("Charlie", "charlie@example.com");

            var author = await context.Authors.FirstOrDefaultAsync(a => a.Name == "Charlie");
            Assert.NotNull(author);
            Assert.Equal("charlie@example.com", author!.Email);
        }

        [Fact]
        public async Task CreateAuthorAsync_ShouldThrowException_WhenNameExists()
        {
            using var context = CreateDbContext();
            var repo = new AuthorRepository(context);

            context.Authors.Add(new Author { Name = "Dave", Email = "dave@example.com" });
            await context.SaveChangesAsync();

            await Assert.ThrowsAsync<InvalidOperationException>(
                () => repo.CreateAuthorAsync("Dave", "newemail@example.com"));
        }

        [Fact]
        public async Task CreateAuthorAsync_ShouldThrowException_WhenEmailExists()
        {
            using var context = CreateDbContext();
            var repo = new AuthorRepository(context);

            context.Authors.Add(new Author { Name = "Eve", Email = "eve@example.com" });
            await context.SaveChangesAsync();

            await Assert.ThrowsAsync<InvalidOperationException>(
                () => repo.CreateAuthorAsync("NewName", "eve@example.com"));
        }
    }
}
