using Chirp.SimpleDB;

public interface ICheepService
{
    public List<Cheep> GetCheeps();
    public List<Cheep> GetCheepsFromAuthor(string author);
}

public class CheepService : ICheepService
{
    public List<Cheep> GetCheeps()
    {
        Console.WriteLine($"[CheepService] Reading cheeps from: {Path.GetFullPath("chirp_cli_db.csv")}");
        return CsvDatabase<Cheep>.Instance.Read(100).ToList();
    }

    public List<Cheep> GetCheepsFromAuthor(string author)
    {
        Console.WriteLine($"[CheepService] Reading cheeps for author {author} from: {Path.GetFullPath("chirp_cli_db.csv")}");
        return CsvDatabase<Cheep>.Instance.Read()
            .Where(x => x.Author == author)
            .ToList();
    }

    public static string UnixTimeStampToDateTimeString(double unixTimeStamp)
    {
        DateTime dateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
        dateTime = dateTime.AddSeconds(unixTimeStamp);
        return dateTime.ToString("MM/dd/yy H:mm:ss");
    }
}