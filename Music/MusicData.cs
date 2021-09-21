using DSharpPlus.Entities;
using DSharpPlus.Lavalink;
using System;
using System.Collections.Generic;
using System.Text;

namespace DiscordBot.Music
{
    public class MusicData
    {
        public LavalinkTrack Track { get; }
        public DiscordMember RequestedBy { get; }

        public MusicData(LavalinkTrack track, DiscordMember member)
        {
            Track = track;
            RequestedBy = member;
        }
    }
}
