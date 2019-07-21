using BattleTech;
using Harmony;
using LowVisibility.Helper;
using LowVisibility.Object;
using UnityEngine;

namespace LowVisibility.Patch {

    [HarmonyPatch(typeof(PilotableActorRepresentation), "OnPlayerVisibilityChanged")]
    public static class PilotableActorRepresentation_OnPlayerVisibilityChanged {
        public static void Postfix(PilotableActorRepresentation __instance, VisibilityLevel newLevel, CapsuleCollider ___mainCollider) {
            Mod.Log.Trace("PAR:OPVC entered.");

            Traverse parentT = Traverse.Create(__instance).Property("parentActor");
            AbstractActor parentActor = parentT.GetValue<AbstractActor>();

            EWState parentState = new EWState(parentActor);

            if (parentState.HasStealth()) {
                VfxHelper.EnableSensorStealthEffect(parentActor);
            } else {
                VfxHelper.DisableSensorStealthEffect(parentActor);
            }

            if (parentState.HasMimetic()) {
                VfxHelper.EnableVisionStealthEffect(parentActor);
            } else {
                VfxHelper.DisableVisionStealthEffect(parentActor);
            }
        }
    }

}
