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
    public class EqualizerModeConverter : IArgumentConverter<EqualizerMode>
    {
        public Task<Optional<EqualizerMode>> ConvertAsync(string value, CommandContext ctx)
        {
            switch (value)
            {
                case "no":
                case "off":
                case "none":
                case "default":
                    return Task.FromResult(Optional.FromValue(EqualizerMode.Default));
                case "bassboosted":
                case "bb":
                case "bass":
                    return Task.FromResult(Optional.FromValue(EqualizerMode.BassBoosted));
                case "test":
                    return Task.FromResult(Optional.FromValue(EqualizerMode.Test));
                default:
                    return Task.FromResult(Optional.FromNoValue<EqualizerMode>());
            }
        }
    }
}
