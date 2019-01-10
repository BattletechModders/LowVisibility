using BattleTech;
using BattleTech.UI;
using Harmony;
using LowVisibility.Helper;
using System.Linq;
using UnityEngine;
using static LowVisibility.Helper.ActorHelper;

namespace LowVisibility.Patch {

    [HarmonyPatch(typeof(AbstractActor), "OnActivationBegin")]
    public static class AbstractActor_OnActivationBegin {

        public static void CheckForJamming(AbstractActor source) {

            int sourceJammingStrength = 0;
            ActorEWConfig sourceEWConfig = State.GetOrCreateActorEWConfig(source);
            foreach (AbstractActor enemyActor in source.Combat.GetAllEnemiesOf(source)) {

                float actorsDistance = Vector3.Distance(source.CurrentPosition, enemyActor.CurrentPosition);
                ActorEWConfig enemyEWConfig = State.GetOrCreateActorEWConfig(enemyActor);
                LowVisibility.Logger.LogIfDebug($"Found enemy actor:{enemyActor.DisplayName}_{enemyActor.GetPilot().Name}.");
                //LowVisibility.Logger.LogIfDebug($"  - enemyEWConfig:{enemyEWConfig.ToString()}");
                //LowVisibility.Logger.LogIfDebug($"  - sourceEWConfig:{sourceEWConfig.ToString()}");
                if (sourceEWConfig.probeTier < enemyEWConfig.ecmTier) {
                    LowVisibility.Logger.Log($"Target:{enemyActor.DisplayName}_{enemyActor.GetPilot().Name} has ECM tier{enemyEWConfig.ecmTier} vs. source Probe tier:{sourceEWConfig.probeTier}");                    
                    if (actorsDistance > enemyEWConfig.ecmRange) {
                        LowVisibility.Logger.Log($"Actors are {actorsDistance}m apart, outside of ECM bubble range of:{enemyEWConfig.ecmRange}");
                    } else {
                        LowVisibility.Logger.Log($"Source:{source.DisplayName}_{source.GetPilot().Name} is within stronger ECM bubble of enemy:{enemyActor.DisplayName}_{enemyActor.GetPilot().Name}");
                        if (enemyEWConfig.ecmModifier > sourceJammingStrength) { sourceJammingStrength = enemyEWConfig.ecmModifier; }
                    }
                }

                // If the source has ECM, jam the target
                if (sourceEWConfig.ecmTier > -1 && enemyEWConfig.probeTier < sourceEWConfig.ecmTier) {
                    if (actorsDistance > sourceEWConfig.ecmRange) {
                        LowVisibility.Logger.Log($"Actors are {actorsDistance}m apart, outside of ECM bubble range of:{sourceEWConfig.ecmRange}");
                    } else {
                        LowVisibility.Logger.Log($"Enemy:{enemyActor.DisplayName}_{enemyActor.GetPilot().Name} is within ECM bubble of source actor:{source.DisplayName}_{source.GetPilot().Name} .");
                        State.JamActor(enemyActor, sourceEWConfig.ecmModifier);
                    }
                }

            }
            if (sourceJammingStrength > 0) { State.JamActor(source, sourceJammingStrength); } 
            else { State.UnjamActor(source); }

        }

        public static void Prefix(AbstractActor __instance, int stackItemID) {
            if (stackItemID == -1) {
                // For some bloody reason DoneWithActor() invokes OnActivationBegin, EVEN THOUGH IT DOES NOTHING. GAH!
                return;
            }

            LowVisibility.Logger.LogIfDebug($"=== AbstractActor:OnActivationBegin:pre - handling {ActorLabel(__instance)}.");
            bool isPlayer = __instance.team == __instance.Combat.LocalPlayerTeam;
            if (!isPlayer) {
                // Players are selected through the TrySelectActor patch below.

                // Make Round Check first
                RoundDetectRange detectRange = MakeSensorRangeCheck(__instance, false);
                LowVisibility.Logger.LogIfDebug($"  Actor:{ActorLabel(__instance)} has detectRange:{detectRange} at start of activation");
                State.RoundDetectResults[__instance.GUID] = detectRange;

                // Check for jamming next
                AbstractActor_OnActivationBegin.CheckForJamming(__instance);

                // Then update detection 
                //State.UpdateActorDetection(__instance);
                AbstractActor randomPlayerActor = __instance.Combat.AllActors
                    .Where(aa => aa.TeamId == __instance.Combat.LocalPlayerTeamGuid)
                    .First();
                State.UpdateDetectionForAllActors(__instance.Combat, randomPlayerActor);
            }
        }
    }

    [HarmonyPatch(typeof(CombatSelectionHandler), "TrySelectActor")]
    public static class CombatSelectionHandler_TrySelectActor {
        public static void Postfix(CombatSelectionHandler __instance, bool __result, AbstractActor actor, bool manualSelection) {
            LowVisibility.Logger.LogIfDebug($"=== CombatSelectionHandler:TrySelectActor:post - entered for {ActorLabel(actor)}.");
            if (__instance != null && actor != null && __result == true) {
                // Make Round Check first
                RoundDetectRange detectRange = MakeSensorRangeCheck(actor, true);
                LowVisibility.Logger.LogIfDebug($"  Actor:{ActorLabel(actor)} has detectRange:{detectRange} at start of activation");
                State.RoundDetectResults[actor.GUID] = detectRange;

                // Check for jamming next
                AbstractActor_OnActivationBegin.CheckForJamming(actor);

                // Then update detection 
                //State.UpdateActorDetection(actor);
                State.UpdateDetectionForAllActors(actor.Combat, actor);

                bool isPlayer = actor.team == actor.Combat.LocalPlayerTeam;
                if (isPlayer) {
                    State.LastPlayerActivatedActorGUID = actor.GUID;
                }
            }
        }
    }

    // Update the visibility checks
    [HarmonyPatch(typeof(Mech), "OnMovePhaseComplete")]
    public static class Mech_OnMovePhaseComplete {
        public static void Postfix(Mech __instance) {
            LowVisibility.Logger.LogIfDebug($"=== Mech:OnMovePhaseComplete:post - entered for {ActorLabel(__instance)}.");

            AbstractActor_OnActivationBegin.CheckForJamming(__instance);            
            State.UpdateDetectionForAllActors(__instance.Combat, __instance);
        }
    }

    // Update the visibility checks
    [HarmonyPatch(typeof(Vehicle), "OnMovePhaseComplete")]
    public static class Vehicle_OnMovePhaseComplete {
        public static void Postfix(Vehicle __instance) {
            LowVisibility.Logger.LogIfDebug($"=== Vehicle:OnMovePhaseComplete:post - entered for {ActorLabel(__instance)}.");

            AbstractActor_OnActivationBegin.CheckForJamming(__instance);
            State.UpdateDetectionForAllActors(__instance.Combat, __instance);
        }
    }

}
