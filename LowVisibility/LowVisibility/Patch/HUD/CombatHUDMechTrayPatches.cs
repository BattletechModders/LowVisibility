using BattleTech;
using BattleTech.UI;
using Harmony;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LowVisibility.Patch.HUD
{

    [HarmonyPatch(typeof(CombatHUDMechTray), "refreshMechInfo")]
    static class CombatHUDMechTrayPatches
    {
        public static void Postfix(CombatHUDMechTray __instance)
        {
            if (__instance == null || __instance.DisplayedActor == null || __instance.DisplayedActor.Description == null) 
            {
                return; 
            }

            string fullName = __instance.DisplayedActor.Description.UIName;
            if (!string.IsNullOrEmpty(fullName))
            {
                Mod.Log.Debug($"RefreshMechInfo - Setting CombatHUDMechTray name to {fullName}");
                __instance.MechNameText.SetText(fullName, Array.Empty<object>());
            }
        }
    }
}
