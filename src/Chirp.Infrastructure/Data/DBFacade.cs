using Chirp.Domain.Entities;
using Microsoft.Data.Sqlite;

namespace Chirp.Infrastructure.Data;

//TODO: remove legacy DBFacade after session 6 EF Core refactor
/* public class DBFacade
{
    private readonly string _dbPath;
    public DBFacade()
    {
        var envPath = Environment.GetEnvironmentVariable("CHIRPDBPATH");

        if (!string.IsNullOrEmpty(envPath))
        {
            _dbPath = envPath;
        }
        else
        {
            var baseDir = AppContext.BaseDirectory;
            _dbPath = Path.Combine(baseDir, "chirp.db");
        }
    }

    public List<Cheep> GetCheeps(int limit = 1000)
    {
        using var connection = new SqliteConnection($"Data Source={_dbPath}");
        connection.Open();
        var cheeps = new List<Cheep>();
        var command = connection.CreateCommand();
        command.CommandText = $@"select m.pub_date, m.message_id, m.author_id, m.text, u.username from message m 
        join user u on m.author_id = u.user_id where author_id = user_id order by m.pub_date desc limit {limit}";

        var reader = command.ExecuteReader();

        while (reader.Read())
        {
            int pubDate = reader.GetInt32(0);
            int messageId = reader.GetInt32(1);
            int authorId = reader.GetInt32(2);
            string text = reader.GetString(3);
            string username = reader.GetString(4);

            cheeps.Add(new Cheep(messageId, authorId, text, pubDate, username));
        }
        return cheeps;
    }

    public List<Cheep> GetCheepsByAuthor(string author)
    {
        using var connection = new SqliteConnection($"Data Source={_dbPath}");
        connection.Open();
        var cheeps = new List<Cheep>();
        var command = connection.CreateCommand();
        command.CommandText = $@"select m.pub_date, m.message_id, m.author_id, m.text, u.username from message m join user u
        on m.author_id = u.user_id where u.username = $author order by m.pub_date desc";

        command.Parameters.AddWithValue("$author", author);

        var reader = command.ExecuteReader();
        while (reader.Read())
        {
            int pubDate = reader.GetInt32(0);
            int messageId = reader.GetInt32(1);
            int authorId = reader.GetInt32(2);
            string text = reader.GetString(3);
            string username = reader.GetString(4);

            cheeps.Add(new Cheep(messageId, authorId, text, pubDate, username));
        }
        return cheeps;
    }
}
*/