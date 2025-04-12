using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;

namespace QuakeSounds
{
    public partial class QuakeSounds
    {
        private void DebugPrint(string message)
        {
            if (Config.Debug)
            {
                Console.WriteLine(Localizer["core.debugprint"].Value.Replace("{message}", message));
            }
        }

        private static object? GetGameRule(string rule)
        {
            var ents = Utilities.FindAllEntitiesByDesignerName<CCSGameRulesProxy>("cs_gamerules");
            foreach (var ent in ents)
            {
                var gameRules = ent.GameRules;
                if (gameRules == null) continue;

                var property = gameRules.GetType().GetProperty(rule);
                if (property != null && property.CanRead)
                {
                    return property.GetValue(gameRules);
                }
            }
            return null;
        }
    }
}