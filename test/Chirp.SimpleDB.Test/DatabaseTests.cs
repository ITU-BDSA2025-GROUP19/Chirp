namespace Chirp.SimpleDB.Tests;

public class DatabaseTests : IDisposable
{
    private readonly string _tempPath;

    public DatabaseTests()
    {
        _tempPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString() + ".csv"); //Temp filePath used for every test
    }

    [Fact]
    public void StoreAndReadRecords()
    {
        //Arrange
        var fakeDatabase = Setup();
        var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        var fakeCheep = new Cheep("Tester", "I'm Testing", timestamp);

        //Act
        fakeDatabase.Store(fakeCheep);
        var messagesOut = fakeDatabase.Read().ToList();

        //Assert
        Assert.Single(messagesOut);
        Assert.Equal("Tester", messagesOut[0].Author);
        Assert.Equal("I'm Testing", messagesOut[0].Message);
        Assert.Equal(timestamp, messagesOut[0].Timestamp);
    }

    [Fact]
    public void ReturnsLastNRecords()
    {

        //Arrange
        var fakeDatabase = Setup();
        var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        var cheep1 = new Cheep("Tester", "Testing message", timestamp);
        var cheep2 = new Cheep("Tester2", "Testing message2", timestamp);
        var cheep3 = new Cheep("Tester3", "Testing message3", timestamp);

        //Act
        fakeDatabase.Store(cheep1);
        fakeDatabase.Store(cheep2);
        fakeDatabase.Store(cheep3);
        var messagesOut = fakeDatabase.Read(limit: 2).ToList();

        //Assert
        Assert.Equal(2, messagesOut.Count);
        Assert.Equal("Tester2", messagesOut[0].Author);
        Assert.Equal("Tester3", messagesOut[1].Author);
    }

    [Fact]
    public void ReadOnNonexistentFile_ThrowsFileNotFound()
    {

        //Arrange
        var fakeDatabase = Setup();

        //Act and Assert
        Assert.Throws<FileNotFoundException>(() =>
        {
            fakeDatabase.Read();
        });
    }

    private CsvDatabase<Cheep> Setup() => new CsvDatabase<Cheep>(_tempPath);

    public void Dispose()
    {
        if (File.Exists(_tempPath))
            File.Delete(_tempPath);
    }
}