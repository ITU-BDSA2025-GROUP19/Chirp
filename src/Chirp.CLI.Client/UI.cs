using System;
using System.Collections.Generic;
using Chirp.SimpleDB;

public static class UI {
    public static void PrintCheeps(IEnumerable<Cheep> messagesOut) {
        foreach (var message in messagesOut) {
                var dateFormatted = DateTimeOffset.FromUnixTimeSeconds(message.Timestamp).UtcDateTime;
                Console.WriteLine($"{message.Author} @ {dateFormatted} @ {message.Message}");
        }
    }
}