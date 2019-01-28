using BattleTech;
using Harmony;
using LowVisibility.Helper;
using System.Collections.Generic;
using UnityEngine;

namespace LowVisibility.Patch {
 
    [HarmonyPatch(typeof(VisibilityCache), "CanDetectPositionNonCached")]
    public static class VisibilityCache_CanDetectPositionNonCached {

        public static void Postfix(VisibilityCache __instance, bool __result, Vector3 worldPos, AbstractActor target) {
            LowVisibility.Logger.Log($"VC_CDPNC: entered");
            AbstractActor owningActor = (AbstractActor) Traverse.Create(__instance).Property("OwningActor").GetValue();
            LowVisibility.Logger.Log($"VC_CDPNC: source{CombatantHelper.Label(owningActor)} checking detection " +
                $"from pos:{worldPos} vs. target:{CombatantHelper.Label(target)}");
        }
    }

    [HarmonyPatch(typeof(VisibilityCache), "CanSeeTargetAtPositionNonCached")]
    public static class VisibilityCache_CanSeeTargetAtPositionNonCached {

        public static void Postfix(VisibilityCache __instance, bool __result, Vector3 worldPos, AbstractActor target) {
            LowVisibility.Logger.Log($"VC_CSTAPNC: entered");
            AbstractActor owningActor = (AbstractActor)Traverse.Create(__instance).Property("OwningActor").GetValue();
            LowVisibility.Logger.Log($"VC_CSTAPNC: source{CombatantHelper.Label(owningActor)} checking vision" +
                $"from pos:{worldPos} vs. target:{CombatantHelper.Label(target)}");
        }
    }

    [HarmonyPatch(typeof(VisibilityCache), "UpdateCacheReciprocal")]
    public static class VisibilityCache_UpdateCacheReciprocal {

        public static void Postfix(VisibilityCache __instance, List<ICombatant> allCombatants) {
            LowVisibility.Logger.Log($"VC_UCR: entered");
            AbstractActor owningActor = (AbstractActor)Traverse.Create(__instance).Property("OwningActor").GetValue();

            LowVisibility.Logger.Log($"VC_UCR: source{CombatantHelper.Label(owningActor)} updating vision to combatants");
            foreach (ICombatant combatant in allCombatants) {
                LowVisibility.Logger.Log($"  -- target:{CombatantHelper.Label(combatant)}");
            }
        }
    }

    [HarmonyPatch(typeof(VisibilityCache), "CalcVisValueToTarget")]
    public static class VisibilityCache_CalcVisValueToTarget {

        public static void Postfix(VisibilityCache __instance, ICombatant target) {
            LowVisibility.Logger.Log($"VC_CVVTT: entered");
            AbstractActor owningActor = (AbstractActor)Traverse.Create(__instance).Property("OwningActor").GetValue();
            LowVisibility.Logger.Log($"VC_CVVTT: source{CombatantHelper.Label(owningActor)} updating vision to target:{CombatantHelper.Label(target)}");
        }
    }

    [HarmonyPatch(typeof(SharedVisibilityCache), "CanDetectPositionNonCached")]
    public static class SharedVisibilityCache_CanDetectPositionNonCached {

        public static void Postfix(SharedVisibilityCache __instance, bool __result, Vector3 worldPos, AbstractActor target) {            
            LowVisibility.Logger.Log($"SVC_CDPNC: shared cache checking detection " +
                $"from pos:{worldPos} vs. target:{CombatantHelper.Label(target)}");
        }
    }

    [HarmonyPatch(typeof(SharedVisibilityCache), "CanSeeTargetAtPositionNonCached")]
    public static class SharedVisibilityCache_CanSeeTargetAtPositionNonCached {

        public static void Postfix(SharedVisibilityCache __instance, bool __result, Vector3 worldPos, AbstractActor target) {            
            LowVisibility.Logger.Log($"SVC_CSTAPNC: shared cache checking vision" +
                $"from pos:{worldPos} vs. target:{CombatantHelper.Label(target)}");
        }
    }

    [HarmonyPatch(typeof(AbstractActor), "CanDetectPositionNonCached")]
    public static class AbstractActor_CanDetectPositionNonCached {

        public static void Postfix(AbstractActor __instance, bool __result, Vector3 worldPos, AbstractActor target) {
            LowVisibility.Logger.Log($"AA_CDPNC: source{CombatantHelper.Label(__instance)} checking detection " +
                $"from pos:{worldPos} vs. target:{CombatantHelper.Label(target)}");
        }
    }


    [HarmonyPatch(typeof(AbstractActor), "CanSeeTargetAtPositionNonCached")]
    public static class AbstractActor_CanSeeTargetAtPositionNonCached {

        public static void Postfix(AbstractActor __instance, bool __result, Vector3 worldPos, AbstractActor target) {
            LowVisibility.Logger.Log($"AA_CSTAPNC: source{__instance} checking vision" +
                $"from pos:{worldPos} vs. target:{CombatantHelper.Label(target)}");
        }
    }

}
