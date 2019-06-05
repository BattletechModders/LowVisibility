using BattleTech;
using Harmony;
using System.Collections.Generic;
using UnityEngine;

namespace LowVisibility.Patch {
 
    //[HarmonyPatch(typeof(VisibilityCache), "CanDetectPositionNonCached")]
    //public static class VisibilityCache_CanDetectPositionNonCached {

    //    public static void Postfix(VisibilityCache __instance, bool __result, Vector3 worldPos, AbstractActor target) {
    //        AbstractActor owningActor = (AbstractActor) Traverse.Create(__instance).Property("OwningActor").GetValue();
    //    }
    //}

    //[HarmonyPatch(typeof(VisibilityCache), "CanSeeTargetAtPositionNonCached")]
    //public static class VisibilityCache_CanSeeTargetAtPositionNonCached {

    //    public static void Postfix(VisibilityCache __instance, bool __result, Vector3 worldPos, AbstractActor target) {
    //        AbstractActor owningActor = (AbstractActor)Traverse.Create(__instance).Property("OwningActor").GetValue();

    //    }
    //}

    //[HarmonyPatch(typeof(VisibilityCache), "UpdateCacheReciprocal")]
    //public static class VisibilityCache_UpdateCacheReciprocal {

    //    public static void Postfix(VisibilityCache __instance, List<ICombatant> allLivingCombatants) {
    //        AbstractActor owningActor = (AbstractActor)Traverse.Create(__instance).Property("OwningActor").GetValue();

    //    }
    //}

    //[HarmonyPatch(typeof(VisibilityCache), "CalcVisValueToTarget")]
    //public static class VisibilityCache_CalcVisValueToTarget {

    //    public static void Postfix(VisibilityCache __instance, VisibilityLevelAndAttribution __result, ICombatant livingTarget) {
    //        AbstractActor owningActor = (AbstractActor)Traverse.Create(__instance).Property("OwningActor").GetValue();

    //    }
    //}

    //[HarmonyPatch(typeof(SharedVisibilityCache), "CanDetectPositionNonCached")]
    //public static class SharedVisibilityCache_CanDetectPositionNonCached {

    //    public static void Postfix(SharedVisibilityCache __instance, bool __result, Vector3 worldPos, AbstractActor target) {            

    //    }
    //}

    //[HarmonyPatch(typeof(SharedVisibilityCache), "CanSeeTargetAtPositionNonCached")]
    //public static class SharedVisibilityCache_CanSeeTargetAtPositionNonCached {

    //    public static void Postfix(SharedVisibilityCache __instance, bool __result, Vector3 worldPos, AbstractActor target) {            

    //    }
    //}


}
