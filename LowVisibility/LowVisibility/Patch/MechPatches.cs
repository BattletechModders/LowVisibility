using BattleTech.UI;
using IRBTModUtils;
using IRBTModUtils.Extension;
using LowVisibility.Object;
using System.Collections.Generic;
using UnityEngine;

namespace LowVisibility.Patch
{

    [HarmonyPatch(typeof(Mech), "OnPositionUpdate")]
    public static class Mech_OnPositionUpdate
    {
        // public override void OnPositionUpdate(Vector3 position, Quaternion heading, int stackItemUID, bool updateDesignMask, List<DesignMaskDef> remainingMasks, bool skipLogging = false)
        public static void Postfix(Mech __instance, Vector3 position, Quaternion heading, int stackItemUID, bool updateDesignMask, List<DesignMaskDef> remainingMasks, bool skipLogging)
        {
            if (stackItemUID == -1 || __instance == null)
            {
                // Nothing to do? Why were we even called?!?
                return;
            }

            Mod.Log.Info?.Write($"== Mech: {__instance.DistinctId()} updated to position: {__instance.CurrentPosition} rot: {__instance.CurrentRotation} from position: {__instance.PreviousPosition} rot: {__instance.PreviousRotation}");
            EWState actorState = new EWState(__instance);
            if (actorState.HasMimetic())
            {
                Mod.Log.Info?.Write($"  Mimetic pips updated to: {actorState.CurrentMimeticPips()}");
            }

            // If the player, update some UI elements
            if (__instance.team.IsLocalPlayer)
            {

                // Refresh the floating icons after the player is done moving
                foreach (ICombatant combatant in SharedState.Combat.AllActors)
                {
                    if (__instance.VisibilityToTargetUnit(combatant) > VisibilityLevel.None)
                    {
                        CombatHUDNumFlagHex combatHUDNumFlagHex = SharedState.CombatHUD?.InWorldMgr?.GetNumFlagForCombatant(combatant);
                        CombatHUDMarkDisplay combatHUDMarkDisplay = combatHUDNumFlagHex != null ? combatHUDNumFlagHex?.ActorInfo?.MarkDisplay : null;
                        if (combatHUDMarkDisplay != null) combatHUDMarkDisplay.RefreshInfo();
                    }
                }

            }
        }
    }

}

