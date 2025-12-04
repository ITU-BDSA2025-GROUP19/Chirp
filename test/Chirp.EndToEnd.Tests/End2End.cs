using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.Playwright;
using Microsoft.Data.Sqlite;
using Xunit;

namespace Chirp.EndToEnd.Tests
{
    public class End2End : IAsyncLifetime
    {
        private IPlaywright? _playwright;
        private IBrowser? _browser;
        private IBrowserContext? _context;
        private IPage? _page;

        private const string BaseUrl = "http://localhost:5273";
        public async Task InitializeAsync()
        {
            _playwright = await Playwright.CreateAsync();
            _browser = await _playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions { Headless = true });
            _context = await _browser.NewContextAsync(new BrowserNewContextOptions { IgnoreHTTPSErrors = true });
            _page = await _context.NewPageAsync();
        }

        public async Task DisposeAsync()
        {
            try
            {
                if (_page != null) await _page.CloseAsync();
                if (_context != null) await _context.CloseAsync();
                if (_browser != null) await _browser.CloseAsync();
            }
            finally
            {
                _playwright?.Dispose();
            }
        }

        // Test: input is no more than 160 Characters
        [Fact]
        public async Task Cheep_Input_Is_Truncated_At_160_Characters()
        {
            if (_page == null) throw new InvalidOperationException("Page not ready");

            await LoginAsAsync("anna@itu.dk", "Password123");

            var input = _page.Locator("#CheepText");
            await input.WaitForAsync();

            var longMessage = new string('a', 170);
            await input.FillAsync(longMessage);

            var val = await input.InputValueAsync();
            Assert.Equal(160, val.Length);
        }

        // Test: cheep is in UI then   DB
        [Fact]
        public async Task Cheep_Is_Persisted_And_Displayed_For_Author()
        {
            if (_page == null) throw new InvalidOperationException("Page not ready");

            var email = "anna@itu.dk";
            var password = "Password123";
            var expectedAuthor = "Anna";
            var uniqueCheep = $"E2E test {Guid.NewGuid()}";
            string? dbPath = null;

            try
            {
                await LoginAsAsync(email, password);

                var cheepBox = _page.Locator(".cheepbox");
                await cheepBox.WaitForAsync();

                await _page.FillAsync("#CheepText", uniqueCheep);
                await ClickSubmitButtonAsync();
                await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);

                // Checking UI
                var listItem = _page.Locator("li")
                    .Filter(new() { HasText = uniqueCheep })
                    .Filter(new() { HasText = expectedAuthor });

                await listItem.WaitForAsync(new LocatorWaitForOptions { Timeout = 30000 });
                Assert.True(await listItem.IsVisibleAsync());

                // Checking DB
                dbPath = ResolveDatabasePath();
                Assert.True(File.Exists(dbPath));

                var exists = await PollForCheepInDb(dbPath!, uniqueCheep, expectedAuthor);
                Assert.True(exists, $"Cheep '{uniqueCheep}' not found in DB");
            }
            finally
            {
                // Delation of the testing chirp
                
                if (!string.IsNullOrEmpty(dbPath) && File.Exists(dbPath))
                {
                    await CleanupCheepInDb(dbPath, uniqueCheep);
                }
                //uNTIL HERE 
            }
        }

        private async Task LoginAsAsync(string email, string password)
        {
            if (_page == null) return;
            await _page.GotoAsync($"{BaseUrl}/Identity/Account/Login");
            await _page.GetByLabel("Email").FillAsync(email);
            await _page.GetByLabel("Password").FillAsync(password);
            await _page.GetByRole(AriaRole.Button, new() { Name = "Log in", Exact = true }).ClickAsync();
            await _page.WaitForURLAsync(url => !url.Contains("/Login"), new PageWaitForURLOptions { Timeout = 10000 });
        }

        private async Task ClickSubmitButtonAsync()
        {
            if (_page == null) return;

            var shareBtn = _page.GetByRole(AriaRole.Button, new() { Name = "Share" });
            if (await shareBtn.IsVisibleAsync())
            {
                await shareBtn.ClickAsync();
                return;
            }

            var inputBtn = _page.Locator("input[type='submit']");
            if (await inputBtn.IsVisibleAsync())
            {
                await inputBtn.ClickAsync();
                return;
            }

            var genericBtn = _page.Locator(".cheepbox button");
            if (await genericBtn.CountAsync() > 0)
            {
                await genericBtn.First.ClickAsync();
                return;
            }

            throw new Exception("No submit button found");
        }

        private static string ResolveDatabasePath()
        {
            var env = Environment.GetEnvironmentVariable("TEST_DB_PATH");
            if (!string.IsNullOrEmpty(env) && File.Exists(env)) return Path.GetFullPath(env);

            var root = new DirectoryInfo(Directory.GetCurrentDirectory());
            while (root != null && !root.GetFiles("*.sln").Any()) root = root.Parent;

            if (root != null)
            {
                var razorDb = Path.Combine(root.FullName, "src", "Chirp.Razor", "Chirp.db");
                if (File.Exists(razorDb)) return razorDb;

                var webDb = Path.Combine(root.FullName, "src", "Chirp.Web", "Chirp.db");
                if (File.Exists(webDb)) return webDb;
            }
            return "Chirp.db";
        }

        private async Task<bool> PollForCheepInDb(string dbPath, string cheepText, string authorName)
        {
            var connString = $"Data Source={dbPath}";
            var timeout = TimeSpan.FromSeconds(5);
            var start = DateTime.Now;

            while (DateTime.Now - start < timeout)
            {
                try
                {
                    using var conn = new SqliteConnection(connString);
                    await conn.OpenAsync();

                    var cmd = conn.CreateCommand();
                    cmd.CommandText = @"
                        SELECT COUNT(1)
                        FROM Cheeps c
                        JOIN Authors a ON c.AuthorId = a.AuthorId
                        WHERE c.Text = @text AND a.Name = @name";
                    cmd.Parameters.AddWithValue("@text", cheepText);
                    cmd.Parameters.AddWithValue("@name", authorName);

                    var count = (long)(await cmd.ExecuteScalarAsync() ?? 0L);
                    if (count > 0) return true;
                }
                catch { }

                await Task.Delay(100);
            }
            return false;
        }

        private async Task CleanupCheepInDb(string dbPath, string cheepText)
        {
            try
            {
                using var conn = new SqliteConnection($"Data Source={dbPath}");
                await conn.OpenAsync();
                var cmd = conn.CreateCommand();
                cmd.CommandText = "DELETE FROM Cheeps WHERE Text = @text";
                cmd.Parameters.AddWithValue("@text", cheepText);
                await cmd.ExecuteNonQueryAsync();
            }
            catch { }
        }

        private async Task DumpRecentCheeps(string dbPath)
        {
            try
            {
                using var conn = new SqliteConnection($"Data Source={dbPath}");
                await conn.OpenAsync();
                var cmd = conn.CreateCommand();
                cmd.CommandText = @"
                    SELECT c.Text, a.Name
                    FROM Cheeps c
                    LEFT JOIN Authors a ON c.AuthorId = a.AuthorId
                    ORDER BY c.CheepId DESC LIMIT 5";
                using var reader = await cmd.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    var txt = reader.IsDBNull(0) ? "NULL" : reader.GetString(0);
                    var auth = reader.IsDBNull(1) ? "NULL" : reader.GetString(1);
                    Console.WriteLine($"Author: {auth} | Text: {txt}");
                }
            }
            catch { }
        }
    }
}