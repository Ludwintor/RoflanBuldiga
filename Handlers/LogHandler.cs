using DSharpPlus;
using DSharpPlus.EventArgs;
using DSharpPlus.CommandsNext;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext.Exceptions;
using DSharpPlus.Entities;

namespace DiscordBot.Handlers
{
    public class LogHandler
    {
        private static readonly EventId botId = new EventId(420, "RoflBuldiga");
        private static DiscordClient discord;

        public LogHandler(DiscordClient discord, CommandsNextExtension commands)
        {
            LogHandler.discord = discord;
            discord.Ready += Discord_Ready;
            discord.GuildDownloadCompleted += Discord_GuildDownloadCompleted;
            discord.ClientErrored += Discord_ClientErrored;

            commands.CommandExecuted += Commands_CommandExecuted;
            commands.CommandErrored += Commands_CommandErrored;
        }

        public static void Log(string message, LogLevel logLevel = LogLevel.Information)
        {
            discord.Logger.Log(logLevel, botId, message);
        }

        private Task Discord_ClientErrored(DiscordClient sender, ClientErrorEventArgs e)
        {
            sender.Logger.LogError(botId, e.Exception, "Error occured");

            return Task.CompletedTask;
        }

        private Task Discord_GuildDownloadCompleted(DiscordClient sender, GuildDownloadCompletedEventArgs e)
        {
            sender.Logger.LogInformation(botId, "All guilds are available");

            return Task.CompletedTask;
        }

        private Task Discord_Ready(DiscordClient sender, ReadyEventArgs e)
        {
            sender.Logger.LogInformation(botId, "Client responds to requests");

            return Task.CompletedTask;
        }

        private Task Commands_CommandExecuted(CommandsNextExtension sender, CommandExecutionEventArgs e)
        {
            e.Context.Client.Logger.LogInformation(botId, $"{e.Context.User.Username} successfully executed '{e.Command.QualifiedName}'");

            return Task.CompletedTask;
        }

        private async Task Commands_CommandErrored(CommandsNextExtension sender, CommandErrorEventArgs e)
        {
            e.Context.Client.Logger.LogError(botId, $"{e.Context.User.Username} tried executing '{e.Command?.QualifiedName ?? "<unknown command>"}' but it errored: {e.Exception.GetType()}: {e.Exception.Message ?? "<no message>"}");

            if (e.Exception is ChecksFailedException)
            {
                DiscordEmoji emoji = DiscordEmoji.FromName(e.Context.Client, ":no_entry:");

                DiscordEmbedBuilder embed = new DiscordEmbedBuilder
                {
                    Title = "Access denied",
                    Description = $"{emoji} You do not have the permissions required to execute this command.",
                    Color = DiscordColor.Red
                };
                await e.Context.RespondAsync(embed);
            }
        }
    }
}
