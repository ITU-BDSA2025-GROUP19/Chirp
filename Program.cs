using System;
using System.Globalization;
using System.IO;
using CsvHelper;
using System.Collections.Generic;

public class Program
{
    public static void Main(string[] args)
    {
        var filepath = "chirp_cli_db.csv";

        if (args[0] == "read")
        {
            using var reader = new StreamReader(filepath);
            using var csvReader = new CsvReader(reader, CultureInfo.InvariantCulture); 
            var messagesOut = csvReader.GetRecords<ChirpOutput>(); 
            foreach (var message in messagesOut)
            { 
                var dateFormatted = DateTimeOffset.FromUnixTimeSeconds(message.Timestamp).UtcDateTime;
                Console.WriteLine($"{message.Author} @ {dateFormatted} @ {message.Message}");
            }
        }
        else if (args[0] == "cheep")
        {
            var messagesIn = new List<Cheep>();
            if (File.Exists(filepath))
            {
                using var reader = new StreamReader(filepath);
                using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);
                messagesIn.AddRange(csv.GetRecords<Cheep>());
            }
            
            string currentUser = Environment.UserName;
            long currentTimestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

            messagesIn.Add(new Cheep { Author = currentUser, Message = args[1], Timestamp = (int)currentTimestamp });
            using var writer = new StreamWriter(filepath, false);
            using var csvWriter = new CsvWriter(writer, CultureInfo.InvariantCulture);
            csvWriter.WriteRecords(messagesIn);
        }
    }
}



public class ChirpOutput
{
    public required string Author { get; set; }
    public required string Message { get; set; }
    public int Timestamp { get; set; }
}

public class Cheep
{    
    public required string Author { get; set; }
    public required string Message { get; set; }
    public int Timestamp { get; set; }
}