using Newtonsoft.Json;

namespace DiscordBot.Music
{
    public struct GuildMusicSerializable
    {
        [JsonProperty("repeatMode")]
        public RepeatMode RepeatMode { get; private set; }
        [JsonProperty("equalizerMode")]
        public EqualizerMode EqualizerMode { get; private set; }
        [JsonProperty("volume")]
        public int Volume { get; private set; }
        [JsonProperty("isShuffled")]
        public bool IsShuffled { get; private set; }

        public GuildMusicSerializable(GuildMusicData data)
        {
            RepeatMode = data.RepeatMode;
            EqualizerMode = data.EqualizerMode;
            Volume = data.Volume;
            IsShuffled = data.IsShuffled;

        }
    }
}
