using BattleTech;
using Harmony;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace LowVisibility.Patch {

    // Setup the actor and pilot states at the start of the encounter
    [HarmonyPatch(typeof(TurnDirector), "OnEncounterBegin")]
    public static class TurnDirector_OnEncounterBegin {
        public static void Postfix(TurnDirector __instance) {

        }
    }

    [HarmonyPatch(typeof(LineOfSight), "GetVisibilityToTargetWithPositionsAndRotations")]
    [HarmonyPatch(new Type[] { typeof(AbstractActor), typeof(Vector3), typeof(ICombatant), typeof(Vector3), typeof(Quaternion) })]
    public static class LineOfSight_GetVisibilityToTargetWithPositionsAndRotations {
        public static bool Prefix(LineOfSight __instance, ref VisibilityLevel __result, AbstractActor source, Vector3 sourcePosition, ICombatant target, Vector3 targetPosition, Quaternion targetRotation) {
            LowVisibility.Logger.Log($"LineOfSight:GetVisibilityToTargetWithPositionsAndRotations:pre - entered. ");
            AbstractActor abstractActor = target as AbstractActor;

            // UGLY FETHING HACK
            bool hasECM = false;
            if (target.GetType() == typeof(Mech)) {
                Mech targetMech = (Mech)target;
                foreach (MechComponentRef componentRef in targetMech.MechDef.Inventory) {
                    LowVisibility.Logger.Log($"Actor:{targetMech.DisplayName}_{targetMech.GetPilot().Name} has component:{componentRef.ComponentDefID}");
                    if (componentRef.ComponentDefID.Equals("Gear_Guardian_ECM")) {
                        hasECM = true;
                    }
                }
            }


            float adjustedSpotterRange = __instance.GetAdjustedSpotterRange(source, abstractActor);

            float adjustedSensorRange = __instance.GetAdjustedSensorRange(source, abstractActor);
            if (adjustedSensorRange < adjustedSpotterRange) {
                adjustedSensorRange = adjustedSpotterRange;
            }

            float distance = Vector3.Distance(sourcePosition, targetPosition);
            if (distance > adjustedSensorRange) {
                __result = VisibilityLevel.None;
            }
            Vector3 forward = targetPosition - sourcePosition;
            forward.y = 0f;
            Quaternion rotation = Quaternion.LookRotation(forward);

            int visLevel = 0;
            if (distance < adjustedSpotterRange) {
                Vector3[] lossourcePositions = source.GetLOSSourcePositions(sourcePosition, rotation);
                Vector3[] lostargetPositions = target.GetLOSTargetPositions(targetPosition, targetRotation);
                for (int i = 0; i < lossourcePositions.Length; i++) {
                    for (int j = 0; j < lostargetPositions.Length; j++) {
                        if (__instance.HasLineOfSight(lossourcePositions[i], lostargetPositions[j], adjustedSpotterRange, target.GUID)) {
                            visLevel = hasECM ? (int)VisibilityLevel.Blip4Maximum : 9;
                            break;
                        }
                    }
                    if (visLevel != 0) {
                        break;
                    }
                }
            } 
            if (visLevel == 0 && source.IsPilotable) {
                int tactics = source.GetPilot().Tactics;
                visLevel = (int)__instance.GetVisibilityLevelForTactics(tactics);
            }
            if (abstractActor != null) {
                if (abstractActor.IsSensorLocked) {
                    visLevel = 9;
                }
                visLevel += abstractActor.CurrentShadowingResult;
                if (visLevel > 9) {
                    visLevel = 9; 
                } else if (visLevel < 0) {
                    visLevel = 0;
                }
            }
            __result = (VisibilityLevel)visLevel;

            return false;
        }
    }

}
