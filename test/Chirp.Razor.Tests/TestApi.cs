using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;
using System.IO;
using System;

namespace Chirp.Razor.Tests
{
    public class LegacyDbTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly HttpClient _client;

        public LegacyDbTests(WebApplicationFactory<Program> fixture)
        {
            var baseDir = AppContext.BaseDirectory;
            var targetPath = Path.Combine(baseDir, "chirp.db");
            var sourcePath = Path.Combine("..", "..", "..", "..", "src", "Chirp.Razor", "chirp.db");

            if (File.Exists(sourcePath))
            {
                File.Copy(sourcePath, targetPath, overwrite: true);
                Environment.SetEnvironmentVariable("CHIRPDBPATH", targetPath);
            }

            _client = fixture.CreateClient(new WebApplicationFactoryClientOptions
            {
                AllowAutoRedirect = true,
                HandleCookies = true
            });
        }

        [Fact]
        public async Task CanSeePublicTimeline()
        {
            var response = await _client.GetAsync("/");
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();

            Assert.Contains("Chirp!", content);
            Assert.Contains("Public Timeline", content);
        }

        [Theory]
        [InlineData("alexm")]
        [InlineData("adho")]
        public async Task CanSeePrivateTimeline(string author)
        {
            var response = await _client.GetAsync($"/{author}");
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();

            Assert.Contains("Chirp!", content);
            Assert.Contains($"{author}'s Timeline", content);
        }
    }

    public class SqlSeededTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly HttpClient _client;
        private readonly string _dbPath;

        public SqlSeededTests(WebApplicationFactory<Program> fixture)
        {
            var baseDir = AppContext.BaseDirectory;
            _dbPath = Path.Combine(baseDir, $"chirp_test_{Guid.NewGuid()}.db");

            var schemaPath = Path.Combine(baseDir, "schema.sql");
            var dumpPath = Path.Combine(baseDir, "dump.sql");

            using (var connection = new SqliteConnection($"Data Source={_dbPath}"))
            {
                connection.Open();

                // Apply schema
                var schemaSql = File.ReadAllText(schemaPath);
                foreach (var stmt in schemaSql.Split(';', StringSplitOptions.RemoveEmptyEntries))
                {
                    var trimmed = stmt.Trim();
                    if (string.IsNullOrWhiteSpace(trimmed)) continue;

                    using var cmd = connection.CreateCommand();
                    cmd.CommandText = trimmed + ";";
                    cmd.ExecuteNonQuery();
                }

                // Apply dump
                var dumpSql = File.ReadAllText(dumpPath);
                using var dumpCmd = connection.CreateCommand();
                dumpCmd.CommandText = dumpSql;
                dumpCmd.ExecuteNonQuery();
            }

            Environment.SetEnvironmentVariable("CHIRPDBPATH", _dbPath);

            _client = fixture.CreateClient(new WebApplicationFactoryClientOptions
            {
                AllowAutoRedirect = true,
                HandleCookies = true
            });
        }

        // Search all public pages one by one for expected text
        private async Task AssertPublicTimelineContains(string author, string message)
        {
            int totalCheeps;
            using (var connection = new SqliteConnection($"Data Source={_dbPath}"))
            {
                connection.Open();
                using var cmd = connection.CreateCommand();
                cmd.CommandText = "SELECT COUNT(*) FROM message;";
                totalCheeps = Convert.ToInt32(cmd.ExecuteScalar());
            }

            int pageSize = 32;
            int totalPages = (totalCheeps + pageSize - 1) / pageSize;

            for (int page = 1; page <= totalPages; page++)
            {
                var response = await _client.GetAsync($"/?page={page}");
                response.EnsureSuccessStatusCode();
                var content = await response.Content.ReadAsStringAsync();

                if (content.Contains(author) && content.Contains(message))
                {
                    return; //Successfully found the cheep
                }
            }

            Assert.Fail($"{author}'s cheep not found in any public timeline page");
            //The cheep author was not found
        }

        //Public timeline tests
        [Fact]
        public async Task PublicTimeline_ShouldContain_Helge_Cheep()
            => await AssertPublicTimelineContains("Helge", "Hello, BDSA students!");

        [Fact]
        public async Task PublicTimeline_ShouldContain_Quintin_Cheep()
            => await AssertPublicTimelineContains("Quintin Sitts", "Swim away from your contemporary consciousness.");

        [Fact]
        public async Task PublicTimeline_ShouldContain_Jacqualine_Cheep()
            => await AssertPublicTimelineContains("Jacqualine Gilcoine", "The more terrible, therefore, seemed that some of his feet.");

        //Private timeline tests
        [Fact]
        public async Task AdrianTimeline_ShouldContain_Adrian_Cheep()
        {
            var response = await _client.GetAsync("/Adrian");
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();

            Assert.Contains("Adrian", content);
            Assert.Contains("Hej, velkommen til kurset", content);
        }

        [Fact]
        public async Task JohnnieTimeline_ShouldContain_Johnnie_Cheep()
        {
            var response = await _client.GetAsync("/Johnnie Calixto");
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();

            Assert.Contains("Johnnie Calixto", content);
            Assert.Contains("Mrs. Straker tells us that his mates thanked God the direful disorders seemed waning.", content);
        }

        [Fact]
        public async Task QuintinTimeline_ShouldContain_Quintin_Cheep()
        {
            var response = await _client.GetAsync("/Quintin Sitts");
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();

            Assert.Contains("Quintin Sitts", content);
            Assert.Contains("What did they take?", content);
        }

}

    }

