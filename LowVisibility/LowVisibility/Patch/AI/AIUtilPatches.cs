using BattleTech;
using Harmony;
using System.Collections.Generic;
using UnityEngine;

namespace LowVisibility.Patch {
    [HarmonyPatch(typeof(AIUtil), "UnitHasVisibilityToTargetFromCurrentPosition")]
    public static class AIUtil_UnitHasVisibilityToTargetFromCurrentPosition {
        public static void Postfix(AIUtil __instance, ref bool __result, AbstractActor attacker, ICombatant target) {
            //LowVisibility.Logger.Debug("AIUtil:UnitHasVisibilityToTargetFromCurrentPosition:post - entered.");
            //__result = attacker.VisibilityToTargetUnit(target) == VisibilityLevel.LOSFull;
            __result = attacker.VisibilityToTargetUnit(target) >= VisibilityLevel.Blip0Minimum;
            //LowVisibility.Logger.Debug($"AIUtil:UnitHasVisibilityToTargetFromCurrentPosition:post - result is:{__result}");
        }
    }

    [HarmonyPatch(typeof(AIUtil), "UnitHasDetectionToTargetFromCurrentPosition")]
    public static class AIUtil_UnitHasDetectionToTargetFromCurrentPosition {
        public static void Postfix(AIUtil __instance, ref bool __result, AbstractActor attacker, ICombatant target) {
            //LowVisibility.Logger.Debug("AIUtil:UnitHasDetectionToTargetFromCurrentPosition:post - entered.");
            //__result = attacker.VisibilityToTargetUnit(target) == VisibilityLevel.LOSFull;
            __result = attacker.VisibilityToTargetUnit(target) >= VisibilityLevel.Blip0Minimum;
            //LowVisibility.Logger.Debug($"AIUtil:UnitHasDetectionToTargetFromCurrentPosition:post - result is:{__result}");
        }
    }

    [HarmonyPatch(typeof(AIUtil), "UnitHasVisibilityToTargetFromPosition")]
    public static class AIUtil_UnitHasVisibilityToTargetFromPosition {
        public static void Postfix(AIUtil __instance, ref bool __result, AbstractActor attacker, ICombatant target, Vector3 position, List<AbstractActor> allies) {
            //LowVisibility.Logger.Debug("AIUtil:UnitHasVisibilityToTargetFromPosition:post - entered.");
            bool alliesHaveVis = false;
            for (int i = 0; i < allies.Count; i++) {
                //if (allies[i].VisibilityCache.VisibilityToTarget(target).VisibilityLevel == VisibilityLevel.LOSFull) {
                if (allies[i].VisibilityCache.VisibilityToTarget(target).VisibilityLevel == VisibilityLevel.Blip0Minimum) {
                    alliesHaveVis = true;
                }
            }
            if (alliesHaveVis) {
                VisibilityLevel visibilityToTargetWithPositionsAndRotations =
                    attacker.Combat.LOS.GetVisibilityToTargetWithPositionsAndRotations(attacker, position, target);
                //__result = visibilityToTargetWithPositionsAndRotations >= VisibilityLevel.LOSFull;
                __result = visibilityToTargetWithPositionsAndRotations >= VisibilityLevel.Blip0Minimum;
            } else {
                __result = true;
            }
            //LowVisibility.Logger.Debug($"AIUtil:UnitHasVisibilityToTargetFromPosition:post - result is:{__result}");
        }
    }
}
