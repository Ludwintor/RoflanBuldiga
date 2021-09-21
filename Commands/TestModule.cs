using DiscordBot.Helpers;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiscordBot.Commands
{
    public class TestModule : BaseCommandModule
    {
        private int lol = 0;

        [Command("test"), RequireOwner(), Hidden()]
        public async Task Test(CommandContext ctx)
        {
            lol++;
            await ctx.RespondAsync(lol.ToString());
        }

        [Command("embedtest"), RequireOwner(), Hidden()]
        public async Task PreviewEmbed(CommandContext ctx, int size = 1024)
        {
            DiscordEmbedBuilder embed = new DiscordEmbedBuilder();

            embed.WithAuthor("Author name", "https://author-url.com", "https://cdn.discordapp.com/attachments/697344057764216845/843899488283787355/1621043912.jpg");
            embed.WithColor(DiscordColor.Red);
            embed.WithDescription("Description");

            embed.AddField("Field 1 Inline", "Field 1 Value", true);
            embed.AddField("Field 2 Inline", "Field 2 Value", true);
            embed.AddField("Field 3", "Field 3 Value", false);
            embed.AddField("Field 4", "Field 4 Value", false);
            embed.AddField("Field 5 Inline", "Field 5 Value", true);
            embed.AddField("Field 6", "Field 6 Value", false);

            embed.WithFooter("Footer text", "https://cdn.discordapp.com/attachments/697344057764216845/839484495770288168/unknown.png");
            embed.WithImageUrl("https://cdn.discordapp.com/attachments/697344057764216845/839196236992741376/latest.png");
            embed.WithThumbnail("https://cdn.discordapp.com/attachments/697344057764216845/837359343556493372/unknown.png", size, size);
            embed.WithTimestamp(DateTime.UtcNow);
            embed.WithTitle("Title :white_circle:");
            embed.WithUrl("https://title-url.com");

            await ctx.Channel.SendMessageAsync(embed.Build());
        }

        [Command("clear"), RequireOwner(), Hidden()]
        public async Task Clear(CommandContext ctx)
        {
            IEnumerable<DiscordChannel> hitlerChannels = ctx.Guild.Channels.Values.Where(x => x.Name == "hitler");
            foreach (DiscordChannel channel in hitlerChannels)
                await channel.DeleteAsync();

            await MessageHelper.TimedSendMsgAsync(ctx, $"Deleted {hitlerChannels.Count()} channels", 4, true);
        }
    }
}
