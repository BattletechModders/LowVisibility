using BattleTech.UI;
using IRBTModUtils;
using IRBTModUtils.Extension;
using LowVisibility.Helper;
using LowVisibility.Integration;
using LowVisibility.Object;
using LowVisibility.Patch.HUD;
using UnityEngine;
using us.frostraptor.modUtils;

namespace LowVisibility.Patch
{
    [HarmonyPatch(typeof(MoveStatusPreview), "DisplayPreviewStatus")]
    static class MoveStatusPreview_DisplayPreviewStatus
    {
        static void Postfix(MoveStatusPreview __instance, AbstractActor actor, Vector3 worldPos, MoveType moveType)
        {
            if (__instance == null || actor == null) return;

            // If the player, update some UI elements
            if (actor.team.IsLocalPlayer)
            {
                // If a combatant is selected, update the CAC side panel
                CombatHUDTargetingComputer chudTargetingComp = SharedState.CombatHUD?.TargetingComputer;
                if (chudTargetingComp != null && chudTargetingComp.ActivelyShownCombatant != null)
                {
                    ICombatant target = chudTargetingComp.ActivelyShownCombatant;
                    float range = Vector3.Distance(worldPos, target.CurrentPosition);
                    bool hasVisualScan = VisualLockHelper.CanSpotTarget(actor, worldPos,
                        target, target.CurrentPosition, target.CurrentRotation, target.Combat.LOS);
                    SensorScanType scanType = SensorLockHelper.CalculateSharedLock(target, actor);
                    Mod.Log.Debug?.Write($"CHTC:RAI ~~~ LastActivated:{CombatantUtils.Label(ModState.LastPlayerActorActivated)} vs. enemy:{CombatantUtils.Label(target)} " +
                        $"at range: {range} has scanType:{scanType} visualScan:{hasVisualScan}");

                    // Build the CAC side-panel
                    CACSidePanelHooks.SetCHUDInfoSidePanelInfo(ModState.LastPlayerActorActivated, target, range, hasVisualScan, scanType);
                }

                // Refresh the floating icons after the player is done moving
                foreach (ICombatant combatant in SharedState.Combat.AllActors)
                {
                    Mod.UILog.Debug?.Write($"Updating numFlagHex for actor: {combatant.DistinctId()}");
                    if (actor.VisibilityToTargetUnit(combatant) > VisibilityLevel.None)
                    {
                        CombatHUDNumFlagHex combatHUDNumFlagHex = SharedState.CombatHUD?.InWorldMgr?.GetNumFlagForCombatant(combatant);
                        CombatHUDMarkDisplay combatHUDMarkDisplay = combatHUDNumFlagHex != null ? combatHUDNumFlagHex?.ActorInfo?.MarkDisplay : null;
                        if (combatHUDMarkDisplay != null)
                        {
                            Mod.UILog.Debug?.Write($"  Refreshing numFlagHex");
                            CombatHUDMarkDisplay_RefreshInfo.RefreshMarkDisplay(combatHUDMarkDisplay, worldPos);
                        }
                    }
                }

            }

        }
    }
}
