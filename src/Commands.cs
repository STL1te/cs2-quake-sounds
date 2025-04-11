using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Extensions;
using CounterStrikeSharp.API.Modules.Menu;

namespace QuakeSounds
{
    public partial class QuakeSounds
    {
        [ConsoleCommand("qs", "QuakeSounds")]
        [CommandHelper(whoCanExecute: CommandUsage.CLIENT_ONLY, minArgs: 0, usage: "")]
        public void CommandSounds(CCSPlayerController player, CommandInfo command)
        {
            // close any active menu
            MenuManager.CloseActiveMenu(player);
            // create menu to choose sound
            var menu = new ChatMenu(Localizer["menu.title"]);
            // check if player is muted
            if (Config.PlayersMuted.Contains(player.NetworkIDString))
            {
                menu.AddMenuOption(Localizer["menu.unmute"], (_, _) => ToggleMute(player));
            }
            else
            {
                // add option to mute
                menu.AddMenuOption(Localizer["menu.mute"], (_, _) => ToggleMute(player));
            }
            // open menu
            MenuManager.OpenChatMenu(player, menu);
        }

        [ConsoleCommand("QuakeSounds", "QuakeSounds admin commands")]
        [CommandHelper(whoCanExecute: CommandUsage.SERVER_ONLY, minArgs: 1, usage: "<command>")]
        public void CommandQuakeSounds(CCSPlayerController player, CommandInfo command)
        {
            string subCommand = command.GetArg(1);
            switch (subCommand.ToLower())
            {
                case "reload":
                    Config.Reload();
                    command.ReplyToCommand(Localizer["admin.reload"]);
                    break;
                case "disable":
                    Config.Enabled = false;
                    Config.Update();
                    command.ReplyToCommand(Localizer["admin.disable"]);
                    break;
                case "enable":
                    Config.Enabled = true;
                    Config.Update();
                    command.ReplyToCommand(Localizer["admin.enable"]);
                    break;
                default:
                    command.ReplyToCommand(Localizer["admin.unknown_command"].Value
                        .Replace("{command}", subCommand));
                    break;
            }
        }
    }
}
