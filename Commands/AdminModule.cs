using DiscordBot.Handlers;
using DiscordBot.Helpers;
using DiscordBot.Localization;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace DiscordBot.Commands
{
    class AdminModule : BaseCommandModule
    {
        [Command("language"), RequirePermissions(Permissions.Administrator)]
        public async Task ChangeLanguage(CommandContext ctx, Language language)
        {
            Settings settings = Settings.Get(ctx.Guild.Id);
            settings.SelectLanguage(language);
            await ctx.RespondAsync(settings.GetLocalString("settings-successlanguage"));
        }

        [Command("purge"), RequirePermissions(Permissions.Administrator)]
        public async Task Purge(CommandContext ctx, int count)
        {
            IReadOnlyList<DiscordMessage> msgs = await ctx.Channel.GetMessagesBeforeAsync(ctx.Message.Id, count);
            foreach (DiscordMessage msg in msgs)
            {
                await msg.DeleteAsync();
            }

            await MessageHelper.TimedSendMsgAsync(ctx, $"Purged {msgs.Count} messages", 4, true);
        }
    }
}
