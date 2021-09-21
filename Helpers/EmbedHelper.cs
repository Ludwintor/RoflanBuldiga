using DSharpPlus.Entities;

namespace DiscordBot.Helpers
{
    public static class EmbedHelper
    {
        public static DiscordEmbedBuilder CreateBasicEmbed(string title, string description = "", DiscordColor? color = null, DiscordMember author = null)
        {
            DiscordEmbedBuilder builder = new DiscordEmbedBuilder();
            builder.WithTitle(title);
            builder.WithDescription(description);
            builder.WithColor(color ?? DiscordColor.White);
            if (author != null)
                builder.WithAuthor(author.DisplayName, null, author.AvatarUrl);

            return builder;
        }
    }
}
