using DiscordBot.Helpers;
using DiscordBot.Music;
using DiscordBot.Services;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.Enums;
using DSharpPlus.Interactivity.Extensions;
using DSharpPlus.Lavalink;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiscordBot.Commands
{
    [ModuleLifespan(ModuleLifespan.Transient)]
    public class MusicModule : BaseCommandModule
    {
        private const string LIVE_STATUS = ":red_circle:LIVE";

        private static readonly string[] IGNORE_CHANNEL_CHECK = { "join" };
        private static readonly PaginationEmojis PAGINATION_EMOJIS;
        private static readonly DiscordEmoji[] NUMBERS;
        private MusicService Music { get; }

        private GuildMusicData GuildMusic { get; set; }

        public MusicModule(MusicService music)
        {
            Music = music;
        }

        static MusicModule()
        {
            PAGINATION_EMOJIS = new PaginationEmojis
            {
                SkipLeft = null,
                SkipRight = null,
                Left = DiscordEmoji.FromUnicode("◀"),
                Stop = DiscordEmoji.FromUnicode("⏹"),
                Right = DiscordEmoji.FromUnicode("▶"),
            };

            NUMBERS = new DiscordEmoji[10];
            for (int i = 0; i < 9; i++)
            {
                NUMBERS[i] = DiscordEmoji.FromUnicode($"{i+1}\u20e3");
            }
            NUMBERS[9] = DiscordEmoji.FromUnicode("\uD83D\uDD1F");
        }

        public override async Task BeforeExecutionAsync(CommandContext ctx)
        {
            DiscordChannel memberChannel = ctx.Member.VoiceState?.Channel;

            if (memberChannel == null)
            {
                await ctx.RespondAsync("You need to be in a voice channel.");
                throw new CommandCancelledException();
            }

            DiscordChannel botChannel = ctx.Guild.CurrentMember.VoiceState?.Channel;
            if (botChannel != null && memberChannel != botChannel && !IgnoreCheck(ctx.Command))
            {
                await ctx.RespondAsync("You need to be in the same voice channel");
                throw new CommandCancelledException();
            }

            GuildMusic = Music.GetOrCreate(ctx.Guild);
            GuildMusic.CommandChannel = ctx.Channel;

            await base.BeforeExecutionAsync(ctx);
        }

        [Command("join")]
        [Aliases("j")]
        [Description("Joins current channel")]
        public async Task JoinAsync(CommandContext ctx)
        {
            await GuildMusic.CreatePlayerAsync(ctx.Member.VoiceState.Channel);
            await ctx.RespondAsync("Joined channel!");
        }

        [Command("leave")]
        [Aliases("l")]
        [Description("Leaves current channel")]
        public async Task LeaveAsync(CommandContext ctx)
        {
            await GuildMusic.DestroyPlayerAsync();
            await ctx.RespondAsync("Left channel!");
        }

        [Command("play")]
        [Aliases("p")]
        [Description("Plays or enqueues track from search term or URL")]
        [Priority(0)]
        public async Task PlayAsync(CommandContext ctx, [RemainingText, Description("Search term")] string search)
        {
            LavalinkLoadResult loadResult = await Music.GetTracksAsync(search);
            if (loadResult.LoadResultType == LavalinkLoadResultType.LoadFailed || 
                loadResult.LoadResultType == LavalinkLoadResultType.NoMatches)
            {
                await ctx.RespondAsync($"No matches found for {search}");
                return;
            }

            LavalinkTrack track = loadResult.Tracks.First();
            IReadOnlyList<MusicData> queue = GuildMusic.Queue;

            TimeSpan estimatedTime = TimeSpan.Zero;
            bool isLive = false;
            if ((GuildMusic.NowPlaying != null && GuildMusic.NowPlaying.Track.IsStream) ||
                queue.Any(x => x.Track.IsStream))
            {
                isLive = true;
            }

            if (!isLive)
            {
                estimatedTime = (GuildMusic.NowPlaying?.Track.Length - GuildMusic.CurrentPosition) ?? TimeSpan.Zero;
                foreach (MusicData music in queue)
                    estimatedTime += music.Track.Length;
            }

            GuildMusic.Enqueue(new MusicData(track, ctx.Member));

            if (GuildMusic.NowPlaying == null)
            {
                await GuildMusic.CreatePlayerAsync(ctx.Member.VoiceState.Channel);
                await GuildMusic.PlayAsync();

                string title = Formatter.MaskedUrl(track.Title, track.Uri);
                DiscordEmbedBuilder builder = EmbedHelper.CreateBasicEmbed("Playing NOW", title, DiscordColor.Gold, ctx.Member);
                builder.AddField("Author", Formatter.InlineCode(track.Author), true);
                builder.AddField("Duration", track.IsStream ? LIVE_STATUS : Formatter.InlineCode(track.Length.ToReadable()), true);

                StringBuilder sb = new StringBuilder();
                sb.AppendLine(GuildMusic.RepeatMode != RepeatMode.None ? "Repeat mode is " + Formatter.InlineCode(GuildMusic.RepeatMode.ToString()) : "");
                sb.AppendLine(GuildMusic.EqualizerMode != EqualizerMode.Default ? "Equalizer preset is " + Formatter.InlineCode(GuildMusic.EqualizerMode.ToString()) : "");
                string note = sb.ToString().Trim();
                if (!string.IsNullOrEmpty(note))
                    builder.AddField(":warning:Player Modified:warning:", note);
                builder.WithThumbnail(Music.GetThumbnail(track.Uri));

                await ctx.RespondAsync(builder.Build());
            }
            else
            {
                string title = Formatter.MaskedUrl(track.Title, track.Uri);
                DiscordEmbedBuilder builder = EmbedHelper.CreateBasicEmbed("Track Added", title, DiscordColor.SpringGreen, ctx.Member);
                builder.AddField("Author", Formatter.InlineCode(track.Author), true);
                builder.AddField("Duration", track.IsStream ? LIVE_STATUS : Formatter.InlineCode(track.Length.ToReadable()), true);
                builder.AddField("Estimated time until playing", isLive ? LIVE_STATUS : Formatter.InlineCode(estimatedTime.ToReadable()), true);
                builder.AddField("Position in queue", Formatter.InlineCode(queue.Count.ToString()), false);
                builder.WithThumbnail(Music.GetThumbnail(track.Uri));

                await ctx.RespondAsync(builder.Build());
            }
        }

        [Command("play")]
        [Priority(1)]
        public async Task PlayAsync(CommandContext ctx, [Description("URL to play")] Uri url)
        {
            LavalinkLoadResult loadResult = await Music.GetTracksAsync(url);
            if (loadResult.LoadResultType == LavalinkLoadResultType.LoadFailed ||
                loadResult.LoadResultType == LavalinkLoadResultType.NoMatches)
            {
                await ctx.RespondAsync($"No matches found for {Formatter.InlineCode(url.AbsoluteUri)}");
                return;
            }

            LavalinkTrack track = loadResult.Tracks.First();
            IReadOnlyCollection<MusicData> queue = GuildMusic.Queue;

            TimeSpan estimatedTime = TimeSpan.Zero;
            bool isLive = false;
            if ((GuildMusic.NowPlaying != null && GuildMusic.NowPlaying.Track.IsStream) ||
                queue.Any(x => x.Track.IsStream))
            {
                isLive = true;
            }

            if (!isLive)
            {
                estimatedTime = (GuildMusic.NowPlaying?.Track.Length - GuildMusic.CurrentPosition) ?? TimeSpan.Zero;
                foreach (MusicData music in queue)
                    estimatedTime += music.Track.Length;
            }

            GuildMusic.Enqueue(new MusicData(track, ctx.Member));

            if (GuildMusic.NowPlaying == null)
            {
                await GuildMusic.CreatePlayerAsync(ctx.Member.VoiceState.Channel);
                await GuildMusic.PlayAsync();

                string title = Formatter.MaskedUrl(track.Title, track.Uri);
                DiscordEmbedBuilder builder = EmbedHelper.CreateBasicEmbed("Playing NOW", title, DiscordColor.Gold, ctx.Member);
                builder.AddField("Author", Formatter.InlineCode(track.Author), true);
                builder.AddField("Duration", track.IsStream ? LIVE_STATUS : Formatter.InlineCode(track.Length.ToReadable()), true);

                StringBuilder sb = new StringBuilder();
                sb.AppendLine(GuildMusic.RepeatMode != RepeatMode.None ? "Repeat mode is " + Formatter.InlineCode(GuildMusic.RepeatMode.ToString()) : "");
                sb.AppendLine(GuildMusic.EqualizerMode != EqualizerMode.Default ? "Equalizer preset is " + Formatter.InlineCode(GuildMusic.EqualizerMode.ToString()) : "");
                string note = sb.ToString().Trim();
                if (!string.IsNullOrEmpty(note))
                    builder.AddField(":warning:Player Modified:warning:", note);
                builder.WithThumbnail(Music.GetThumbnail(track.Uri));

                await ctx.RespondAsync(builder.Build());
            }
            else
            {
                string title = Formatter.MaskedUrl(track.Title, track.Uri);
                DiscordEmbedBuilder builder = EmbedHelper.CreateBasicEmbed("Track Added", title, DiscordColor.SpringGreen, ctx.Member);
                builder.AddField("Author", Formatter.InlineCode(track.Author), true);
                builder.AddField("Duration", track.IsStream ? LIVE_STATUS : Formatter.InlineCode(track.Length.ToReadable()), true);
                builder.AddField("Estimated time until playing", isLive ? LIVE_STATUS : Formatter.InlineCode(estimatedTime.ToReadable()), true);
                builder.AddField("Position in queue", Formatter.InlineCode(queue.Count.ToString()), false);
                builder.WithThumbnail(Music.GetThumbnail(track.Uri));

                await ctx.RespondAsync(builder.Build());
            }
        }

        [Command("queue")]
        [Aliases("q")]
        [Description("Displays queue information")]
        public async Task QueueAsync(CommandContext ctx)
        {
            if (GuildMusic.NowPlaying == null)
            {
                DiscordEmbed embed = EmbedHelper.CreateBasicEmbed("Queue", "Nothing is playing right now", DiscordColor.Red, ctx.Member).Build();

                await ctx.RespondAsync(embed);
                return;
            }

            MusicData current = GuildMusic.NowPlaying;
            if (GuildMusic.RepeatMode == RepeatMode.Single)
            {
                string description = $"Queue repeats {Formatter.MaskedUrl(current.Track.Title, current.Track.Uri)} requested by {Formatter.InlineCode(current.RequestedBy.DisplayName)}";
                DiscordEmbed embed = EmbedHelper.CreateBasicEmbed("Queue", description, DiscordColor.CornflowerBlue, ctx.Member).Build();

                await ctx.RespondAsync(embed);
                return;
            }

            string playingDescription = $"Now playing {Formatter.MaskedUrl(current.Track.Title, current.Track.Uri)} requested by {Formatter.InlineCode(current.RequestedBy.DisplayName)}";
            IReadOnlyList<MusicData> queue = GuildMusic.Queue;
            int pageCount = queue.Count / 10 + (queue.Count % 10 != 0 ? 1 : 0);
            if (pageCount == 1)
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendLine(playingDescription);
                sb.AppendLine();
                for (int i = 0; i < queue.Count; i++)
                {
                    MusicData music = queue[i];
                    sb.Append(NUMBERS[i]).Append(" ").Append(Formatter.MaskedUrl(music.Track.Title, music.Track.Uri));
                    sb.Append(" requested by `").Append(music.RequestedBy.DisplayName).AppendLine("`");
                }

                DiscordEmbed embed = EmbedHelper.CreateBasicEmbed("Queue", sb.ToString(), DiscordColor.CornflowerBlue, ctx.Member).Build();

                await ctx.RespondAsync(embed);
            }
            else if (pageCount > 1)
            {
                List<Page> pages = new List<Page>(pageCount);
                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < pageCount; i++)
                {
                    sb.Clear();
                    sb.AppendLine(playingDescription);
                    sb.AppendLine();
                    int indexLimit = i == queue.Count / 10 ? queue.Count % 10 : 10;
                    for (int j = 0; j < indexLimit; j++)
                    {
                        MusicData music = queue[j + 10 * i];
                        sb.Append(NUMBERS[j]).Append(" ").Append(Formatter.MaskedUrl(music.Track.Title, music.Track.Uri));
                        sb.Append(" requested by `").Append(music.RequestedBy.DisplayName).AppendLine("`");
                    }

                    sb.AppendLine().Append("Page `").Append(i + 1).Append("/").Append(pageCount).Append("`");

                    DiscordEmbedBuilder embed = EmbedHelper.CreateBasicEmbed("Queue", sb.ToString(), DiscordColor.CornflowerBlue, ctx.Member);

                    pages.Add(new Page(embed: embed));
                }

                await ctx.Channel.SendPaginatedMessageAsync(ctx.User, pages, PAGINATION_EMOJIS, PaginationBehaviour.Ignore, PaginationDeletion.KeepEmojis, TimeSpan.FromMinutes(2f));
            }
            else
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendLine(playingDescription);
                sb.AppendLine();
                sb.Append("Queue is empty");

                await ctx.RespondAsync(sb.ToString());
            }
        }

        [Command("stop")]
        [Aliases("s")]
        [Description("Stops playback completely and empties queue")]
        public async Task StopAsync(CommandContext ctx)
        {
            int count = GuildMusic.EmptyQueue();
            await GuildMusic.StopAsync();
            await GuildMusic.DestroyPlayerAsync();

            string noun = count == 1 ? "track" : "tracks";
            DiscordEmbed embed = EmbedHelper.CreateBasicEmbed("Player Status", $"Removed {count} {noun} from queue", DiscordColor.Red, ctx.Member).Build();

            await ctx.RespondAsync(embed);
        }

        [Command("pause")]
        [Aliases("ps")]
        [Description("Pauses playback")]
        public async Task PauseAsync(CommandContext ctx)
        {
            await GuildMusic.PauseAsync();

            DiscordEmbed embed = EmbedHelper.CreateBasicEmbed("Player Status", "Playback paused!", DiscordColor.Orange, ctx.Member).Build();
            await ctx.RespondAsync(embed);
        }

        [Command("resume")]
        [Aliases("rs")]
        [Description("Resumes playback")]
        public async Task ResumeAsync(CommandContext ctx)
        {
            await GuildMusic.ResumeAsync();

            DiscordEmbed embed = EmbedHelper.CreateBasicEmbed("Player Status", "Playback resumed!", DiscordColor.SapGreen, ctx.Member).Build();
            await ctx.RespondAsync(embed);
        }

        [Command("seek")]
        [Description("Seeks to specified track time")]
        public async Task SeekAsync(CommandContext ctx, [RemainingText, Description("Which time to seek")] TimeSpan position)
        {
            await GuildMusic.SeekAsync(position, false);
        }

        [Command("forward")]
        [Aliases("fw")]
        [Description("Forwards track on specified time")]
        public async Task ForwardAsync(CommandContext ctx, [RemainingText, Description("How much to forward")] TimeSpan position)
        {
            await GuildMusic.SeekAsync(position, true);
        }

        [Command("rewind")]
        [Aliases("rw")]
        [Description("Rewinds track on specified time")]
        public async Task RewindAsync(CommandContext ctx, [RemainingText, Description("How much to rewind")] TimeSpan position)
        {
            await GuildMusic.SeekAsync(-position, true);
        }

        [Command("skip")]
        [Aliases("fs", "next", "n")]
        [Description("Skips current track")]
        public async Task SkipAsync(CommandContext ctx)
        {
            MusicData music = GuildMusic.NowPlaying;
            await GuildMusic.StopAsync();

            string description = $"Skipped {Formatter.Bold(music.Track.Title)} by {Formatter.Bold(music.Track.Author)}";
            DiscordEmbed embed = EmbedHelper.CreateBasicEmbed("Track skip", description, DiscordColor.Aquamarine, ctx.Member).Build();
            await ctx.RespondAsync(embed);
        }

        [Command("repeat")]
        [Aliases("loop")]
        [Description("Changes repeat mode. `off`, `all` or `one`")]
        public async Task RepeatAsync(CommandContext ctx, [Description("Repeat mode")] RepeatMode? mode = null)
        {
            RepeatMode newMode = mode ?? GuildMusic.RepeatMode.Next();

            GuildMusic.SetRepeatMode(newMode);

            string description = $"Repeat mode set to {Formatter.Bold(newMode.ToString())}";
            DiscordEmbed embed = EmbedHelper.CreateBasicEmbed("Player Status", description, DiscordColor.Purple, ctx.Member).Build();
            await ctx.RespondAsync(embed);
        }

        [Command("remove")]
        [Aliases("rm")]
        [Description("Removes track from queue")]
        public async Task RemoveAsync(CommandContext ctx, [Description("Track position in queue")] int index)
        {
            MusicData music = GuildMusic.Remove(index - 1);
            if (music == null)
            {
                await MessageHelper.TimedResponseAsync(ctx, "There's no track in the given position", 8, true);
                return;
            }

            string description = $"Removed {Formatter.Bold(music.Track.Title)} by {Formatter.Bold(music.Track.Author)} from the queue";
            DiscordEmbed embed = EmbedHelper.CreateBasicEmbed("Track removed", description, DiscordColor.Red, ctx.Member).Build();
            await ctx.RespondAsync(embed);
        }

        [Command("nowplaying")]
        [Aliases("np")]
        [Description("Shows currently played track")]
        public async Task NowPlayingAsync(CommandContext ctx)
        {
            MusicData music = GuildMusic.NowPlaying;
            if (music == null)
            {
                DiscordEmbed embed = EmbedHelper.CreateBasicEmbed("Now Playing", "Nothing is playing right now", DiscordColor.Red, ctx.Member).Build();

                await ctx.RespondAsync(embed);
            }
            else
            {
                TimeSpan currentPosition = GuildMusic.CurrentPosition;
                TimeSpan length = music.Track.Length;

                StringBuilder progress = new StringBuilder();
                int current = (int)(currentPosition.TotalSeconds / length.TotalSeconds * 42);
                for (int i = 0; i < 42; i++)
                {
                    if (i == current)
                        progress.Append(DiscordEmoji.FromName(ctx.Client, ":radio_button:"));
                    else
                        progress.Append("\u25AC");
                }

                string title = Formatter.MaskedUrl(music.Track.Title, music.Track.Uri);
                DiscordEmbedBuilder builder = EmbedHelper.CreateBasicEmbed("Now Playing", title, DiscordColor.CornflowerBlue, ctx.Member);
                builder.AddField("Author", Formatter.InlineCode(music.Track.Author), true);
                builder.AddField("Requested by", Formatter.InlineCode(music.RequestedBy.DisplayName), true);
                builder.AddField("Duration", Formatter.InlineCode($"{currentPosition.ToReadable()} / {length.ToReadable()}"), true);
                builder.AddField("\u200B", Formatter.InlineCode(progress.ToString()), false);

                await ctx.RespondAsync(builder.Build());
            }
        }

        [Command("equalizer")]
        [Aliases("eq")]
        [Description("Changes equalizer preset. `off` or `bassboosted`")]
        public async Task AdjustEqualizerAsync(CommandContext ctx, [Description("Equalizer Preset")] EqualizerMode mode)
        {
            await GuildMusic.SetEqualizerAsync(mode);

            string description = $"Equalizer preset set to {Formatter.Bold(mode.ToString())}";
            DiscordEmbed embed = EmbedHelper.CreateBasicEmbed("Player Status", description, DiscordColor.Purple, ctx.Member).Build();
            await ctx.RespondAsync(embed);
        }

        private bool IgnoreCheck(Command command) =>
            IGNORE_CHANNEL_CHECK.Contains(command.Name);
    }
}
