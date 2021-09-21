using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace DiscordBot.Localization
{
    public class LocalizationStrings
    {
        private const string ERROR_ID = "Localization error: There's no localization with this id [{0}]. Please report to bot owner";
        private const string ERROR_LANG = "Localization error: There's no localization for this language {0} for this id {1}. Please report to bot owner";

        private Dictionary<string, Dictionary<Language, string>> localizations; // ID retrieves Dictionary that contains same strings on different languages

        public LocalizationStrings()
        {
            localizations = new Dictionary<string, Dictionary<Language, string>>();
            string path = Path.Combine(Directory.GetCurrentDirectory(), "localization.json");

            string json = File.ReadAllText(path);
            localizations = JsonConvert.DeserializeObject<Dictionary<string, Dictionary<Language, string>>>(json);
        }

        public string GetString(string id, Language language)
        {
            if (!localizations.ContainsKey(id))
                return string.Format(ERROR_ID, id);
            Dictionary<Language, string> strings = localizations[id];

            if (!strings.ContainsKey(language))
                return string.Format(ERROR_LANG, language, id);

            return strings[language];
        }
    }

    public enum Language
    {
        ENG,
        RUS
    }
}
