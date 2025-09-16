using Chirp.SimpleDB;

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
        var FakeDatabase = Setup();
        var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        var FakeCheep = new Cheep("Tester", "I'm Testing", timestamp);

        //Act
        FakeDatabase.Store(FakeCheep);
        var messagesout = FakeDatabase.Read().ToList();

        //Assert
        Assert.Single(messagesout);
        Assert.Equal("Tester", messagesout[0].Author);
        Assert.Equal("I'm Testing", messagesout[0].Message);
        Assert.Equal(timestamp, messagesout[0].Timestamp);
    }


    [Fact]
    public void ReturnsLastNRecords()
    {

        //Arrange
        var FakeDatabase = Setup();
        var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        var Cheep1 = new Cheep("Tester", "Testing message", timestamp);
        var Cheep2 = new Cheep("Tester2", "Testing message2", timestamp);
        var Cheep3 = new Cheep("Tester3", "Testing message3", timestamp);

        //Act
        FakeDatabase.Store(Cheep1);
        FakeDatabase.Store(Cheep2);
        FakeDatabase.Store(Cheep3);
        var messagesout = FakeDatabase.Read(limit: 2).ToList();

        //Assert
        Assert.Equal(2, messagesout.Count);
        Assert.Equal("Tester2", messagesout[0].Author);
        Assert.Equal("Tester3", messagesout[1].Author);
    }

    [Fact]
    public void ReadOnNonexistentFile_ThrowsFileNotFound()
    {

        //Arrange
        var FakeDatabase = Setup();

        //Act and Assert
        Assert.Throws<FileNotFoundException>(() =>
        {
            FakeDatabase.Read();
        });
    }

    private CsvDatabase<Cheep> Setup() => new CsvDatabase<Cheep>(_tempPath);


    public void Dispose()
    {
        if (File.Exists(_tempPath))
            File.Delete(_tempPath);
    }



}