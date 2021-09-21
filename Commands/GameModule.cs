using DiscordBot.Handlers;
using DiscordBot.Helpers;
using DiscordBot.Hitler;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Exceptions;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.Extensions;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace DiscordBot.Commands
{
    public class GameModule : BaseCommandModule
    {
        [Command("join"), RequireGuild()]
        public async Task Join(CommandContext ctx)
        {
            Game game = Game.Get(ctx.Guild.Id);

            if (!game.Join(ctx.Member)) // Todo: Replace this shit with custom status response struct or smth like that
            {
                await MessageHelper.TimedResponseAsync(ctx, game.GetString("hitler-alreadyjoined")/*"You already joined!"*/, 3f, true);
                return;
            }

            await MessageHelper.TimedResponseAsync(ctx, game.GetString("hitler-successjoin"), 3f, true);
        }

        [Command("leave"), RequireGuild()]
        public async Task Leave(CommandContext ctx)
        {
            Game game = Game.Get(ctx.Guild.Id);

            if (!game.Leave(ctx.Member))
            {
                await MessageHelper.TimedResponseAsync(ctx, game.GetString("hitler-alreadyleft"), 3f, true);
                return;
            }

            await MessageHelper.TimedResponseAsync(ctx, game.GetString("hitler-successleave"), 3f, true);
        }

        [Command("setup"), RequireGuild()]
        public async Task Setup(CommandContext ctx)
        {
            Game game = Game.Get(ctx.Guild.Id);

            await ctx.Channel.SendMessageAsync(game.GetString("hitler-setupstart"));
            if (!game.Setup())
            {
                await ctx.RespondAsync("Minimum players: 5 - Maximum players: 10"); // Todo: Replace this shit with custom status response struct or smth like that
                return;
            }

            foreach (Player player in game.Players)
            {
                string roleName = player.gameRole == GameRole.Liberal ? game.GetString("hitler-liberal") : player.isHitler ? game.GetString("hitler-hitler") : game.GetString("hitler-fascist");
                try
                {
                    await player.member.SendMessageAsync(game.GetStringFormat("hitler-showrole", roleName));
                }
                catch (UnauthorizedException)
                {
                    await ctx.Channel.SendMessageAsync(game.GetStringFormat("hitler-dmblocked", player.member.Mention));
                    return;
                }
            }


            await ctx.Channel.SendMessageAsync(game.GetString("hitler-setupend"));
        }

        [Command("start"), RequireGuild()]
        public async Task Start(CommandContext ctx)
        {
            Game game = Game.Get(ctx.Guild.Id);

            StatusResponse check = game.CanStart(ctx);
            if (!check.success)
            {
                await ctx.Channel.SendMessageAsync(game.GetString(check.errorId));
                return;
            }

            List<DiscordOverwriteBuilder> dobList = new List<DiscordOverwriteBuilder>();
            dobList.Add(new DiscordOverwriteBuilder(ctx.Guild.EveryoneRole) { Denied = Permissions.AccessChannels }); // Everyone shouldn't see this channel except players
            foreach (Player player in game.Players)
            {
                DiscordOverwriteBuilder dob = new DiscordOverwriteBuilder(player.member)
                {
                    Allowed = Permissions.AccessChannels
                };
                dobList.Add(dob);
            }
            DiscordChannel gameChannel = await ctx.Guild.CreateChannelAsync(game.GetString("hitler-hitler"), ChannelType.Text, null, default, null, null, dobList);
            DiscordChannel voiceChannel = ctx.Member.VoiceState.Channel;

            await game.StartAsync(gameChannel, voiceChannel);

            await ctx.Channel.SendMessageAsync($"Game started! Navigate to {gameChannel.Mention}");
        }

        [Command("setuptest"), Hidden(), RequireOwner()]
        public async Task SetupTest(CommandContext ctx)
        {
            Game game = Game.Get(ctx.Guild.Id);

            await ctx.RespondAsync("Setting game. Please wait");
            if (!game.TestSetup())
            {
                await ctx.RespondAsync("Error occured while setting game!");
                return;
            }

            StringBuilder sb = new StringBuilder();
            int i = 1;
            foreach (Player player in game.Players)
            {
                string roleName = player.gameRole == GameRole.Liberal ? "Liberal" : player.isHitler ? "Hitler" : "Fascist";
                sb.AppendLine($"Player {i} playing as {roleName}.");

                i++;
            }

            await ctx.RespondAsync(sb.ToString());
            try
            {
                await ctx.Member.SendMessageAsync(sb.ToString());
            }
            catch (UnauthorizedException)
            {
                await ctx.Channel.SendMessageAsync($"Looks like {ctx.Member.Mention} have dm closed or blocked me :(");
                return;
            }

            //START

            //if (ctx.Member.VoiceState?.Channel is null)
            //{
            //    await ctx.Channel.SendMessageAsync("You are not in voice channel");
            //    return;
            //}

            List<DiscordOverwriteBuilder> dobList = new List<DiscordOverwriteBuilder>
            {
                new DiscordOverwriteBuilder(ctx.Guild.EveryoneRole) { Denied = Permissions.AccessChannels },
                new DiscordOverwriteBuilder(ctx.Member) { Allowed = Permissions.AccessChannels }
            };

            DiscordChannel gameChannel = await ctx.Guild.CreateChannelAsync("Hitler", ChannelType.Text, null, default, null, null, dobList);
            DiscordChannel voiceChannel = ctx.Member.VoiceState?.Channel ?? null;
            InteractivityExtension interactivity = ctx.Client.GetInteractivity();

            await game.StartTestAsync(gameChannel, voiceChannel, interactivity);

            await ctx.Channel.SendMessageAsync($"Game started! Navigate to {gameChannel.Mention}");
        }
    }
}
