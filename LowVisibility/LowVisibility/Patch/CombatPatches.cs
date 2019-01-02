using BattleTech;
using BattleTech.UI;
using Harmony;
using LowVisibility.Helper;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using UnityEngine;
using static LowVisibility.Helper.ActorHelper;

namespace LowVisibility.Patch {

    // Setup the actor and pilot states at the start of the encounter
    [HarmonyPatch(typeof(TurnDirector), "OnEncounterBegin")]
    public static class TurnDirector_OnEncounterBegin {

        public static void Postfix(TurnDirector __instance) {
            LowVisibility.Logger.LogIfDebug("TurnDirector:OnEncounterBegin:post - entered.");
        }
    }

    [HarmonyPatch()]
    public static class TurnDirector_BeginNewRound {

        // Private method can't be patched by annotations, so use MethodInfo
        public static MethodInfo TargetMethod() {
            return AccessTools.Method(typeof(TurnDirector), "BeginNewRound", new Type[] { typeof(int) });
        }

        public static void Postfix(TurnDirector __instance) {
            LowVisibility.Logger.LogIfDebug("TurnDirector:BeginNewRound:post - entered.");
            foreach (AbstractActor actor in __instance.Combat.AllActors) {
                RoundDetectRange detectRange = MakeSensorRangeCheck(actor);
                LowVisibility.Logger.LogIfDebug($"Actor:{actor.DisplayName}_{actor.GetPilot().Name} has detectRange:{detectRange} this round!");
                State.roundDetectResults[actor.GUID] = detectRange;
                State.UpdateActorIDLevel(actor);
            }
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
                LowVisibility.Logger.Log($"Found enemy actor:{enemyActor.DisplayName}_{enemyActor.GetPilot().Name}. \nenemyEWConfig:{enemyEWConfig.ToString()} \nsourceEWConfig:{sourceEWConfig.ToString()}");
                if (sourceEWConfig.probeTier < enemyEWConfig.ecmTier) {
                    LowVisibility.Logger.Log($"Target:{enemyActor.DisplayName}_{enemyActor.GetPilot().Name} has ECM tier{enemyEWConfig.ecmTier} vs. source Probe tier:{sourceEWConfig.probeTier}");                    
                    if (actorsDistance > enemyEWConfig.ecmRange) {
                        LowVisibility.Logger.Log($"Actors are {actorsDistance}m apart, outside of ECM bubble range of:{enemyEWConfig.ecmRange}");
                    } else {
                        LowVisibility.Logger.Log($"Source:{source.DisplayName}_{source.GetPilot().Name} is within stronger ECM bubble of enemy:{enemyActor.DisplayName}_{enemyActor.GetPilot().Name}");
                        if (enemyEWConfig.ecmModifier > sourceJammingStrength) { sourceJammingStrength = enemyEWConfig.ecmModifier; }
                    }
                }

                // TODO: APPLY ECM MODIFIER TO DETECT CHECK

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
            LowVisibility.Logger.LogIfDebug($"AbstractActor:OnActivationBegin:post - handling {__instance.DisplayName}_{__instance.GetPilot().Name}.");
            if (__instance != null) {
                CheckForJamming(__instance);
                State.UpdateActorIDLevel(__instance);
            }
        }
    }

    // Update the visibility checks
    [HarmonyPatch(typeof(Mech), "OnMovePhaseComplete")]
    public static class Mech_OnMovePhaseComplete {
        public static void Postfix(Mech __instance) {
            LowVisibility.Logger.LogIfDebug($"Mech:OnMovePhaseComplete:post - entered.");
            AbstractActor_OnActivationBegin.CheckForJamming(__instance);
            State.UpdateActorIDLevel(__instance);
        }
    }

    // Update the visibility checks
    [HarmonyPatch(typeof(Vehicle), "OnMovePhaseComplete")]
    public static class Vehicle_OnMovePhaseComplete {
        public static void Postfix(Vehicle __instance) {
            LowVisibility.Logger.LogIfDebug($"Vehicle:OnMovePhaseComplete:post - entered.");
            AbstractActor_OnActivationBegin.CheckForJamming(__instance);
        }
    }

}
