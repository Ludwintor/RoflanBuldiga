using Newtonsoft.Json;
using DSharpPlus.Entities;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading;
using System.IO;
using DSharpPlus.Lavalink;
using System.Linq;
using DiscordBot.Helpers;
using System.Threading.Tasks;
using DiscordBot.Services;
using DSharpPlus.Lavalink.EventArgs;

namespace DiscordBot.Music
{
    public class GuildMusicData
    {
        private const string SAVE_FOLDER = "MusicData";

        public string GuildId => Guild.Id.ToString();
        public TimeSpan CurrentPosition => NowPlaying == null ? TimeSpan.Zero : Player.CurrentState.PlaybackPosition;
        public DiscordChannel Channel => Player?.Channel;
        public DiscordChannel CommandChannel { get; set; }
        public IReadOnlyList<MusicData> Queue { get; }
        public RepeatMode RepeatMode { get; private set; } = RepeatMode.None;
        public EqualizerMode EqualizerMode { get; private set; } = EqualizerMode.Default;
        public int Volume { get; private set; } = 100;
        public bool IsShuffled { get; private set; } = false;
        public bool IsPlaying { get; private set; } = false;
        public bool IsBassBoosted { get; private set; } = false;
        public MusicData NowPlaying { get; private set; }

        private DiscordGuild Guild { get; }
        private List<MusicData> QueueInternal { get; }
        private Random RNG { get; }
        private LavalinkService Lavalink { get; }
        private LavalinkGuildConnection Player { get; set; }
        private bool IsConnected => Player != null && Player.IsConnected;

        public GuildMusicData(DiscordGuild guild, Random random, LavalinkService lavalink)
        {
            Guild = guild;
            QueueInternal = new List<MusicData>();
            RNG = random;
            Lavalink = lavalink;

            Queue = new ReadOnlyCollection<MusicData>(QueueInternal);
        }

        public void Save()
        {
            string json = JsonConvert.SerializeObject(new GuildMusicSerializable(this), Formatting.Indented);
            string path = Directory.GetCurrentDirectory(); // TODO: Hide saving in a file under some service
            path = Path.Combine(path, SAVE_FOLDER);
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);

            path = Path.Combine(path, $"{GuildId}.json");
            File.WriteAllText(path, json);
        }

        public void Load()
        {
            string path = Directory.GetCurrentDirectory();
            path = Path.Combine(path, SAVE_FOLDER, $"{GuildId}.json");
            if (!File.Exists(path))
                return;

            string json = File.ReadAllText(path);
            GuildMusicSerializable loadedData = JsonConvert.DeserializeObject<GuildMusicSerializable>(json);
            RepeatMode = loadedData.RepeatMode;
            EqualizerMode = loadedData.EqualizerMode;
            Volume = loadedData.Volume;
            IsShuffled = loadedData.IsShuffled;
        }

        public async Task PlayAsync()
        {
            if (!IsConnected || NowPlaying != null)
                return;

            await PlayNextAsync();
        }

        public async Task ResumeAsync()
        {
            if (!IsConnected)
                return;

            IsPlaying = true;
            await Player.ResumeAsync();
        }

        public async Task PauseAsync()
        {
            if (!IsConnected)
                return;

            IsPlaying = false;
            await Player.PauseAsync();
        }

        public async Task StopAsync()
        {
            if (!IsConnected)
                return;

            IsPlaying = false;
            await Player.StopAsync();
        }

        public async Task RestartAsync()
        {
            if (!IsConnected || NowPlaying == null)
                return;

            lock (QueueInternal)
            {
                QueueInternal.Insert(0, NowPlaying);
            }

            await Player.StopAsync();
        }

        public async Task CreatePlayerAsync(DiscordChannel channel)
        {
            if (IsConnected)
                return;

            Player = await Lavalink.LavalinkNode.ConnectAsync(channel);
            if (Volume != 100)
                await Player.SetVolumeAsync(Volume);
            if (EqualizerMode != EqualizerMode.Default)
                await Player.AdjustEqualizerAsync(EqualizerPresets.FromMode(EqualizerMode));

            Player.PlaybackFinished += Player_PlaybackFinished;
        }

        public async Task DestroyPlayerAsync()
        {
            if (Player == null)
                return;

            if (Player.IsConnected)
                await Player.DisconnectAsync();

            Player.PlaybackFinished -= Player_PlaybackFinished;
            Player = null;
            Save();
        }

        public async Task SeekAsync(TimeSpan target, bool relative)
        {
            if (!IsConnected)
                return;

            if (!relative)
                await Player.SeekAsync(target);
            else
                await Player.SeekAsync(CurrentPosition + target);
        }

        public void SetRepeatMode(RepeatMode mode)
        {
            RepeatMode previousMode = RepeatMode;
            RepeatMode = mode;
            if (NowPlaying?.Track != null)
            {
                if (mode == RepeatMode.Single && previousMode != mode)
                {
                    lock (QueueInternal)
                    {
                        QueueInternal.Insert(0, NowPlaying);
                    }
                }
                else if (mode != RepeatMode.Single && previousMode == RepeatMode.Single)
                {
                    lock (QueueInternal)
                    {
                        QueueInternal.RemoveAt(0);
                    }
                }
            }
        }

        public void Enqueue(MusicData music)
        {
            lock (QueueInternal)
            {
                if (RepeatMode == RepeatMode.All && QueueInternal.Count == 1)
                {
                    QueueInternal.Insert(0, music);
                }
                else if (!IsShuffled || !QueueInternal.Any())
                {
                    QueueInternal.Add(music);
                }
                else
                {
                    int index = RNG.Next(QueueInternal.Count);
                    QueueInternal.Insert(index, music);
                }
            }
        }

        public MusicData Dequeue()
        {
            lock (QueueInternal)
            {
                if (!QueueInternal.Any())
                    return null;

                MusicData music;
                switch (RepeatMode)
                {
                    case RepeatMode.None:
                        music = QueueInternal[0];
                        QueueInternal.RemoveAt(0);

                        return music;
                    case RepeatMode.All:
                        music = QueueInternal[0];
                        QueueInternal.RemoveAt(0);
                        QueueInternal.Add(music);

                        return music;
                    case RepeatMode.Single:
                        return QueueInternal[0];
                }

                return null;
            }
        }

        public MusicData Remove(int index)
        {
            lock (QueueInternal)
            {
                if (index < 0 || index >= QueueInternal.Count)
                    return null;

                MusicData data = QueueInternal[index];
                QueueInternal.RemoveAt(index);

                return data;
            }
        }

        public int EmptyQueue()
        {
            lock (QueueInternal)
            {
                int count = QueueInternal.Count;
                QueueInternal.Clear();

                return count;
            }
        }

        public void Shuffle()
        {
            if (IsShuffled)
                return;

            IsShuffled = true;
            lock (QueueInternal)
            {
                QueueInternal.Shuffle(RNG);
            }
        }

        public void StopShuffle()
        {
            IsShuffled = false;
        }

        public async Task SetEqualizerAsync(EqualizerMode mode)
        {
            LavalinkBandAdjustment[] bands = EqualizerPresets.FromMode(mode);

            if (Player != null)
                await Player.AdjustEqualizerAsync(bands);

            EqualizerMode = mode;
        }

        private async Task PlayNextAsync()
        {
            MusicData music = Dequeue();
            if (music == null)
            {
                NowPlaying = null;
                return;
            }

            NowPlaying = music;
            IsPlaying = true;
            await Player.PlayAsync(music.Track);
        }

        private async Task Player_PlaybackFinished(LavalinkGuildConnection sender, TrackFinishEventArgs e)
        {
            await Task.Delay(500);
            IsPlaying = false;
            await PlayNextAsync();

            Save();
        }
    }
}
