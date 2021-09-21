using System;
using System.Collections.Generic;
using System.Text;

namespace DiscordBot.Helpers
{
    public static class ListExtensions
    {
        public static void Shuffle<T>(this List<T> list, Random rng)
        {
            List<T> listToShuffle = new List<T>(list);
            list.Clear();

            for (int i = listToShuffle.Count - 1; i >= 0; i--)
            {
                int index = rng.Next(0, i);
                T element = listToShuffle[index];
                listToShuffle.RemoveAt(index);
                list.Insert(0, element);

                // If isn't last index
                if (listToShuffle.Count != 0 && index != listToShuffle.Count - 1)
                {
                    int lastIndex = listToShuffle.Count - 1;
                    T lastElement = listToShuffle[lastIndex];
                    listToShuffle.RemoveAt(lastIndex);
                    listToShuffle.Insert(index, lastElement);
                }
            }
        }
    }
}
