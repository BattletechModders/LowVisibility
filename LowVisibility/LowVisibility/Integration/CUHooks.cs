using BattleTech.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LowVisibility.Integration
{
    public class CUHooks
    {
        public static void ToggleTargetingComputerArmorDisplay(CombatHUDTargetingComputer __instance, bool show=true)
        {
            CustomUnits.LowVisibilityAPIHelper.SetArmorDisplayActive(__instance, show);
        }

    }
}
