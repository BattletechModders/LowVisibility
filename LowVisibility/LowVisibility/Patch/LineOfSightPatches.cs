using BattleTech;
using Harmony;
using HBS.Math;
using LowVisibility.Object;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using static LowVisibility.Helper.ActorHelper;
using static LowVisibility.Helper.VisibilityHelper;

namespace LowVisibility.Patch {

    // Modify the visual spotting range based upon environmental conditions only
    [HarmonyPatch(typeof(LineOfSight), "GetSpotterRange")]
    [HarmonyPatch(new Type[] { typeof(AbstractActor) })]
    public static class LineOfSight_GetSpotterRange {
        public static void Postfix(LineOfSight __instance, ref float __result, AbstractActor source, CombatGameState ___Combat) {

            if (__instance != null && source != null) {
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
    }

    // Modify the visual spotting range based upon environmental conditions only
    [HarmonyPatch(typeof(LineOfSight), "GetSensorRange")]
    [HarmonyPatch(new Type[] { typeof(AbstractActor) })]
    public static class LineOfSight_GetSensorState {
        public static void Postfix(LineOfSight __instance, ref float __result, AbstractActor source) {
            if (__instance != null && source != null) {
                __result = CalculateSensorRange(source);
            }
        }
    }

    [HarmonyPatch(typeof(LineOfSight), "GetAdjustedSensorRange")]
    [HarmonyPatch(new Type[] { typeof(AbstractActor), typeof(AbstractActor) })]
    public static class LineOfSight_GetAdjustedSensorRange {
        public static void Postfix(LineOfSight __instance, ref float __result, AbstractActor source, AbstractActor target, CombatGameState ___Combat) {
            if (__instance != null && source != null) {
                float sourceSensorRange = CalculateSensorRange(source);
                float targetSignature = CalculateTargetSignature(target);

                //if (target != null && source.VisibilityToTargetUnit(target) > VisibilityLevel.None) {
                //    // If is sensor lock, add the Hysterisis modifier
                //    signatureModifiedRange += ___Combat.Constants.Visibility.SensorHysteresisAdditive;
                //}

                float signatureModifiedRange = State.IsJammed(source) ? 0.0f : sourceSensorRange * targetSignature;
                //LowVisibility.Logger.Log($"For sourceSensorRange:{sourceSensorRange} and targetSignature:{targetSignature} adjustedRange is:{signatureModifiedRange}");
                __result = signatureModifiedRange;
            }
        }
    }

    [HarmonyPatch(typeof(LineOfSight), "GetTargetVisibility")]
    [HarmonyPatch(new Type[] { typeof(AbstractActor) })]
    public static class LineOfSight_GetTargetVisibility {
        public static void Postfix(LineOfSight __instance, ref float __result, AbstractActor target) {
            if (__instance != null && target != null) {
                __result = CalculateTargetVisibility(target);
            }
        }
    }

    [HarmonyPatch(typeof(LineOfSight), "GetTargetSignature")]
    [HarmonyPatch(new Type[] { typeof(AbstractActor) })]
    public static class LineOfSight_GetTargetSignature {
        public static void Postfix(LineOfSight __instance, ref float __result, AbstractActor target) {
            if (__instance != null && target != null) {
                __result = CalculateTargetSignature(target);
            }
        }
    }

    [HarmonyPatch(typeof(LineOfSight), "GetVisibilityToTargetWithPositionsAndRotations")]
    [HarmonyPatch(new Type[] { typeof(AbstractActor), typeof(Vector3), typeof(ICombatant), typeof(Vector3), typeof(Quaternion) })]
    public static class LineOfSight_GetVisibilityToTargetWithPositionsAndRotations {
        public static bool Prefix(LineOfSight __instance, ref VisibilityLevel __result, 
            AbstractActor source, Vector3 sourcePosition, ICombatant target, Vector3 targetPosition, Quaternion targetRotation) {
            // Skip if we aren't ready to process 
            if (State.TurnDirectorStarted == false) { return true;  }

            //LowVisibility.Logger.Log($"LineOfSight:GetVisibilityToTargetWithPositionsAndRotations:pre - entered. ");
            //LowVisibility.Logger.Log($"LineOfSight:GetVisibilityToTargetWithPositionsAndRotations:pre - stacktrace: {Environment.StackTrace}");

            AbstractActor sourceActor = source as AbstractActor;
            AbstractActor targetActor = target as AbstractActor;

            // If you can spot beyond sensor range, increase sensor range to spotting range
            float adjustedSensorRange = __instance.GetAdjustedSensorRange(source, targetActor);
            float adjustedSpotterRange = __instance.GetAdjustedSpotterRange(source, targetActor);
            if (adjustedSensorRange < adjustedSpotterRange) {
                adjustedSensorRange = adjustedSpotterRange;
            }

            // If you are beyond sensorRange or spotter range, visibility is none
            float distance = Vector3.Distance(sourcePosition, targetPosition);
            if (distance > adjustedSensorRange) {
                __result = VisibilityLevel.None;
                return false;
            }

            Vector3 forward = targetPosition - sourcePosition;
            forward.y = 0f;
            Quaternion rotation = Quaternion.LookRotation(forward);

            // If you are within spotter range, check for visual lock.
            VisibilityLevel visibilityLevel = VisibilityLevel.None;
            if (distance < adjustedSpotterRange) {
                Vector3[] lossourcePositions = source.GetLOSSourcePositions(sourcePosition, rotation);
                Vector3[] lostargetPositions = target.GetLOSTargetPositions(targetPosition, targetRotation);
                for (int i = 0; i < lossourcePositions.Length; i++) {
                    for (int j = 0; j < lostargetPositions.Length; j++) {
                        // If you can visually spot the target, you immediately have detection on then
                        if (__instance.HasLineOfSight(lossourcePositions[i], lostargetPositions[j], adjustedSpotterRange, target.GUID)) {
                            visibilityLevel = VisibilityLevel.LOSFull;
                            break;
                        }
                    }
                    if (visibilityLevel != VisibilityLevel.None) {
                        break;
                    }
                }
            } 

            // If vis is still 0, check tactics to determine what type of blip to display, 0/4/7 -> Blip0, Blip1, Blip4
            //if (visibilityLevel == VisibilityLevel.None && source.IsPilotable) {
            //    int tactics = source.GetPilot().Tactics;
            //    visibilityLevel = __instance.GetVisibilityLevelForTactics(tactics);
            //}
            
            if (targetActor != null) {
                // If you are sensor locked, you are automatically vis 9
                //if (targetActor.IsSensorLocked) {
                //    visibilityLevel = VisibilityLevel.LOSFull;
                //}

                // Determine if any sensor shadows are around 
                //   - any alive, active ally 
                //   - whose SensorSignatureFromDef > this model's SensorSignatureFromDef
                //   - that is within ShadowSignatureDistance ( 1f + sensorsignaturefromdef * MaxShadowingDistance [80] )
                //  provides a +NumShadowingSteps (-1) bonus to the visible units
                //int shadowingVisLevel = (int)visibilityLevel;
                //shadowingVisLevel += targetActor.CurrentShadowingResult;
                //if (shadowingVisLevel > 9) {
                //    visibilityLevel = VisibilityLevel.LOSFull; 
                //} else if (shadowingVisLevel < 0) {
                //    visibilityLevel = VisibilityLevel.None;
                //} else {
                //    visibilityLevel = (VisibilityLevel)shadowingVisLevel;
                //}

                // If EW effects have rendered the target invisible, break the lock
                LockState lockState = GetUnifiedLockStateForTarget(source, targetActor);
                //LowVisibility.Logger.LogIfDebug($"VTTWPAR Source:{ActorLabel(source)} has lockState:{lockState} to target:{targetActor as AbstractActor}");
                if (lockState.visionLockLevel == VisionLockType.None && lockState.sensorLockLevel >= DetectionLevel.Silhouette) {
                    visibilityLevel = VisibilityLevel.Blip4Maximum;
                } else if (lockState.visionLockLevel == VisionLockType.None && lockState.sensorLockLevel >= DetectionLevel.Type) {
                    visibilityLevel = VisibilityLevel.Blip1Type;
                } else if (lockState.visionLockLevel == VisionLockType.None && lockState.sensorLockLevel >= DetectionLevel.Location) {
                    visibilityLevel = VisibilityLevel.BlobMedium;
                } else if (lockState.visionLockLevel == VisionLockType.None && lockState.sensorLockLevel >= DetectionLevel.NoInfo) {
                    visibilityLevel = VisibilityLevel.None;
                }
            }
            __result = visibilityLevel;

            return false;
        }
    }

    [HarmonyPatch(typeof(LineOfSight), "GetLineOfFireUncached")]
    public static class LineOfSight_GetLineOfFireUncached {
        public static void Postfix(LineOfSight __instance, ref LineOfFireLevel __result, CombatGameState ___Combat,
            AbstractActor source, Vector3 sourcePosition, ICombatant target, Vector3 targetPosition, Quaternion targetRotation, out Vector3 collisionWorldPos) {
            //LowVisibility.Logger.Log($"LineOfSight:GetLineOfFireUncached:pre - entered. ");

            Vector3 forward = targetPosition - sourcePosition;
            forward.y = 0f;
            Quaternion rotation = Quaternion.LookRotation(forward);
            Vector3[] lossourcePositions = source.GetLOSSourcePositions(sourcePosition, rotation);
            Vector3[] lostargetPositions = target.GetLOSTargetPositions(targetPosition, targetRotation);

            List<AbstractActor> allActors = new List<AbstractActor>(___Combat.AllActors);
            allActors.Remove(source);

            AbstractActor abstractActor = target as AbstractActor;
            string targetedBuildingGuid = null;
            if (abstractActor != null) {
                allActors.Remove(abstractActor);
            } else {
                targetedBuildingGuid = target.GUID;
            }

            LineSegment lineSegment = new LineSegment(sourcePosition, targetPosition);
            // Sort the target actors by distance from the source
            allActors.Sort((AbstractActor x, AbstractActor y) => 
                Vector3.Distance(x.CurrentPosition, sourcePosition).CompareTo(Vector3.Distance(y.CurrentPosition, sourcePosition))
            );
            float targetPositionDistance = Vector3.Distance(sourcePosition, targetPosition);
            for (int i = allActors.Count - 1; i >= 0; i--) {
                if (allActors[i].IsDead 
                    || Vector3.Distance(allActors[i].CurrentPosition, sourcePosition) > targetPositionDistance 
                    || lineSegment.DistToPoint(allActors[i].CurrentPosition) > allActors[i].Radius * 5f) {
                    // If the actor is 
                    //      1) dead
                    //      2) the distance from actor to source is greater than targetPos distance
                    //      3) the distance to the actor is greater than the radious of all actors (?!?) 
                    //  remove the actor from consideration
                    allActors.RemoveAt(i);
                }
            }
            float sourcePositionsWithLineOfFireToTargetPositions = 0f; // num2
            float losTargetPositionsCount = 0f; // num3
            float weaponsWithUnobstructedLOF = 0f; // num4
            collisionWorldPos = targetPosition;
            float shortestDistanceFromVectorToIteratedActor = 999999.9f; // num5
            Weapon longestRangeWeapon = source.GetLongestRangeWeapon(false, false);
            float maximumWeaponRangeForSource = (longestRangeWeapon != null) ? longestRangeWeapon.MaxRange : 0f;

            // MY CHANGE: 
            float adjustedSpotterRange = ___Combat.LOS.GetAdjustedSpotterRange(source, abstractActor);
            float adjustedSensorRange = ___Combat.LOS.GetAdjustedSensorRange(source, abstractActor);

            //LowVisibility.Logger.Log($"LineOfSight:GetLineOfFireUncached:pre - using sensorRange:{adjustedSensorRange} instead of spotterRange:{adjustedSpotterRange}.  Max weapon range is:{maximumWeaponRangeForSource} ");
            maximumWeaponRangeForSource = Mathf.Max(maximumWeaponRangeForSource, adjustedSensorRange);
            for (int j = 0; j < lossourcePositions.Length; j++) {
                // Iterate the source positions (presumably each weapon has different source locations)
                for (int k = 0; k < lostargetPositions.Length; k++) {
                    // Iterate the target positions (presumably each build/mech has differnet locations)
                    losTargetPositionsCount += 1f;
                    float distanceFromSourceToTarget = Vector3.Distance(lossourcePositions[j], lostargetPositions[k]);
                    if (distanceFromSourceToTarget <= maximumWeaponRangeForSource) {
                        // Possible match, check for collisions
                        lineSegment = new LineSegment(lossourcePositions[j], lostargetPositions[k]);
                        bool canUseDirectAttack = false;
                        Vector3 vector;
                        if (targetedBuildingGuid == null) {
                            // Not a building, so check for compatible actors
                            for (int l = 0; l < allActors.Count; l++) {
                                if (lineSegment.DistToPoint(allActors[l].CurrentPosition) < allActors[l].Radius) {
                                    vector = NvMath.NearestPointStrict(lossourcePositions[j], lostargetPositions[k], allActors[l].CurrentPosition);
                                    float distanceFromVectorToIteratedActor = Vector3.Distance(vector, allActors[l].CurrentPosition);
                                    if (distanceFromVectorToIteratedActor < allActors[l].HighestLOSPosition.y) {
                                        // TODO: Could I have this flipped, and .y is the highest y in the path? This is checking for indirect fire?
                                        // If the height of the attack is less than the HighestLOSPosition.y value, we have found the match?
                                        canUseDirectAttack = true;
                                        weaponsWithUnobstructedLOF += 1f;
                                        if (distanceFromVectorToIteratedActor < shortestDistanceFromVectorToIteratedActor) {
                                            shortestDistanceFromVectorToIteratedActor = distanceFromVectorToIteratedActor;
                                            collisionWorldPos = vector;
                                        }
                                        break;
                                    }
                                }
                            }
                        }

                        // If there is a source position with LOS to the target, record it
                        if (__instance.HasLineOfFire(lossourcePositions[j], lostargetPositions[k], targetedBuildingGuid, maximumWeaponRangeForSource, out vector)) {
                            sourcePositionsWithLineOfFireToTargetPositions += 1f;
                            if (targetedBuildingGuid != null) {
                                break;
                            }
                        } else {
                            // There is no LineOfFire between the source and targert position
                            if (canUseDirectAttack) {
                                weaponsWithUnobstructedLOF -= 1f;
                            }

                            float distanceFromVectorToSourcePosition = Vector3.Distance(vector, sourcePosition);
                            if (distanceFromVectorToSourcePosition < shortestDistanceFromVectorToIteratedActor) {
                                shortestDistanceFromVectorToIteratedActor = distanceFromVectorToSourcePosition;
                                // There is a collection somewhere in the path (MAYBE?)
                                collisionWorldPos = vector;
                            }
                        }
                    }
                }
                if (targetedBuildingGuid != null && sourcePositionsWithLineOfFireToTargetPositions > 0.5f) {
                    break;
                }
            }

            // If a building, ignore the various positions (WHY?)
            float ratioSourcePosToTargetPos = (targetedBuildingGuid != null) ? 
                sourcePositionsWithLineOfFireToTargetPositions : (sourcePositionsWithLineOfFireToTargetPositions / losTargetPositionsCount);

            // "MinRatioFromActors": 0.2,
            float b = ratioSourcePosToTargetPos - ___Combat.Constants.Visibility.MinRatioFromActors;
            float ratioDirectAttacksToTargetPositions = Mathf.Min(weaponsWithUnobstructedLOF / losTargetPositionsCount, b);
            if (ratioDirectAttacksToTargetPositions > 0.001f) {
                ratioSourcePosToTargetPos -= ratioDirectAttacksToTargetPositions;
            }

            //LowVisibility.Logger.Log($"LineOfSight:GetLineOfFireUncached:pre - ratio is:{ratioSourcePosToTargetPos} / direct:{ratioDirectAttacksToTargetPositions} / b:{b}");
            // "RatioFullVis": 0.79,
            // "RatioObstructedVis": 0.41,
            if (ratioSourcePosToTargetPos >= ___Combat.Constants.Visibility.RatioFullVis) {
                __result = LineOfFireLevel.LOFClear;
            } else if (ratioSourcePosToTargetPos >= ___Combat.Constants.Visibility.RatioObstructedVis) {
                __result = LineOfFireLevel.LOFObstructed;
            } else {
                __result = LineOfFireLevel.LOFBlocked;
            }

            //LowVisibility.Logger.Log($"LineOfSight:GetLineOfFireUncached:pre - LOS result is:{__result}");
        }
    }


    // TODO: Duplicate work - make prefix if necessary?
    [HarmonyPatch()]
    public static class FiringPreviewManager_HasLOS {

        // Private method can't be patched by annotations, so use MethodInfo
        public static MethodInfo TargetMethod() {
            return AccessTools.Method(typeof(FiringPreviewManager), "HasLOS", new Type[] { typeof(AbstractActor), typeof(ICombatant), typeof(UnityEngine.Vector3), typeof(List<AbstractActor>) });
        }

        public static void Postfix(FiringPreviewManager __instance, ref bool __result, CombatGameState ___combat,
            AbstractActor attacker, ICombatant target, UnityEngine.Vector3 position, List<AbstractActor> allies) {
            //LowVisibility.Logger.LogIfDebug("FiringPreviewManager:HasLOS:post - entered.");
            for (int i = 0; i < allies.Count; i++) {
                //if (allies[i].VisibilityCache.VisibilityToTarget(target).VisibilityLevel == VisibilityLevel.LOSFull) {
                if (allies[i].VisibilityCache.VisibilityToTarget(target).VisibilityLevel >= VisibilityLevel.Blip0Minimum) {
                    __result = true;
                }
            }
            VisibilityLevel visibilityToTargetWithPositionsAndRotations =
                ___combat.LOS.GetVisibilityToTargetWithPositionsAndRotations(attacker, position, target);
            //__result = visibilityToTargetWithPositionsAndRotations == VisibilityLevel.LOSFull;
            __result = visibilityToTargetWithPositionsAndRotations >= VisibilityLevel.Blip0Minimum;
        }
    }

    [HarmonyPatch(typeof(AbstractActor), "HasLOSToTargetUnit")]
    public static class AbstractActor_HasLOSToTargetUnit {
        public static void Postfix(AbstractActor __instance, ref bool __result, ICombatant targetUnit) {
            //LowVisibility.Logger.LogIfDebug("AbstractActor:HasLOSToTargetUnit:post - entered.");
            __result = __instance.VisibilityToTargetUnit(targetUnit) >= VisibilityLevel.Blip0Minimum;
        }
    }
}
