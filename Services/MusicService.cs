using DiscordBot.Music;
using DSharpPlus.Entities;
using DSharpPlus.Lavalink;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace DiscordBot.Services
{
    public class MusicService
    {
        private LavalinkService Lavalink { get; }
        private Random RNG { get; }
        private ConcurrentDictionary<ulong, GuildMusicData> MusicData { get; }

        public MusicService(LavalinkService lavalink, Random random)
        {
            Lavalink = lavalink;
            RNG = random;

            MusicData = new ConcurrentDictionary<ulong, GuildMusicData>();
        }

        public GuildMusicData GetOrCreate(DiscordGuild guild)
        {
            if (MusicData.TryGetValue(guild.Id, out GuildMusicData data))
                return data;

            data = MusicData.AddOrUpdate(guild.Id, new GuildMusicData(guild, RNG, Lavalink), (key, value) => value);
            data.Load();

            return data;
        }

        public Task<LavalinkLoadResult> GetTracksAsync(string search, LavalinkSearchType searchType = LavalinkSearchType.Youtube) => 
            Lavalink.LavalinkNode.Rest.GetTracksAsync(search, searchType);

        public Task<LavalinkLoadResult> GetTracksAsync(Uri url) =>
            Lavalink.LavalinkNode.Rest.GetTracksAsync(url);

        public Uri GetThumbnail(Uri youtube)
        {
            int start = youtube.AbsoluteUri.IndexOf("v=") + 2;
            int end = youtube.AbsoluteUri.IndexOf('&', start);
            string id;
            if (end != -1)
                id = youtube.AbsoluteUri.Substring(start, end - start);
            else
                id = youtube.AbsoluteUri.Substring(start);

            return new Uri($"https://i.ytimg.com/vi/{id}/hqdefault.jpg");
        }
    }
}
