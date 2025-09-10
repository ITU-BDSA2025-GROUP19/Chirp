using System.CommandLine;
using SimpleDB;


namespace Chirp.CLI;

public class Program
{
    public static async Task<int> Main(string[] args)
    {
        var databaseRepository = new CsvDatabase<Cheep>();

        var rootCommand = new RootCommand("Chirp (X formally known as Twitter) ");
        
        // Read
        var readCommand = new Command("read", "Show all cheeps");
        readCommand.SetHandler(() =>
        {
            var messagesOut = databaseRepository.Read();
            foreach (var message in messagesOut)
            {
                var dateFormatted = DateTimeOffset.FromUnixTimeSeconds(message.Timestamp).UtcDateTime;
                Console.WriteLine($"{message.Author} @ {dateFormatted} @ {message.Message}");
            }
        });

        // Cheep
        var cheepCommand = new Command("cheep", "Add a new cheep");
        var messageArg = new Argument<string>("message", "Message to cheep");
        cheepCommand.AddArgument(messageArg);
        cheepCommand.SetHandler((message) =>
        {
            string currentUser = Environment.UserName;
            long currentTimestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            var cheep = new Cheep(currentUser, message, currentTimestamp);
            
            databaseRepository.Store(cheep);

            Console.WriteLine("Cheep added!");
        }, messageArg);

        rootCommand.AddCommand(readCommand);
        rootCommand.AddCommand(cheepCommand);

        return await rootCommand.InvokeAsync(args);
    }
}
