using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.CommandsNext.Converters;
using DSharpPlus.CommandsNext.Entities;
using DSharpPlus.Entities;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using DiscordBot.Helpers;
using DSharpPlus;

namespace DiscordBot
{
    public class HelpFormatter : BaseHelpFormatter
    {
        private string Prefix { get; }
        private DiscordEmbedBuilder Embed { get; }

        public HelpFormatter(CommandContext ctx) : base(ctx)
        {
            Prefix = ctx.Prefix;
            Embed = EmbedHelper.CreateBasicEmbed("", "", DiscordColor.Cyan, ctx.Member);
        }

        public override BaseHelpFormatter WithCommand(Command command)
        {
            Embed.WithTitle(command.QualifiedName);
            Embed.WithDescription(command.Description);

            StringBuilder sb = new StringBuilder();
            foreach (CommandOverload overload in command.Overloads.OrderByDescending(x => x.Priority))
            {
                sb.Append("`").Append(SetPrefix(command.QualifiedName));

                foreach (CommandArgument arg in overload.Arguments)
                {
                    sb.Append(arg.IsOptional || arg.IsCatchAll ? " [" : " <");
                    sb.Append(arg.Name);
                    sb.Append(arg.IsCatchAll ? "..." : "");
                    sb.Append(arg.IsOptional || arg.IsCatchAll ? "]" : ">");
                }

                sb.Append("`\n");

                foreach (CommandArgument arg in overload.Arguments)
                {
                    sb.Append("`").Append(arg.Name).Append("`");
                    if (!string.IsNullOrEmpty(arg.Description))
                        sb.Append(": ").Append(arg.Description);
                    sb.AppendLine();
                }

                sb.AppendLine();
            }

            Embed.AddField("Arguments", sb.ToString(), true);
            Embed.AddField("Aliases", string.Join(" ", command.Aliases.Select(x => Formatter.InlineCode(SetPrefix(x)))), true);

            return this;
        }

        public override BaseHelpFormatter WithSubcommands(IEnumerable<Command> subcommands)
        {
            foreach (Command command in subcommands)
                if (!string.IsNullOrEmpty(command.Description))
                    Embed.AddField(Formatter.InlineCode(SetPrefix(command.Name)), command.Description, true);

            return this;
        }

        public override CommandHelpMessage Build()
        {
            return new CommandHelpMessage(embed: Embed.Build());
        }

        private string SetPrefix(string str) =>
            Prefix + str;
    }
}
