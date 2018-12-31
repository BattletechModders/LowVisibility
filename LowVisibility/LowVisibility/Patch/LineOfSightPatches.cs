using BattleTech;
using Harmony;
using System;
using UnityEngine;
using static LowVisibility.Helper.ActorHelper;

namespace LowVisibility.Patch {

    // Modify the visual spotting range based upon environmental conditions only
    [HarmonyPatch(typeof(LineOfSight), "GetSpotterRange")]
    [HarmonyPatch(new Type[] { typeof(AbstractActor) })]
    public static class LineOfSight_GetSpotterRange {
        public static void Postfix(LineOfSight __instance, ref float __result, AbstractActor source) {
            CombatGameState ___Combat = (CombatGameState)Traverse.Create(__instance).Property("Combat").GetValue();
            
            //float baseSpotterDistance = ___Combat.Constants.Visibility.BaseSpotterDistance;
            float baseSpotterDistance = State.GetMapVisionRange();
            if (source.IsShutDown) {
                __result = baseSpotterDistance * ___Combat.Constants.Visibility.ShutdownSpottingDistanceMultiplier;
            }
            if (source.IsProne) {
                __result = baseSpotterDistance * ___Combat.Constants.Visibility.ProneSpottingDistanceMultiplier;
            }
            float allSpotterMultipliers = __instance.GetAllSpotterMultipliers(source);
            float allSpotterAbsolutes = __instance.GetAllSpotterAbsolutes(source);
            __result = baseSpotterDistance * allSpotterMultipliers + allSpotterAbsolutes;
        }
    }

    [HarmonyPatch(typeof(LineOfSight), "GetAdjustedSensorRange")]
    [HarmonyPatch(new Type[] { typeof(AbstractActor), typeof(AbstractActor) })]
    public static class LineOfSight_GetAdjustedSensorRange {
        public static void Postfix(LineOfSight __instance, ref float __result, AbstractActor source, AbstractActor target, CombatGameState ___Combat) {
            if (__instance != null) {
                //CombatGameState ___Combat = (CombatGameState)Traverse.Create(__instance).Property("Combat").GetValue();

                //float sensorRange = __instance.GetSensorRange(source);
                RoundDetectRange detectRange = State.GetOrCreateRoundDetectResults(source);
                ActorEWConfig ewConfig = State.GetOrCreateActorEWConfig(source);
                float sensorRange = ewConfig.sensorsRange * (int)detectRange;
                LowVisibility.Logger.LogIfDebug($"Actor:{source.DisplayName}_{source.GetPilot().Name} has sensorsRange:{sensorRange}");

                float targetSignature = __instance.GetTargetSignature(target);
                LowVisibility.Logger.LogIfDebug($"Target signature is:{targetSignature}");
                float signatureModifiedRange = sensorRange * targetSignature;
                if (target != null && source.VisibilityToTargetUnit(target) > VisibilityLevel.None) {
                    // If is sensor lock, add the Hysterisis modifier
                    signatureModifiedRange += ___Combat.Constants.Visibility.SensorHysteresisAdditive;
                }
                LowVisibility.Logger.LogIfDebug($"SensorRange:{sensorRange} modified by targetSignature:{targetSignature} is: {signatureModifiedRange}");
                __result = signatureModifiedRange;
            }
        }
    }

    [HarmonyPatch(typeof(LineOfSight), "GetVisibilityToTargetWithPositionsAndRotations")]
    [HarmonyPatch(new Type[] { typeof(AbstractActor), typeof(Vector3), typeof(ICombatant), typeof(Vector3), typeof(Quaternion) })]
    public static class LineOfSight_GetVisibilityToTargetWithPositionsAndRotations {
        public static bool Prefix(LineOfSight __instance, ref VisibilityLevel __result, 
            AbstractActor source, Vector3 sourcePosition, ICombatant target, Vector3 targetPosition, Quaternion targetRotation) {
            //LowVisibility.Logger.Log($"LineOfSight:GetVisibilityToTargetWithPositionsAndRotations:pre - entered. ");

            AbstractActor sourceActor = source as AbstractActor;
            AbstractActor targetActor = target as AbstractActor;

            float adjustedSpotterRange = __instance.GetAdjustedSpotterRange(source, targetActor);
            // If you can spot beyond sensor range, increase sensor range to spotting range
            float adjustedSensorRange = __instance.GetAdjustedSensorRange(source, targetActor);
            if (adjustedSensorRange < adjustedSpotterRange) {
                adjustedSensorRange = adjustedSpotterRange;
            }

            // If you are beyond sensorRange, visibility is none
            float distance = Vector3.Distance(sourcePosition, targetPosition);
            if (distance > adjustedSensorRange) {
                __result = VisibilityLevel.None;
            }

            Vector3 forward = targetPosition - sourcePosition;
            forward.y = 0f;
            Quaternion rotation = Quaternion.LookRotation(forward);

            // If you are within spotter range, check for direct LOS. That provides an immediate '9'
            int visLevel = 0;
            if (distance < adjustedSpotterRange) {
                Vector3[] lossourcePositions = source.GetLOSSourcePositions(sourcePosition, rotation);
                Vector3[] lostargetPositions = target.GetLOSTargetPositions(targetPosition, targetRotation);
                for (int i = 0; i < lossourcePositions.Length; i++) {
                    for (int j = 0; j < lostargetPositions.Length; j++) {
                        // If you can spot the target, you immediately have detection on then
                        if (__instance.HasLineOfSight(lossourcePositions[i], lostargetPositions[j], adjustedSpotterRange, target.GUID)) {
                            visLevel = 9;
                            break;
                        }
                    }
                    if (visLevel != 0) {
                        break;
                    }
                }
            } 

            // If vis is still 0, check tactics to determine what type of blip to display, 0/4/7 -> Blip0, Blip1, Blip4
            if (visLevel == 0 && source.IsPilotable) {
                int tactics = source.GetPilot().Tactics;
                visLevel = (int)__instance.GetVisibilityLevelForTactics(tactics);
            }
            
            if (targetActor != null) {
                // If you are sensor locked, you are automatically vis 9
                if (targetActor.IsSensorLocked) {
                    visLevel = 9;
                }

                // Determine if any sensor shadows are around 
                //   - any alive, active ally 
                //   - whose SensorSignatureFromDef > this model's SensorSignatureFromDef
                //   - that is within ShadowSignatureDistance ( 1f + sensorsignaturefromdef * MaxShadowingDistance [80] )
                //  provides a +NumShadowingSteps (-1) bonus to the visible units
                visLevel += targetActor.CurrentShadowingResult;
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
