using System.CommandLine;
using Chirp.SimpleDB;

namespace Chirp.CLI;

public class Program
{
    public static Task<int> Main(string[] args) =>
        RunAsync(args, CsvDatabase<Cheep>.Instance);
    public static async Task<int> RunAsync(string[] args, IDatabaseRepository<Cheep> databaseRepository)
    {
        var rootCommand = new RootCommand("Chirp (X formally known as Twitter) ");

        var readCommand = new Command("read", "Show all cheeps");
        readCommand.SetHandler(() =>
        {
            var messagesOut = databaseRepository.Read();
            UI.PrintCheeps(messagesOut);
        });

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
