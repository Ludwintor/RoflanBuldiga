using DiscordBot.Localization;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Converters;
using DSharpPlus.Entities;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace DiscordBot.Converters
{
    public class LanguageConverter : IArgumentConverter<Language>
    {
        public Task<Optional<Language>> ConvertAsync(string value, CommandContext ctx)
        {
            return value.ToLower() switch
            {
                "eng" => Task.FromResult(Optional.FromValue(Language.ENG)),
                "rus" => Task.FromResult(Optional.FromValue(Language.RUS)),
                _ => Task.FromResult(Optional.FromNoValue<Language>())
            };
        }
    }
}
