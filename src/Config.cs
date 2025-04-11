using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Entities;
using CounterStrikeSharp.API.Modules.Extensions;
using System.Text.Json.Serialization;

namespace QuakeSounds
{
    public class PluginConfig : BasePluginConfig
    {
        // disabled
        [JsonPropertyName("enabled")] public bool Enabled { get; set; } = true;
        // debug prints
        [JsonPropertyName("debug")] public bool Debug { get; set; } = false;
        // enable center message
        [JsonPropertyName("enable_center_message")] public bool CenterMessage { get; set; } = true;
        // center message typ (default, alert or html)
        [JsonPropertyName("center_message_type")] public string CenterMessageType { get; set; } = "default";
        // enable chat message
        [JsonPropertyName("enable_chat_message")] public bool ChatMessage { get; set; } = true;
        // sounds dict (language, string to match, sound path)
        [JsonPropertyName("sounds")] public Dictionary<int, Dictionary<string, string>> Sounds { get; set; } = [];
        // muted players
        [JsonPropertyName("muted")] public List<string> Muted { get; set; } = [];
        // player languages
        [JsonPropertyName("player_languages")] public Dictionary<string, string> PlayerLanguages { get; set; } = [];
    }

    public partial class QuakeSounds : BasePlugin, IPluginConfig<PluginConfig>
    {
        public required PluginConfig Config { get; set; }

        public void OnConfigParsed(PluginConfig config)
        {
            Config = config;
            // sort Config.Sounds and sub dictionaries by key
            Config.Sounds = Config.Sounds
                .OrderBy(x => x.Key)
                .ToDictionary(x => x.Key, x => x.Value
                    .OrderBy(y => y.Key)
                    .ToDictionary(y => y.Key, y => y.Value));
            // update config and write new values from plugin to config file if changed after update
            Config.Update();
            Console.WriteLine(Localizer["core.config"]);
        }

        private bool ToggleMute(CCSPlayerController player)
        {
            if (Config.Muted.Contains(player.NetworkIDString))
            {
                Config.Muted.Remove(player.NetworkIDString);
                Config.Update();
                player.PrintToChat(Localizer["sounds.unmuted"]);
                return false;
            }
            else
            {
                Config.Muted.Add(player.NetworkIDString);
                Config.Update();
                player.PrintToChat(Localizer["sounds.muted"]);
                return true;
            }
        }

        private void LoadPlayerLanguage(string steamID)
        {
            if (!Config.PlayerLanguages.ContainsKey(steamID)
                || Config.PlayerLanguages[steamID] == "") return;
            playerLanguageManager.SetLanguage(
                new SteamID(steamID),
                new System.Globalization.CultureInfo(Config.PlayerLanguages[steamID]));
        }

        private void SavePlayerLanguage(string steamID, string language)
        {
            if (language == null
                || language == "") return;
            // set language for player
            if (!Config.PlayerLanguages.ContainsKey(steamID))
                Config.PlayerLanguages.Add(steamID, language);
            else
                Config.PlayerLanguages[steamID] = language;
            Config.Update();
            playerLanguageManager.SetLanguage(new SteamID(steamID), new System.Globalization.CultureInfo(language));
            DebugPrint($"Saved language for player {steamID} to {language}.");
        }
    }
}
