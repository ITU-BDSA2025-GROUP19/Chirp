using System;
using System.Collections.Generic;

public static class UI {
    public static void PrintCheeps(var messagesOut) {
        foreach (var message in messagesOut) {
                var dateFormatted = DateTimeOffset.FromUnixTimeSeconds(message.Timestamp).UtcDateTime;
                Console.WriteLine($"{message.Author} @ {dateFormatted} @ {message.Message}");
        }
    }
}