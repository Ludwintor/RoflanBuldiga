using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace DiscordBot
{
    public struct Config
    {
        public const ulong BOT_OWNER_ID = 265703434475274240;

        [JsonProperty("token")]
        public string Token { get; private set; }
        [JsonProperty("prefixes")]
        public string[] CommandPrefixes { get; private set; }
    }
}
