using System;
using System.Collections.Generic;
using System.Text;

namespace DiscordBot.Helpers
{
    public static class EnumExtensions
    {
        public static T Next<T>(this T source) where T : Enum
        {
            T[] values = (T[])Enum.GetValues(typeof(T));
            int next = Array.IndexOf(values, source) + 1;

            return next == values.Length ? values[0] : values[next];
        }
    }
}
