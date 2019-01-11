using BattleTech;
using BattleTech.UI;
using Harmony;
using LowVisibility.Helper;
using LowVisibility.Object;
using System.Linq;
using UnityEngine;
using static LowVisibility.Helper.ActorHelper;

namespace LowVisibility.Patch {

    [HarmonyPatch(typeof(AbstractActor), "OnActivationBegin")]
    public static class AbstractActor_OnActivationBegin {        

        public static void Prefix(AbstractActor __instance, int stackItemID) {
            if (stackItemID == -1) {
                // For some bloody reason DoneWithActor() invokes OnActivationBegin, EVEN THOUGH IT DOES NOTHING. GAH!
                return;
            }

            LowVisibility.Logger.LogIfDebug($"=== AbstractActor:OnActivationBegin:pre - handling {ActorLabel(__instance)}.");
            bool isPlayer = __instance.team == __instance.Combat.LocalPlayerTeam;

            State.BuildDynamicState(__instance);
            JammingHelper.ResolveJammingState(__instance);
                        
            if (isPlayer) {
                State.LastPlayerActivatedActorGUID = __instance.GUID;
                if (State.GetDynamicState(__instance).sensorDetectLevel == DetectionLevel.NoInfo) {
                    // Send a floatie indicating the jamming
                    MessageCenter mc = __instance.Combat.MessageCenter;
                    mc.PublishMessage(new FloatieMessage(__instance.GUID, __instance.GUID, "SENSOR CHECK FAILED!", FloatieMessage.MessageNature.Debuff));
                }
            }
            AbstractActor updateActor = isPlayer ? __instance : __instance.Combat.AllActors.Where(aa => aa.TeamId == __instance.Combat.LocalPlayerTeamGuid).First();
            VisibilityHelper.UpdateDetectionForAllActors(__instance.Combat, updateActor);
        }
    }

    // WARNING: This fires on every selection, not just activation.
    //[HarmonyPatch(typeof(CombatSelectionHandler), "TrySelectActor")]
    //public static class CombatSelectionHandler_TrySelectActor {
    //    public static void Postfix(CombatSelectionHandler __instance, bool __result, AbstractActor actor, bool manualSelection) {
    //        LowVisibility.Logger.LogIfDebug($"=== CombatSelectionHandler:TrySelectActor:post - entered for {ActorLabel(actor)}.");
    //        if (__instance != null && actor != null && __result == true) {

    //        }
    //    }
    //}

    // Update the visibility checks
    [HarmonyPatch(typeof(Mech), "OnMovePhaseComplete")]
    public static class Mech_OnMovePhaseComplete {
        public static void Postfix(Mech __instance) {
            LowVisibility.Logger.LogIfDebug($"=== Mech:OnMovePhaseComplete:post - entered for {ActorLabel(__instance)}.");

            JammingHelper.ResolveJammingState(__instance);
            VisibilityHelper.UpdateDetectionForAllActors(__instance.Combat, __instance);
        }
    }

    // Update the visibility checks
    [HarmonyPatch(typeof(Vehicle), "OnMovePhaseComplete")]
    public static class Vehicle_OnMovePhaseComplete {
        public static void Postfix(Vehicle __instance) {
            LowVisibility.Logger.LogIfDebug($"=== Vehicle:OnMovePhaseComplete:post - entered for {ActorLabel(__instance)}.");

            JammingHelper.ResolveJammingState(__instance);
            VisibilityHelper.UpdateDetectionForAllActors(__instance.Combat, __instance);
        }
    }

}
