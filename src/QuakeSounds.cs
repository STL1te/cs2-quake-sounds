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
            RegisterListener<Listeners.OnMapStart>(OnMapStart);
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
            RemoveListener<Listeners.OnMapStart>(OnMapStart);
        }

        private HookResult OnRoundStart(EventRoundStart @event, GameEventInfo info)
        {
            if (Config.ResetKillsOnRoundStart) _playerKillsInRound.Clear();
            return HookResult.Continue;
        }

        private HookResult OnPlayerDeath(EventPlayerDeath @event, GameEventInfo info)
        {
            // check for warmup period
            if (!Config.EnabledDuringWarmup && (bool)GetGameRule("WarmupPeriod")!)
            {
                DebugPrint("Ignoring during warmup.");
                return HookResult.Continue;
            }
            // check for world damage
            if (@event.Weapon.ToLower() == "world" && Config.IgnoreWorldDamage)
            {
                DebugPrint("Ignoring world damage.");
                return HookResult.Continue;
            }
            // get attacker and victim
            CCSPlayerController? attacker = @event.Attacker;
            CCSPlayerController? victim = @event.Userid;
            DebugPrint($"Player {attacker?.PlayerName} killed by {victim?.PlayerName} with weapon {@event.Weapon}.");
            // get weapon key
            string weaponKey = @event.Weapon.StartsWith("weapon_", StringComparison.OrdinalIgnoreCase)
                    ? @event.Weapon.ToLower()
                    : "weapon_" + @event.Weapon.ToLower();
            // check if attacker exists and is valid
            if (attacker != null
                && attacker.IsValid
                && (!attacker.IsBot || !Config.IgnoreBots))
            {
                // check for self kill or team kill and whether to count them
                if (attacker != victim || Config.CountSelfKills
                    || (victim != null && victim.IsValid && (attacker.Team != victim.Team || Config.CountTeamKills)))
                {
                    if (_playerKillsInRound.ContainsKey(attacker))
                    {
                        _playerKillsInRound[attacker]++;
                    }
                    else
                    {
                        _playerKillsInRound.Add(attacker, 1);
                    }
                    DebugPrint($"Player {attacker.PlayerName} has {_playerKillsInRound[attacker]} kills.");
                }
                // play sound if we found the amount of kills in Config.Sounds
                if (_playerKillsInRound.ContainsKey(attacker)
                    && Config.Sounds.ContainsKey(_playerKillsInRound[attacker].ToString())
                    && Config.Sounds[_playerKillsInRound[attacker].ToString()].TryGetValue("_sound", out string? sound))
                {
                    RecipientFilter filter = PrepareFilter(
                        attacker,
                        victim,
                        Config.Sounds[_playerKillsInRound[attacker].ToString()].TryGetValue("_filter", out string? soundFilter) ? soundFilter : null
                    );
                    PlaySound(attacker, sound, filter: filter);
                    PrintMessage(attacker, Config.Sounds[_playerKillsInRound[attacker].ToString()], filter);
                }
                // check for self kill
                else if (Config.Sounds.TryGetValue("selfkill", out Dictionary<string, string>? selfkillSound)
                    && attacker == victim
                    && selfkillSound.TryGetValue("_sound", out string? selfkillSoundName))
                {
                    RecipientFilter filter = PrepareFilter(
                        attacker,
                        victim,
                        selfkillSound.TryGetValue("_filter", out string? soundFilter) ? soundFilter : null
                    );
                    PlaySound(attacker, selfkillSoundName, "world", filter);
                    PrintMessage(attacker, selfkillSound, filter);
                }
                // check for team kill
                else if (Config.Sounds.TryGetValue("teamkill", out Dictionary<string, string>? teamkillSound)
                    && victim != null && victim.IsValid && attacker.Team == victim.Team
                    && teamkillSound.TryGetValue("_sound", out string? teamkillSoundName))
                {
                    RecipientFilter filter = PrepareFilter(
                        attacker,
                        victim,
                        teamkillSound.TryGetValue("_filter", out string? soundFilter) ? soundFilter : null
                    );
                    PlaySound(attacker, teamkillSoundName, filter: filter);
                    PrintMessage(attacker, teamkillSound, filter);
                }
                // check for first blood
                else if (Config.Sounds.TryGetValue("firstblood", out Dictionary<string, string>? firstbloodSound)
                    && _playerKillsInRound.ContainsKey(attacker)
                    && _playerKillsInRound.Count == 1 && _playerKillsInRound[attacker] == 1
                    && firstbloodSound.TryGetValue("_sound", out string? firstbloodSoundName))
                {
                    RecipientFilter filter = PrepareFilter(
                        attacker,
                        victim,
                        firstbloodSound.TryGetValue("_filter", out string? soundFilter) ? soundFilter : null
                    );
                    PlaySound(attacker, firstbloodSoundName, filter: filter);
                    PrintMessage(attacker, firstbloodSound, filter);
                }
                // check for knife kill
                else if (Config.Sounds.TryGetValue("knifekill", out Dictionary<string, string>? knifekillSound)
                    && @event.Weapon.Contains("knife", StringComparison.OrdinalIgnoreCase)
                    && knifekillSound.TryGetValue("_sound", out string? knifekillSoundName))
                {
                    RecipientFilter filter = PrepareFilter(
                        attacker,
                        victim,
                        knifekillSound.TryGetValue("_filter", out string? soundFilter) ? soundFilter : null
                    );
                    PlaySound(attacker, knifekillSoundName, filter: filter);
                    PrintMessage(attacker, knifekillSound, filter);
                }
                // check for headshot kill
                else if (Config.Sounds.TryGetValue("headshot", out Dictionary<string, string>? headshotSound)
                    && @event.Headshot
                    && headshotSound.TryGetValue("_sound", out string? headshotSoundName))
                {
                    RecipientFilter filter = PrepareFilter(
                        attacker,
                        victim,
                        headshotSound.TryGetValue("_filter", out string? soundFilter) ? soundFilter : null
                    );
                    PlaySound(attacker, headshotSoundName, filter: filter);
                    PrintMessage(attacker, headshotSound, filter);
                }
                // check for specific weapon sound (e.g. grenades and stuff)
                else if (Config.Sounds.TryGetValue(weaponKey, out Dictionary<string, string>? weaponSound)
                    && weaponSound.TryGetValue("_sound", out string? weaponSoundName))
                {
                    RecipientFilter filter = PrepareFilter(
                        attacker,
                        victim,
                        weaponSound.TryGetValue("_filter", out string? soundFilter) ? soundFilter : null
                    );
                    PlaySound(attacker, weaponSoundName, filter: filter);
                    PrintMessage(attacker, weaponSound, filter);
                }
            }
            // check victim
            if (victim != null
                && victim.IsValid
                && Config.ResetKillsOnDeath)
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

        private void OnMapStart(string mapName)
        {
            // reset player kills
            _playerKillsInRound.Clear();
        }

        private RecipientFilter PrepareFilter(CCSPlayerController? attacker, CCSPlayerController? victim, string? _soundFilter = null)
        {
            // prepare filter for sound emission
            RecipientFilter filter = [];
            // check which soundfilter to use
            string soundFilter = _soundFilter ?? Config.FilterSounds;
            foreach (var entry in Utilities.GetPlayers().Where(
                p => p.IsValid
                // ignore bots
                && (!p.IsBot || !Config.IgnoreBots)
                // ignore muted players
                && !Config.PlayersMuted.Contains(p.NetworkIDString)
                // check for sound filter
                && (soundFilter.Equals("all", StringComparison.OrdinalIgnoreCase)
                    || (soundFilter.Equals("attacker_team", StringComparison.OrdinalIgnoreCase) && p.Team == attacker?.Team)
                    || (soundFilter.Equals("victim_team", StringComparison.OrdinalIgnoreCase) && p.Team == victim?.Team)
                    || (soundFilter.Equals("involved", StringComparison.OrdinalIgnoreCase) && (p == attacker || p == victim))
                    || (soundFilter.Equals("attacker", StringComparison.OrdinalIgnoreCase) && p == attacker)
                    || (soundFilter.Equals("victim", StringComparison.OrdinalIgnoreCase) && p != attacker && p != victim)
                    || (soundFilter.Equals("spectator", StringComparison.OrdinalIgnoreCase) && p.Team == CsTeam.Spectator)))
                .ToList())
            {
                filter.Add(entry);
            }
            DebugPrint($"Prepared filter ({_soundFilter ?? "all"}): {string.Join(", ", filter.Select(p => p.PlayerName))}");
            return filter;
        }

        private void PlaySound(CCSPlayerController player, string sound, string? playOn = null, RecipientFilter? filter = null)
        {
            DebugPrint($"Playing quake sound {sound} for player {player.PlayerName}.");
            // check if sound string is a sound path or another type of string
            if (sound.StartsWith("sounds/"))
            {
                DebugPrint("Playing quake sound via client command for all listening players.");
                if (filter == null)
                {
                    // play sound for all players
                    foreach (var entry in Utilities.GetPlayers().Where(
                    p => p.IsValid
                        && !p.IsBot
                        && !Config.PlayersMuted.Contains(p.NetworkIDString)).ToList())
                    {
                        entry.ExecuteClientCommand($"play {sound}");
                    }
                }
                else
                {
                    // play sound for filtered players
                    foreach (var entry in filter)
                    {
                        entry.ExecuteClientCommand($"play {sound}");
                    }
                }
            }
            // check if sound should be played on the player
            else if (Config.PlayOn.Equals("player", StringComparison.CurrentCultureIgnoreCase) && playOn == null
                    || playOn != null && playOn == "player")
            {
                DebugPrint("Playing quake sound on player.");
                player.EmitSound(sound, filter);
            }
            // check if sound should be played on the world entity
            else if (Config.PlayOn.Equals("world", StringComparison.CurrentCultureIgnoreCase) && playOn == null
                    || playOn != null && playOn == "world")
            {
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
        }

        private void PrintMessage(CCSPlayerController player, Dictionary<string, string> sound, RecipientFilter? filter = null)
        {
            // declare local variable to print message correctly depending on filter
            void DoPrint(CCSPlayerController entry, CCSPlayerController player, Dictionary<string, string> sound)
            {
                if (string.IsNullOrEmpty(entry.NetworkIDString))
                    return;
                
                string? message = sound.TryGetValue(playerLanguageManager.GetLanguage(new SteamID(entry.NetworkIDString)).TwoLetterISOLanguageName, out var playerMessage)
                    ? playerMessage
                    : (sound.TryGetValue(CoreConfig.ServerLanguage, out var serverMessage)
                        ? serverMessage
                        : sound.First().Value);
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
            // print to all available players if no filter is set
            if (filter == null)
            {
                foreach (CCSPlayerController entry in Utilities.GetPlayers().Where(
                    p => p.IsValid
                    && !p.IsBot
                    && !Config.PlayersMuted.Contains(p.NetworkIDString))
                    .ToList())
                {
                    DoPrint(entry, player, sound);
                }
            }
            else
            {
                foreach (CCSPlayerController entry in filter.Where(
                    p => p.IsValid
                    && !p.IsBot)
                    .ToList())
                {
                    DoPrint(entry, player, sound);
                }
            }
        }
    }
}