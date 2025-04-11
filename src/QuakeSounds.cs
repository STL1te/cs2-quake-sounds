using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Translations;
using CounterStrikeSharp.API.Modules.Entities;
using CounterStrikeSharp.API.Modules.Events;
using CounterStrikeSharp.API.Modules.Utils;
using System.Globalization;

namespace QuakeSounds
{
    public partial class QuakeSounds : BasePlugin
    {
        public override string ModuleName => "CS2 QuakeSounds";
        public override string ModuleAuthor => "Kalle <kalle@kandru.de>";

        private readonly PlayerLanguageManager playerLanguageManager = new();
        private Dictionary<CCSPlayerController, int> _playerKillsInRound = [];

        public override void Load(bool hotReload)
        {
            RegisterEventHandler<EventRoundStart>(OnRoundStart);
            RegisterEventHandler<EventPlayerDeath>(OnPlayerDeath);
            RegisterEventHandler<EventPlayerConnectFull>(OnPlayerConnect);
            RegisterEventHandler<EventPlayerChat>(OnPlayerChatCommand);
            if (hotReload)
                foreach (CCSPlayerController entry in Utilities.GetPlayers().Where(
                    p => p.IsValid
                        && !p.IsBot))
                    LoadPlayerLanguage(entry.NetworkIDString);
        }

        public override void Unload(bool hotReload)
        {
            DeregisterEventHandler<EventRoundStart>(OnRoundStart);
            DeregisterEventHandler<EventPlayerDeath>(OnPlayerDeath);
            DeregisterEventHandler<EventPlayerConnectFull>(OnPlayerConnect);
            DeregisterEventHandler<EventPlayerChat>(OnPlayerChatCommand);
        }

        private HookResult OnRoundStart(EventRoundStart @event, GameEventInfo info)
        {
            _playerKillsInRound.Clear();
            return HookResult.Continue;
        }

        private HookResult OnPlayerDeath(EventPlayerDeath @event, GameEventInfo info)
        {
            CCSPlayerController? attacker = @event.Attacker;
            CCSPlayerController? victim = @event.Userid;
            // check attacker
            if (attacker != null
                && attacker.IsValid
                && attacker != victim)
            {
                if (_playerKillsInRound.ContainsKey(attacker))
                {
                    _playerKillsInRound[attacker]++;
                }
                else
                {
                    _playerKillsInRound.Add(attacker, 1);
                }
                // play sound if we found the amount of kills in Config.Sounds
                if (Config.Sounds.ContainsKey(_playerKillsInRound[attacker])
                    && Config.Sounds[_playerKillsInRound[attacker]].ContainsKey("_sound"))
                {
                    PlaySound(attacker, Config.Sounds[_playerKillsInRound[attacker]]["_sound"]);
                    PrintMessage(attacker, Config.Sounds[_playerKillsInRound[attacker]]);
                }
            }
            // check victim
            if (victim != null
                && victim.IsValid)
            {
                _playerKillsInRound.Remove(victim);
            }
            return HookResult.Continue;
        }

        private HookResult OnPlayerConnect(EventPlayerConnectFull @event, GameEventInfo info)
        {
            CCSPlayerController? player = @event.Userid;
            if (player == null
                || !player.IsValid
                || player.IsBot) return HookResult.Continue;
            LoadPlayerLanguage(player.NetworkIDString);
            return HookResult.Continue;
        }

        private HookResult OnPlayerChatCommand(EventPlayerChat @event, GameEventInfo info)
        {
            CCSPlayerController? player = Utilities.GetPlayerFromUserid(@event.Userid);
            if (player == null
                || !player.IsValid
                || player.IsBot) return HookResult.Continue;
            if (@event.Text.StartsWith("!lang", StringComparison.OrdinalIgnoreCase))
            {
                // get language from command instead of player because it defaults to english all the time Oo
                string? language = @event.Text.Split(' ').Skip(1).FirstOrDefault()?.Trim();
                if (language == null
                    || !CultureInfo.GetCultures(CultureTypes.AllCultures).Any(c => c.Name.Equals(language, StringComparison.OrdinalIgnoreCase)))
                {
                    return HookResult.Continue;
                }
                // set language for player
                SavePlayerLanguage(player.NetworkIDString, language);
                return HookResult.Continue;
            }
            // redraw GUI
            return HookResult.Continue;
        }

        private void PlaySound(CCSPlayerController player, string sound)
        {
            DebugPrint($"Playing quake sound {sound} for player {player.PlayerName}.");
            // prepare recipient filter (to avoid playing sounds for muted players)
            RecipientFilter filter = [];
            foreach (var entry in Utilities.GetPlayers().Where(
                p => p.IsValid
                    && !p.IsBot
                    && !Config.Muted.Contains(p.NetworkIDString)).ToList())
                filter.Add(entry);
            // get world entity
            CWorld? worldEnt = Utilities.FindAllEntitiesByDesignerName<CWorld>("worldent").FirstOrDefault();
            if (worldEnt == null
                || !worldEnt.IsValid)
            {
                DebugPrint("Could not find world entity.");
                return;
            }
            DebugPrint("Playing quake sound on world entity.");
            // play sound
            worldEnt.EmitSound(sound, filter);
        }

        private void PrintMessage(CCSPlayerController player, Dictionary<string, string> sound)
        {
            foreach (CCSPlayerController entry in Utilities.GetPlayers().Where(
                p => p.IsValid
                    && !p.IsBot
                    && !Config.Muted.Contains(p.NetworkIDString)).ToList())
            {
                string? message = sound.TryGetValue(playerLanguageManager.GetLanguage(new SteamID(entry.NetworkIDString)).TwoLetterISOLanguageName, out var playerMessage)
                    ? playerMessage
                    : (sound.TryGetValue(CoreConfig.ServerLanguage, out var serverMessage)
                        ? serverMessage
                        : sound.Last().Value);
                if (message != null)
                {
                    // use players language for printing messages
                    using (new WithTemporaryCulture(new CultureInfo(playerLanguageManager.GetLanguage(new SteamID(entry.NetworkIDString)).TwoLetterISOLanguageName)))
                    {
                        // check for center messatges
                        if (Config.CenterMessage && Config.CenterMessageType.Equals("default", StringComparison.CurrentCultureIgnoreCase))
                        {
                            if (entry == player)
                                entry.PrintToCenter(Localizer["center.msg.player"].Value
                                    .Replace("{message}", message));
                            else
                                entry.PrintToCenter(Localizer["center.msg.other"].Value
                                    .Replace("{player}", player.PlayerName)
                                    .Replace("{message}", message));
                        }
                        else if (Config.CenterMessage && Config.CenterMessageType.Equals("alert", StringComparison.CurrentCultureIgnoreCase))
                        {
                            if (entry == player)
                                entry.PrintToCenterAlert(Localizer["center.msg.player"].Value
                                    .Replace("{message}", message));
                            else
                                entry.PrintToCenterAlert(Localizer["center.msg.other"].Value
                                    .Replace("{player}", player.PlayerName)
                                    .Replace("{message}", message));
                        }
                        else if (Config.CenterMessage && Config.CenterMessageType.Equals("html", StringComparison.CurrentCultureIgnoreCase))
                        {
                            if (entry == player)
                                entry.PrintToCenterHtml(Localizer["center.msg.player"].Value
                                    .Replace("{message}", message));
                            else
                                entry.PrintToCenterHtml(Localizer["center.msg.other"].Value
                                    .Replace("{player}", player.PlayerName)
                                    .Replace("{message}", message));
                        }
                        // check for chat message
                        if (Config.ChatMessage)
                        {
                            if (entry == player)
                                entry.PrintToChat(Localizer["chat.msg.player"].Value
                                    .Replace("{message}", message));
                            else
                                entry.PrintToChat(Localizer["chat.msg.other"].Value
                                    .Replace("{player}", player.PlayerName)
                                    .Replace("{message}", message));
                        }
                    }
                }
            }
        }
    }
}