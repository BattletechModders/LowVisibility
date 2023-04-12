using System.Collections.Generic;
using UnityEngine;

namespace LowVisibility.Patch
{
    [HarmonyPatch(typeof(AIUtil), "UnitHasVisibilityToTargetFromCurrentPosition")]
    public static class AIUtil_UnitHasVisibilityToTargetFromCurrentPosition
    {
        public static void Postfix(AIUtil __instance, ref bool __result, AbstractActor attacker, ICombatant target)
        {
            __result = attacker.VisibilityToTargetUnit(target) >= VisibilityLevel.Blip0Minimum;
        }
    }

    [HarmonyPatch(typeof(AIUtil), "UnitHasDetectionToTargetFromCurrentPosition")]
    public static class AIUtil_UnitHasDetectionToTargetFromCurrentPosition
    {
        public static void Postfix(AIUtil __instance, ref bool __result, AbstractActor attacker, ICombatant target)
        {
            __result = attacker.VisibilityToTargetUnit(target) >= VisibilityLevel.Blip0Minimum;
        }
    }

    [HarmonyPatch(typeof(AIUtil), "UnitHasVisibilityToTargetFromPosition")]
    public static class AIUtil_UnitHasVisibilityToTargetFromPosition
    {
        public static void Postfix(AIUtil __instance, ref bool __result, AbstractActor attacker, ICombatant target, Vector3 position, List<AbstractActor> allies)
        {
            bool alliesHaveVis = false;
            for (int i = 0; i < allies.Count; i++)
            {
                if (allies[i].VisibilityCache.VisibilityToTarget(target).VisibilityLevel == VisibilityLevel.Blip0Minimum)
                {
                    alliesHaveVis = true;
                }
            }
            if (alliesHaveVis)
            {
                VisibilityLevel visibilityToTargetWithPositionsAndRotations =
                    attacker.Combat.LOS.GetVisibilityToTargetWithPositionsAndRotations(attacker, position, target);
                __result = visibilityToTargetWithPositionsAndRotations >= VisibilityLevel.Blip0Minimum;
            }
            else
            {
                __result = true;
            }
        }
    }
}
