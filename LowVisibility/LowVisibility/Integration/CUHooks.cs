using BattleTech.UI;

namespace LowVisibility.Integration
{
    public class CUHooks
    {
        public static void ToggleTargetingComputerArmorDisplay(CombatHUDTargetingComputer __instance, bool show = true)
        {
            CustomUnits.LowVisibilityAPIHelper.SetArmorDisplayActive(__instance, show);
        }

    }
}
