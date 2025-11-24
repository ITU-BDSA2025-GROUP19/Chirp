using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Chirp.Infrastructure.Data;

using Microsoft.Extensions.DependencyInjection;
using Chirp.Domain.Entities;

namespace Chirp.Razor.Tests //we use the same file-based Sqlite here so the app and test share the same schema
{
    public class IntegrationTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly HttpClient _client;
        private readonly SqliteConnection _connection;
        
        public IntegrationTests(WebApplicationFactory<Program> fixture)
        {

            Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Testing");
            Environment.SetEnvironmentVariable("authentication__github__clientId", "fake-client-id");
            Environment.SetEnvironmentVariable("authentication__github__clientSecret", "fake-client-secret");
            
            _connection = new SqliteConnection("DataSource=:memory:");
            _connection.Open();

            using (var setup = new ChirpDbContext(
                new DbContextOptionsBuilder<ChirpDbContext>()
                    .UseSqlite(_connection)
                    .Options))
            {
                setup.Database.EnsureCreated();
            }

            var factory = fixture.WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    services.AddDbContext<ChirpDbContext>(opts =>
                    opts.UseSqlite(_connection)); // shared memory
                });
            });

            _client = factory.CreateClient();

        }

        [Fact]
        public async Task CanSeePublicTimeline()
        {
   
        // Act
            var response = await _client.GetAsync("/");
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();

        // Assert
            Assert.Contains("Chirp!", content);
            Assert.Contains("Public Timeline", content);
        }
       protected void Dispose() => _connection.Dispose();
    }
    }

