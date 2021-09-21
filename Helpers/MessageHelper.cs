using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace DiscordBot.Helpers
{
    public static class MessageHelper
    {
        public async static Task TimedResponseAsync(CommandContext ctx, string message, float delay = 3f, bool deleteTriggerAlso = false)
        {
            DiscordMessage msg = await ctx.RespondAsync(message);
            Action action = () => msg.DeleteAsync();
            if (deleteTriggerAlso)
                action += () => ctx.Message.DeleteAsync();

            Delay.New(delay, action);
        }

        public async static Task TimedResponseAsync(CommandContext ctx, DiscordEmbed embed, float delay = 3f, bool deleteTriggerAlso = false)
        {
            DiscordMessage msg = await ctx.RespondAsync(embed);
            Action action = () => msg.DeleteAsync();
            if (deleteTriggerAlso)
                action += () => ctx.Message.DeleteAsync();

            Delay.New(delay, action);
        }

        public async static Task TimedSendMsgAsync(CommandContext ctx, string message, float delay = 3f, bool deleteTriggerAlso = false)
        {
            if (ctx.Channel == null)
                return;

            DiscordMessage msg = await ctx.Channel.SendMessageAsync(message);
            Action action = () => msg.DeleteAsync();
            if (deleteTriggerAlso)
                action += () => ctx.Message.DeleteAsync();

            Delay.New(delay, action);
        }

        public async static Task TimedSendMsgAsync(CommandContext ctx, DiscordEmbed embed, float delay = 3f, bool deleteTriggerAlso = false)
        {
            if (ctx.Channel == null)
                return;

            DiscordMessage msg = await ctx.Channel.SendMessageAsync(embed);
            Action action = () => msg.DeleteAsync();
            if (deleteTriggerAlso)
                action += () => ctx.Message.DeleteAsync();

            Delay.New(delay, action);
        }
    }
}
