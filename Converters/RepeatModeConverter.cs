using DiscordBot.Music;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Converters;
using DSharpPlus.Entities;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace DiscordBot.Converters
{
    public class RepeatModeConverter : IArgumentConverter<RepeatMode>
    {
        public Task<Optional<RepeatMode>> ConvertAsync(string value, CommandContext ctx)
        {
            switch (value.ToLower())
            {
                case "n":
                case "no":
                case "none":
                case "off":
                    return Task.FromResult(Optional.FromValue(RepeatMode.None));
                case "all":
                    return Task.FromResult(Optional.FromValue(RepeatMode.All));
                case "single":
                case "one":
                    return Task.FromResult(Optional.FromValue(RepeatMode.Single));
                default:
                    return Task.FromResult(Optional.FromNoValue<RepeatMode>());
            }
        }
    }
}
