using Chirp.SimpleDB;

namespace Chirp.CLI.Client.Tests;

public class ProgramTests
{
    [Fact]
    public async Task CheepCommand_ShouldStoreMessage_AndPrintConfirmation()
    {
        // Arrange
        var db = new FakeDatabase<Cheep>();
        var args = new[] { "cheep", "Hello world" };
        var stringWriter = new StringWriter();
        Console.SetOut(stringWriter);

        // Act
        int exitCode = await Program.RunAsync(args, db);

        // Assert
        string output = stringWriter.ToString();
        Assert.Equal(0, exitCode);
        Assert.Contains("Cheep added!", output);
        Assert.Single(db.Read());
    }
    
    [Fact]
    public async Task ReadCommand_ShouldPrintStoredMessages()
    {
        // Arrange
        var db = new FakeDatabase<Cheep>();
        db.Store(new Cheep("tester", "Unit test msg", DateTimeOffset.UtcNow.ToUnixTimeSeconds()));

        var args = new[] { "read" };
        var stringWriter = new StringWriter();
        Console.SetOut(stringWriter);

        // Act
        int exitCode = await Program.RunAsync(args, db);

        // Assert
        string output = stringWriter.ToString();
        Assert.Equal(0, exitCode);
        Assert.Contains("Unit test msg", output);
    }

    [Fact]
    public async Task ReadCommand_ShouldHandleEmptyDatabase()
    {
        // Arrange
        var db = new FakeDatabase<Cheep>();
        var args = new[] { "read" };
        var stringWriter = new StringWriter();
        Console.SetOut(stringWriter);

        // Act
        int exitCode = await Program.RunAsync(args, db);

        // Assert
        string output = stringWriter.ToString();
        Assert.Equal(0, exitCode);
        Assert.True(string.IsNullOrWhiteSpace(output));
    }

    [Fact]
    public async Task CheepCommand_ShouldFail_WhenMessageIsMissing()
    {
        // Arrange
        var db = new FakeDatabase<Cheep>();
        var args = new[] { "cheep" }; // missing message
        var stringWriter = new StringWriter();
        Console.SetOut(stringWriter);

        // Act
        int exitCode = await Program.RunAsync(args, db);

        // Assert
        string output = stringWriter.ToString();
        Assert.NotEqual(0, exitCode);              // System.CommandLine signals error
        Assert.Contains("Usage:", output);         // help text shown
        Assert.Contains("cheep", output);          // command name appears
    }
}

public class FakeDatabase<T> : IDatabaseRepository<T>
{
    private readonly List<T> _items = new();
    public IEnumerable<T> Read(int? limit = null) => _items;
    public void Store(T record) => _items.Add(record);
}


