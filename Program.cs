using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using DiscordBot.Helpers;

namespace DiscordBot
{
    class Program
    {
        private static void Main(string[] args) => Startup.RunAsync(args).GetAwaiter().GetResult();
    }
}
