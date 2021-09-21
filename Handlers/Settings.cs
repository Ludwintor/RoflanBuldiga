using DiscordBot.Helpers;
using DiscordBot.Localization;
using System;
using System.Collections.Generic;
using System.Text;

namespace DiscordBot.Handlers
{
    public class Settings
    {
        private static Dictionary<ulong, Settings> settings = new Dictionary<ulong, Settings>();
        private static LocalizationStrings localization = new LocalizationStrings();

        public static Settings Get(ulong guildId)
        {
            if (!settings.ContainsKey(guildId))
                settings.Add(guildId, new Settings());

            return settings[guildId];
        }

        private Language currentLanguage;

        public void SelectLanguage(Language language)
        {
            currentLanguage = language;
        }

        public string GetLocalString(string id) => localization.GetString(id, currentLanguage);
        public string GetLocalStringFormat(string id, params object[] args) => string.Format(GetLocalString(id), args);

    }
}
