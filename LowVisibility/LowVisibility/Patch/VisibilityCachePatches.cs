using BattleTech;
using Harmony;
using LowVisibility.Helper;
using System.Collections.Generic;
using UnityEngine;

namespace LowVisibility.Patch {
 
    [HarmonyPatch(typeof(VisibilityCache), "CanDetectPositionNonCached")]
    public static class VisibilityCache_CanDetectPositionNonCached {

        public static void Postfix(VisibilityCache __instance, bool __result, Vector3 worldPos, AbstractActor target) {
            AbstractActor owningActor = (AbstractActor) Traverse.Create(__instance).Property("OwningActor").GetValue();
            //LowVisibility.Logger.Debug($"VC_CDPNC: source{CombatantUtils.Label(owningActor)} checking detection " +
            //    $"from pos:{worldPos} vs. target:{CombatantUtils.Label(target)}");
        }
    }

    [HarmonyPatch(typeof(VisibilityCache), "CanSeeTargetAtPositionNonCached")]
    public static class VisibilityCache_CanSeeTargetAtPositionNonCached {

        public static void Postfix(VisibilityCache __instance, bool __result, Vector3 worldPos, AbstractActor target) {
            AbstractActor owningActor = (AbstractActor)Traverse.Create(__instance).Property("OwningActor").GetValue();
            //LowVisibility.Logger.Debug($"VC_CSTAPNC: source{CombatantUtils.Label(owningActor)} checking vision" +
            //    $"from pos:{worldPos} vs. target:{CombatantUtils.Label(target)}");
        }
    }

    [HarmonyPatch(typeof(VisibilityCache), "UpdateCacheReciprocal")]
    public static class VisibilityCache_UpdateCacheReciprocal {

        public static void Postfix(VisibilityCache __instance, List<ICombatant> allCombatants) {
            AbstractActor owningActor = (AbstractActor)Traverse.Create(__instance).Property("OwningActor").GetValue();

            //LowVisibility.Logger.Log($"VC_UCR: source{CombatantUtils.Label(owningActor)} updating vision to combatants");
            //foreach (ICombatant combatant in allCombatants) {
            //    LowVisibility.Logger.Log($"  -- target:{CombatantUtils.Label(combatant)}");
            //}
        }
    }

    [HarmonyPatch(typeof(VisibilityCache), "CalcVisValueToTarget")]
    public static class VisibilityCache_CalcVisValueToTarget {

        public static void Postfix(VisibilityCache __instance, VisibilityLevelAndAttribution __result, ICombatant target) {
            AbstractActor owningActor = (AbstractActor)Traverse.Create(__instance).Property("OwningActor").GetValue();
            //LowVisibility.Logger.Debug($"VC_CVVTT: source{CombatantUtils.Label(owningActor)} updating vision to " +
            //    $"target:{CombatantUtils.Label(target)} with " +
            //    $"result:{__result.VisibilityLevel}/{__result.LineOfFireLevel}/{__result.LineOfFireCollision}");
        }
    }

    [HarmonyPatch(typeof(SharedVisibilityCache), "CanDetectPositionNonCached")]
    public static class SharedVisibilityCache_CanDetectPositionNonCached {

        public static void Postfix(SharedVisibilityCache __instance, bool __result, Vector3 worldPos, AbstractActor target) {            
            //LowVisibility.Logger.Debug($"SVC_CDPNC: shared cache checking detection " +
            //    $"from pos:{worldPos} vs. target:{CombatantUtils.Label(target)}");
        }
    }

    [HarmonyPatch(typeof(SharedVisibilityCache), "CanSeeTargetAtPositionNonCached")]
    public static class SharedVisibilityCache_CanSeeTargetAtPositionNonCached {

        public static void Postfix(SharedVisibilityCache __instance, bool __result, Vector3 worldPos, AbstractActor target) {            
            //LowVisibility.Logger.Debug($"SVC_CSTAPNC: shared cache checking vision " +
            //    $"from pos:{worldPos} vs. target:{CombatantUtils.Label(target)}");
        }
    }

    [HarmonyPatch(typeof(AbstractActor), "CanDetectPositionNonCached")]
    public static class AbstractActor_CanDetectPositionNonCached {

        public static void Postfix(AbstractActor __instance, bool __result, Vector3 worldPos, AbstractActor target) {
            //LowVisibility.Logger.Debug($"AA_CDPNC: source{CombatantUtils.Label(__instance)} checking detection " +
            //    $"from pos:{worldPos} vs. target:{CombatantUtils.Label(target)}");
        }
    }


    [HarmonyPatch(typeof(AbstractActor), "CanSeeTargetAtPositionNonCached")]
    public static class AbstractActor_CanSeeTargetAtPositionNonCached {

        public static void Postfix(AbstractActor __instance, bool __result, Vector3 worldPos, AbstractActor target) {
            //LowVisibility.Logger.Debug($"AA_CSTAPNC: source{__instance} checking vision" +
            //    $"from pos:{worldPos} vs. target:{CombatantUtils.Label(target)}");
        }
    }

}
