using BattleTech.UI;

namespace LowVisibility.Helper
{
    public static class CombatHUDHelper
    {
        public static void ForceNameRefresh(CombatGameState combat)
        {

            CombatHUD combatHUD = VisRangeIndicator.Instance.HUD;
            CombatHUDInWorldElementMgr inWorldMgr = combatHUD?.InWorldMgr;

            if (combat == null || combatHUD == null || inWorldMgr == null)
            {
                Mod.Log.Warn?.Write("CombatGameState, CombatHUD, or InWorldManager was null when a ForceNameRefresh call was made - skipping!");
                return;
            }

            // Force an update of visibility state to try to get the mech labels to update
            foreach (ICombatant combatant in combat.GetAllImporantCombatants())
            {
                CombatHUDNumFlagHex flagHex = inWorldMgr.GetNumFlagForCombatant(combatant);
                // Can be null in CWolf's blackout contracts. He intentionally disables the flagHex in those cases.
                if (flagHex != null)
                {
                    flagHex.ActorInfo.NameDisplay.RefreshInfo();
                }

            }
        }
    }
}
