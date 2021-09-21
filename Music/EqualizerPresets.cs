using DSharpPlus.Lavalink;
using System;
using System.Collections.Generic;
using System.Text;

namespace DiscordBot.Music
{
    public static class EqualizerPresets
    {
        public static LavalinkBandAdjustment[] Default { get; }
        public static LavalinkBandAdjustment[] BassBoosted { get; }
        public static LavalinkBandAdjustment[] Test { get; }


        static EqualizerPresets()
        {
            Default = new LavalinkBandAdjustment[15];
            for (int i = 0; i < Default.Length; i++)
                Default[i] = new LavalinkBandAdjustment(i, 0f);

            BassBoosted = new LavalinkBandAdjustment[9];
            BassBoosted[0] = new LavalinkBandAdjustment(2, 0.4f);
            BassBoosted[1] = new LavalinkBandAdjustment(4, 0.4f);
            BassBoosted[2] = new LavalinkBandAdjustment(5, -0.2f); 
            BassBoosted[3] = new LavalinkBandAdjustment(6, -0.15f);
            BassBoosted[4] = new LavalinkBandAdjustment(8, -0.1f);
            BassBoosted[5] = new LavalinkBandAdjustment(9, -0.05f);
            BassBoosted[6] = new LavalinkBandAdjustment(11, 0.15f);
            BassBoosted[7] = new LavalinkBandAdjustment(12, 0f);
            BassBoosted[8] = new LavalinkBandAdjustment(14, 0.12f);

            Test = new LavalinkBandAdjustment[4];
            Test[0] = new LavalinkBandAdjustment(0, 0f);
            Test[1] = new LavalinkBandAdjustment(1, 0f);
            Test[2] = new LavalinkBandAdjustment(2, 0f);
            Test[3] = new LavalinkBandAdjustment(3, 1f);
        }

        public static LavalinkBandAdjustment[] FromMode(EqualizerMode mode)
        {
            return mode switch
            {
                EqualizerMode.Default => Default,
                EqualizerMode.BassBoosted => BassBoosted,
                EqualizerMode.Test => Test,
                _ => null,
            };
        }
    }
}
