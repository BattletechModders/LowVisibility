using BattleTech;
using Harmony;
using System;
using System.Reflection;
using UnityEngine;
using static LowVisibility.Helper.ActorHelper;

namespace LowVisibility.Patch {

    // Setup the actor and pilot states at the start of the encounter
    [HarmonyPatch(typeof(TurnDirector), "OnEncounterBegin")]
    public static class TurnDirector_OnEncounterBegin {

        public static void Postfix(TurnDirector __instance) {
            LowVisibility.Logger.LogIfDebug("=== TurnDirector:OnEncounterBegin:post - entered.");
        }
    }

    [HarmonyPatch()]
    public static class TurnDirector_BeginNewRound {

        // Private method can't be patched by annotations, so use MethodInfo
        public static MethodInfo TargetMethod() {
            return AccessTools.Method(typeof(TurnDirector), "BeginNewRound", new Type[] { typeof(int) });
        }

        public static void Prefix(TurnDirector __instance) {
            LowVisibility.Logger.LogIfDebug("=== TurnDirector:BeginNewRound:post - entered.");
            // Determine the sensor check result for each actor
            foreach (AbstractActor actor in __instance.Combat.AllActors) {
                RoundDetectRange detectRange = MakeSensorRangeCheck(actor);
                LowVisibility.Logger.LogIfDebug($"Actor:{actor.DisplayName}_{actor.GetPilot().Name} has detectRange:{detectRange} this round!");
                State.roundDetectResults[actor.GUID] = detectRange;
                
            }

            // Update the current vision for all allied and friendly units
            State.UpdateDetectionOnRoundBegin(__instance.Combat);
        }
    }

    [HarmonyPatch(typeof(AbstractActor), "OnActivationBegin")]
    public static class AbstractActor_OnActivationBegin {

        public static void CheckForJamming(AbstractActor source) {

            int sourceJammingStrength = 0;
            ActorEWConfig sourceEWConfig = State.GetOrCreateActorEWConfig(source);
            foreach (AbstractActor enemyActor in source.Combat.GetAllEnemiesOf(source)) {

                float actorsDistance = Vector3.Distance(source.CurrentPosition, enemyActor.CurrentPosition);
                ActorEWConfig enemyEWConfig = State.GetOrCreateActorEWConfig(enemyActor);
                LowVisibility.Logger.LogIfDebug($"Found enemy actor:{enemyActor.DisplayName}_{enemyActor.GetPilot().Name}.");
                LowVisibility.Logger.LogIfDebug($"  - enemyEWConfig:{enemyEWConfig.ToString()}");
                LowVisibility.Logger.LogIfDebug($"  - sourceEWConfig:{sourceEWConfig.ToString()}");
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

        public static void Prefix(AbstractActor __instance) {
            LowVisibility.Logger.LogIfDebug($"=== AbstractActor:OnActivationBegin:pre - handling {ActorLabel(__instance)}.");
            if (__instance != null) {
                CheckForJamming(__instance);
                State.UpdateActorDetection(__instance);

                bool isPlayer = __instance.team == __instance.Combat.LocalPlayerTeam;
                if (isPlayer) {
                    State.LastPlayerActivatedActor = __instance;
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
            State.UpdateActorDetection(__instance);
        }
    }

    // Update the visibility checks
    [HarmonyPatch(typeof(Vehicle), "OnMovePhaseComplete")]
    public static class Vehicle_OnMovePhaseComplete {
        public static void Postfix(Vehicle __instance) {
            LowVisibility.Logger.LogIfDebug($"=== Vehicle:OnMovePhaseComplete:post - entered for {ActorLabel(__instance)}.");
            AbstractActor_OnActivationBegin.CheckForJamming(__instance);
            State.UpdateActorDetection(__instance);
        }
    }

}
