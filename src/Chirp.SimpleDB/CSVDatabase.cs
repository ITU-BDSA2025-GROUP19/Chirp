using CsvHelper;
using System.Globalization;

namespace Chirp.SimpleDB;

public sealed class CsvDatabase<T> : IDatabaseRepository<T>
{
    private readonly string _filePath = Path.Combine(AppContext.BaseDirectory, "chirp_cli_db.csv");

    public CsvDatabase(string filePath) //Used for testing purposes only
    {
        _filePath = filePath;
    }
    private static CsvDatabase<T>? _instance;
    private CsvDatabase()
    {
    }

    public static CsvDatabase<T> Instance
    {
        get
        {
            // If _instance is null, a new CsvDatabase<T>() is created and assigned to it.
            // If _instance is not null, its current value is used.
            _instance ??= new CsvDatabase<T>();
            return _instance;
        }
    }
    public IEnumerable<T> Read(int? limit = null)
    {
        using var reader = new StreamReader(_filePath);
        using var csvReader = new CsvReader(reader, CultureInfo.InvariantCulture);
        var messagesOut = csvReader.GetRecords<T>().ToList();
        if (limit.HasValue)
        {
            return messagesOut.TakeLast(limit.Value);
        }
        else
        {
            return messagesOut;
        }
        
    }
    public void Store(T record)
    {
        var existingMessages = File.Exists(_filePath)
        ? this.Read().ToList()
        : new List<T>();

        existingMessages.Add(record);

        using var writer = new StreamWriter(_filePath, false);
        using var csvWriter = new CsvWriter(writer, CultureInfo.InvariantCulture);
        csvWriter.WriteRecords(existingMessages);
        
    }
}