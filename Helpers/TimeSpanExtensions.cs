using System;
using System.Collections.Generic;
using System.Text;

namespace DiscordBot.Helpers
{
    public static class TimeSpanExtensions
    {
        public static string ToReadable(this TimeSpan timeSpan)
        {
            if (timeSpan.TotalHours >= 1)
                return timeSpan.ToString(@"hh\:mm\:ss");

            return timeSpan.ToString(@"mm\:ss");
        }
    }
}
