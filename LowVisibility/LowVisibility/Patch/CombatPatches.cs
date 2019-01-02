using BattleTech;
using BattleTech.UI;
using Harmony;
using LowVisibility.Helper;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
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
                RoundDetectRange detectRange = ActorHelper.MakeSensorRangeCheck(actor);
                LowVisibility.Logger.LogIfDebug($"Actor:{actor.DisplayName}_{actor.GetPilot().Name} has detectRange:{detectRange} this round!");
                State.roundDetectResults[actor.GUID] = detectRange;
            }
        }
    }

    [HarmonyPatch(typeof(AbstractActor), "OnActivationBegin")]
    public static class AbstractActor_OnActivationBegin {
        public static void Prefix(AbstractActor __instance) {
            LowVisibility.Logger.LogIfDebug("AbstractActor:OnActivationBegin:post - entered.");

        }
    }

    // Update the visibility checks
    [HarmonyPatch(typeof(Mech), "OnMoveComplete")]
    public static class Mech_OnMoveComplete {
        public static void Postfix(Mech __instance) {
            LowVisibility.Logger.LogIfDebug($"Mech:OnMoveComplete:post - entered.");

        }
    }


    
}
