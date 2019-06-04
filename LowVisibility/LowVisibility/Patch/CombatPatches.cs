using BattleTech;
using BattleTech.UI;
using Harmony;
using LowVisibility.Helper;
using System.Collections.Generic;
using us.frostraptor.modUtils;

namespace LowVisibility.Patch {

   [HarmonyPatch(typeof(CombatSelectionHandler), "TrySelectActor")]
    public static class CombatSelectionHandler_TrySelectActor {
        public static void Postfix(CombatSelectionHandler __instance, bool __result, AbstractActor actor, bool manualSelection) {
            Mod.Log.Debug($"=== CombatSelectionHandler:TrySelectActor:post - entered for {CombatantUtils.Label(actor)}.");
            if (__instance != null && actor != null && __result == true && actor.IsAvailableThisPhase) {
                ECMHelper.UpdateECMState(actor);
                if (actor.team == actor.Combat.LocalPlayerTeam) {
                    State.LastPlayerActor = actor.GUID;
                }

                // Do this to force a refresh during a combat save
                if (TurnDirector_OnEncounterBegin.IsFromSave) {
                    DEBUG_ToggleForcedVisibility(false, actor.Combat);
                    TurnDirector_OnEncounterBegin.IsFromSave = false;
                }                                
            }
        }

        public static void DEBUG_ToggleForcedVisibility(bool forceVisible, CombatGameState Combat) {
            List<ICombatant> list = Combat.AllActors.ConvertAll<ICombatant>((AbstractActor x) => x);
            for (int i = 0; i < list.Count; i++) {
                PilotableActorRepresentation pilotableActorRepresentation = list[i].GameRep as PilotableActorRepresentation;
                if (pilotableActorRepresentation != null) {
                    if (forceVisible) {
                        pilotableActorRepresentation.SetForcedPlayerVisibilityLevel(VisibilityLevel.LOSFull, true);
                    } else {
                        pilotableActorRepresentation.ClearForcedPlayerVisibilityLevel(list);
                    }
                }
            } 
        }
    }

    // Update the visibility checks
    [HarmonyPatch(typeof(Mech), "OnMovePhaseComplete")]
    public static class Mech_OnMovePhaseComplete {
        public static void Postfix(Mech __instance) {
            Mod.Log.Debug($"=== Mech:OnMovePhaseComplete:post - entered for {CombatantUtils.Label(__instance)}.");

            bool isPlayer = __instance.team == __instance.Combat.LocalPlayerTeam;
            if (isPlayer && State.ECMJamming(__instance) != 0) {
                // Send a floatie indicating the jamming
                MessageCenter mc = __instance.Combat.MessageCenter;
                mc.PublishMessage(new FloatieMessage(__instance.GUID, __instance.GUID, "JAMMED BY ECM", FloatieMessage.MessageNature.Debuff));
            }
        }

    }
}
