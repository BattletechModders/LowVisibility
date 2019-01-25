using BattleTech;
using BattleTech.UI;
using Harmony;
using LowVisibility.Helper;
using System.Collections.Generic;
using static LowVisibility.Helper.ActorHelper;

namespace LowVisibility.Patch {

    [HarmonyPatch(typeof(AbstractActor), "OnActivationBegin")]
    public static class AbstractActor_OnActivationBegin {        

        public static void Prefix(AbstractActor __instance, int stackItemID) {
            if (stackItemID == -1 || __instance == null || __instance.HasBegunActivation ) {
                // For some bloody reason DoneWithActor() invokes OnActivationBegin, EVEN THOUGH IT DOES NOTHING. GAH!
                return;
            }

            ECMHelper.UpdateECMState(__instance);
            VisibilityHelper.UpdateDetectionForAllActors(__instance.Combat);
            VisibilityHelper.UpdateVisibilityForAllTeams(__instance.Combat);

            LowVisibility.Logger.LogIfTrace($"=== AbstractActor:OnActivationBegin:pre - processing {CombatantHelper.Label(__instance)}");
            bool isPlayer = __instance.team == __instance.Combat.LocalPlayerTeam;
            if (isPlayer) {
                State.LastPlayerActivatedActorGUID = __instance.GUID; 
            }
        }
    }
    
   [HarmonyPatch(typeof(CombatSelectionHandler), "TrySelectActor")]
    public static class CombatSelectionHandler_TrySelectActor {
        public static void Postfix(CombatSelectionHandler __instance, bool __result, AbstractActor actor, bool manualSelection) {
            LowVisibility.Logger.LogIfDebug($"=== CombatSelectionHandler:TrySelectActor:post - entered for {CombatantHelper.Label(actor)}.");
            if (__instance != null && actor != null && __result == true) {
                VisibilityHelper.UpdateDetectionForAllActors(actor.Combat);
                VisibilityHelper.UpdateVisibilityForAllTeams(actor.Combat);

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

    [HarmonyPatch(typeof(AbstractActor), "UpdateLOSPositions")]
    public static class AbstractActor_UpdateLOSPositions {
        public static void Prefix(AbstractActor __instance) {
            // Check for teamID; if it's not present, unit hasn't spawned yet. Defer to UnitSpawnPointGameLogic::SpawnUnit for these updates
            if (State.TurnDirectorStarted && __instance.TeamId != null) {
                LowVisibility.Logger.LogIfDebug($"AbstractActor_UpdateLOSPositions:pre - entered for {CombatantHelper.Label(__instance)}.");

                // Why am I doing this here, isntead of OnMovePhaseComplete? Is it to help the AI, which needs this frequently evaluated for it's routines?
                //ECMHelper.UpdateECMState(__instance);
                //VisibilityHelper.UpdateDetectionForAllActors(__instance.Combat);
                //VisibilityHelper.UpdateVisibilityForAllTeams(__instance.Combat);
            }
        }

    }

    // Update the visibility checks
    [HarmonyPatch(typeof(Mech), "OnMovePhaseComplete")]
    public static class Mech_OnMovePhaseComplete {
        public static void Postfix(Mech __instance) {
            LowVisibility.Logger.LogIfDebug($"=== Mech:OnMovePhaseComplete:post - entered for {CombatantHelper.Label(__instance)}.");

            bool isPlayer = __instance.team == __instance.Combat.LocalPlayerTeam;
            if (isPlayer && State.ECMJamming(__instance) != 0) {
                // Send a floatie indicating the jamming
                MessageCenter mc = __instance.Combat.MessageCenter;
                mc.PublishMessage(new FloatieMessage(__instance.GUID, __instance.GUID, "JAMMED BY ECM", FloatieMessage.MessageNature.Debuff));
            }
        }

    }
}
