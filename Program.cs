﻿using System.Globalization;
using CsvHelper;

namespace Chirp.CLI;

public class Program
{
    public static void Main(string[] args)
    {
        var filepath = "chirp_cli_db.csv";
        
        if (args.Length == 0)
        {
            Console.WriteLine("You must also write an argument. Options:");
            Console.WriteLine("  read              - Show all cheeps");
            Console.WriteLine("  cheep <message>   - Add a new cheep");
            return;
        }
        
        if (args[0] == "read")
        {
            using var reader = new StreamReader(filepath);
            using var csvReader = new CsvReader(reader, CultureInfo.InvariantCulture); 
            var messagesOut = csvReader.GetRecords<Cheep>(); 
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
            var cheep = new Cheep( currentUser, args[1], (long)currentTimestamp );
            messagesIn.Add(cheep);
            using var writer = new StreamWriter(filepath, false);
            using var csvWriter = new CsvWriter(writer, CultureInfo.InvariantCulture);
            csvWriter.WriteRecords(messagesIn);
        }
    }
}




public record Cheep(string Author, string Message, long Timestamp);