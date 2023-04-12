using IRBTModUtils.Extension;
using LowVisibility.Object;
using UnityEngine;

namespace LowVisibility.Patch
{

    [HarmonyPatch(typeof(Vehicle), "OnPositionUpdate")]
    public static class Vehicle_OnPositionUpdate
    {
        // public override void OnPositionUpdate(Vector3 position, Quaternion heading, int stackItemUID, bool updateDesignMask, List<DesignMaskDef> remainingMasks, bool skipLogging = false)
        public static void Postfix(Vehicle __instance, Vector3 position, Quaternion heading, int stackItemUID)
        {
            if (stackItemUID == -1 || __instance == null)
            {
                // Nothing to do? Why were we even called?!?
                return;
            }

            Mod.Log.Info?.Write($"== Vehicle: {__instance.DistinctId()} updated to position: {__instance.CurrentPosition} rot: {__instance.CurrentRotation} from position: {__instance.PreviousPosition} rot: {__instance.PreviousRotation}");
            EWState actorState = new EWState(__instance);
            if (actorState.HasMimetic())
            {
                Mod.Log.Info?.Write($"  Mimetic pips updated to: {actorState.CurrentMimeticPips()}");
            }

        }
    }

}

