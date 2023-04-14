using HBS.Math;
using LowVisibility.Helper;
using LowVisibility.Object;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using us.frostraptor.modUtils;

namespace LowVisibility.Patch
{

    // Used to show spotting range in Fog of War
    [HarmonyPatch(typeof(LineOfSight), "GetSpotterRange")]
    [HarmonyPatch(new Type[] { typeof(AbstractActor) })]
    public static class LineOfSight_GetSpotterRange
    {
        public static void Postfix(LineOfSight __instance, ref float __result, AbstractActor source, CombatGameState ___Combat)
        {
            if (__instance != null && source != null)
            {
                __result = VisualLockHelper.GetSpotterRange(source);
            }
        }
    }

    // Called to determine if something is visible
    [HarmonyPatch(typeof(LineOfSight), "GetAdjustedSpotterRange")]
    [HarmonyPatch(new Type[] { typeof(AbstractActor), typeof(ICombatant) })]
    public static class LineOfSight_GetAdjustedSpotterRange
    {
        public static void Postfix(LineOfSight __instance, ref float __result, AbstractActor source, ICombatant target)
        {
            if (__instance != null && source != null)
            {
                __result = VisualLockHelper.GetAdjustedSpotterRange(source, target);
            }
        }
    }

    // Used to calculate possible targets
    [HarmonyPatch(typeof(LineOfSight), "GetSensorRange")]
    [HarmonyPatch(new Type[] { typeof(AbstractActor) })]
    public static class LineOfSight_GetSensorState
    {
        public static void Postfix(LineOfSight __instance, ref float __result, AbstractActor source)
        {
            if (__instance != null && source != null)
            {
                __result = SensorLockHelper.GetSensorsRange(source);
            }
        }
    }

    // Called by AI and other states to see what you can actually detected
    [HarmonyPatch(typeof(LineOfSight), "GetAdjustedSensorRange")]
    [HarmonyPatch(new Type[] { typeof(AbstractActor), typeof(AbstractActor) })]
    public static class LineOfSight_GetAdjustedSensorRange
    {
        public static void Postfix(LineOfSight __instance, ref float __result, AbstractActor source, AbstractActor target, CombatGameState ___Combat)
        {
            if (__instance != null && source != null)
            {
                __result = SensorLockHelper.GetAdjustedSensorRange(source, target);
            }
        }
    }

    [HarmonyPatch(typeof(LineOfSight), "GetVisibilityToTargetWithPositionsAndRotations")]
    [HarmonyPatch(new Type[] { typeof(AbstractActor), typeof(Vector3), typeof(ICombatant), typeof(Vector3), typeof(Quaternion) })]
    public static class LineOfSight_GetVisibilityToTargetWithPositionsAndRotations
    {
        public static void Prefix(ref bool __runOriginal, LineOfSight __instance, ref VisibilityLevel __result,
            AbstractActor source, Vector3 sourcePosition, ICombatant target, Vector3 targetPosition, Quaternion targetRotation)
        {
            if (!__runOriginal) return;

            Mod.Log.Trace?.Write($"LOS:GVTTWPAR: source:{CombatantUtils.Label(source)} ==> target:{CombatantUtils.Label(target)}");

            // Skip if we aren't ready to process
            // TODO: Is this necessary anymore?
            //if (State.TurnDirectorStarted == false || (target as AbstractActor) == null) { return true;  }

            AbstractActor sourceActor = source as AbstractActor;

            // TODO: Handle buildings here
            bool sourceHasLineOfSight = VisualLockHelper.CanSpotTarget(sourceActor, sourcePosition, target, targetPosition, targetRotation, __instance);
            if (sourceHasLineOfSight)
            {
                __result = VisibilityLevel.LOSFull;
            }
            else
            {
                VisibilityLevel sensorsVisibility = VisibilityLevel.None;
                if (ModState.TurnDirectorStarted)
                {
                    SensorScanType sensorLock = SensorLockHelper.CalculateSensorLock(sourceActor, sourcePosition, target, targetPosition);
                    sensorsVisibility = sensorLock.Visibility();
                }
                __result = sensorsVisibility;
            }

            //Mod.Log.Trace?.Write($"LOS:GVTTWPAR - [{__result}] visibility for source:{CombatantUtils.Label(source)} ==> target:{CombatantUtils.Label(target)}");
            __runOriginal = false;
        }
    }

    [HarmonyPatch(typeof(LineOfSight), "GetLineOfFireUncached")]
    public static class LineOfSight_GetLineOfFireUncached
    {
        public static void Prefix(ref bool __runOriginal, LineOfSight __instance, ref LineOfFireLevel __result, CombatGameState ___Combat,
            AbstractActor source, Vector3 sourcePosition, ICombatant target, Vector3 targetPosition, Quaternion targetRotation, out Vector3 collisionWorldPos)
        {
            if (!__runOriginal)
            {
                collisionWorldPos = Vector3.zero;
                return;
            }

            Mod.Log.Trace?.Write($"LOS:GLOFU entered. ");

            Vector3 forward = targetPosition - sourcePosition;
            forward.y = 0f;
            Quaternion rotation = Quaternion.LookRotation(forward);
            Vector3[] lossourcePositions = source.GetLOSSourcePositions(sourcePosition, rotation);
            Vector3[] lostargetPositions = target.GetLOSTargetPositions(targetPosition, targetRotation);

            List<AbstractActor> allActors = new List<AbstractActor>(___Combat.AllActors);
            allActors.Remove(source);

            AbstractActor abstractActor = target as AbstractActor;
            string targetedBuildingGuid = null;
            if (abstractActor != null)
            {
                allActors.Remove(abstractActor);
            }
            else
            {
                targetedBuildingGuid = target.GUID;
            }

            LineSegment lineSegment = new LineSegment(sourcePosition, targetPosition);
            // Sort the target actors by distance from the source
            allActors.Sort((AbstractActor x, AbstractActor y) =>
                Vector3.Distance(x.CurrentPosition, sourcePosition).CompareTo(Vector3.Distance(y.CurrentPosition, sourcePosition))
            );
            float targetPositionDistance = Vector3.Distance(sourcePosition, targetPosition);
            for (int i = allActors.Count - 1; i >= 0; i--)
            {
                if (allActors[i].IsDead
                    || Vector3.Distance(allActors[i].CurrentPosition, sourcePosition) > targetPositionDistance
                    || lineSegment.DistToPoint(allActors[i].CurrentPosition) > allActors[i].Radius * 5f)
                {
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
            maximumWeaponRangeForSource = Mathf.Max(maximumWeaponRangeForSource, adjustedSensorRange, adjustedSpotterRange);
            for (int j = 0; j < lossourcePositions.Length; j++)
            {
                // Iterate the source positions (presumably each weapon has different source locations)
                for (int k = 0; k < lostargetPositions.Length; k++)
                {
                    // Iterate the target positions (presumably each build/mech has differnet locations)
                    losTargetPositionsCount += 1f;
                    float distanceFromSourceToTarget = Vector3.Distance(lossourcePositions[j], lostargetPositions[k]);
                    if (distanceFromSourceToTarget <= maximumWeaponRangeForSource)
                    {
                        // Possible match, check for collisions
                        lineSegment = new LineSegment(lossourcePositions[j], lostargetPositions[k]);
                        bool canUseDirectAttack = false;
                        Vector3 vector;
                        if (targetedBuildingGuid == null)
                        {
                            // Not a building, so check for compatible actors
                            for (int l = 0; l < allActors.Count; l++)
                            {
                                if (lineSegment.DistToPoint(allActors[l].CurrentPosition) < allActors[l].Radius)
                                {
                                    vector = NvMath.NearestPointStrict(lossourcePositions[j], lostargetPositions[k], allActors[l].CurrentPosition);
                                    float distanceFromVectorToIteratedActor = Vector3.Distance(vector, allActors[l].CurrentPosition);
                                    if (distanceFromVectorToIteratedActor < allActors[l].HighestLOSPosition.y)
                                    {
                                        // TODO: Could I have this flipped, and .y is the highest y in the path? This is checking for indirect fire?
                                        // If the height of the attack is less than the HighestLOSPosition.y value, we have found the match?
                                        canUseDirectAttack = true;
                                        weaponsWithUnobstructedLOF += 1f;
                                        if (distanceFromVectorToIteratedActor < shortestDistanceFromVectorToIteratedActor)
                                        {
                                            shortestDistanceFromVectorToIteratedActor = distanceFromVectorToIteratedActor;
                                            collisionWorldPos = vector;
                                        }
                                        break;
                                    }
                                }
                            }
                        }

                        // If there is a source position with LOS to the target, record it
                        if (__instance.HasLineOfFire(lossourcePositions[j], lostargetPositions[k], targetedBuildingGuid, maximumWeaponRangeForSource, out vector))
                        {
                            sourcePositionsWithLineOfFireToTargetPositions += 1f;
                            if (targetedBuildingGuid != null)
                            {
                                break;
                            }
                        }
                        else
                        {
                            // There is no LineOfFire between the source and targert position
                            if (canUseDirectAttack)
                            {
                                weaponsWithUnobstructedLOF -= 1f;
                            }

                            float distanceFromVectorToSourcePosition = Vector3.Distance(vector, sourcePosition);
                            if (distanceFromVectorToSourcePosition < shortestDistanceFromVectorToIteratedActor)
                            {
                                shortestDistanceFromVectorToIteratedActor = distanceFromVectorToSourcePosition;
                                // There is a collection somewhere in the path (MAYBE?)
                                collisionWorldPos = vector;
                            }
                        }
                    }
                }
                if (targetedBuildingGuid != null && sourcePositionsWithLineOfFireToTargetPositions > 0.5f)
                {
                    break;
                }
            }

            // If a building, ignore the various positions (WHY?)
            float ratioSourcePosToTargetPos = (targetedBuildingGuid != null) ?
                sourcePositionsWithLineOfFireToTargetPositions : (sourcePositionsWithLineOfFireToTargetPositions / losTargetPositionsCount);

            // "MinRatioFromActors": 0.2,
            float b = ratioSourcePosToTargetPos - ___Combat.Constants.Visibility.MinRatioFromActors;
            float ratioDirectAttacksToTargetPositions = Mathf.Min(weaponsWithUnobstructedLOF / losTargetPositionsCount, b);
            if (ratioDirectAttacksToTargetPositions > 0.001f)
            {
                ratioSourcePosToTargetPos -= ratioDirectAttacksToTargetPositions;
            }

            //LowVisibility.Logger.Log($"LineOfSight:GetLineOfFireUncached:pre - ratio is:{ratioSourcePosToTargetPos} / direct:{ratioDirectAttacksToTargetPositions} / b:{b}");
            // "RatioFullVis": 0.79,
            // "RatioObstructedVis": 0.41,
            if (ratioSourcePosToTargetPos >= ___Combat.Constants.Visibility.RatioFullVis)
            {
                __result = LineOfFireLevel.LOFClear;
            }
            else if (ratioSourcePosToTargetPos >= ___Combat.Constants.Visibility.RatioObstructedVis)
            {
                __result = LineOfFireLevel.LOFObstructed;
            }
            else
            {
                __result = LineOfFireLevel.LOFBlocked;
            }

            Mod.Log.Trace?.Write($"LOS:GLOFU LOS result is:{__result}");

            __runOriginal = false;
        }
    }


    // TODO: Duplicate work - make prefix if necessary?
    [HarmonyPatch(typeof(FiringPreviewManager), "HasLOS", new Type[] { typeof(AbstractActor), typeof(ICombatant), typeof(Vector3), typeof(List<AbstractActor>) })]
    public static class FiringPreviewManager_HasLOS
    {
        public static void Postfix(FiringPreviewManager __instance, ref bool __result, CombatGameState ___combat,
            AbstractActor attacker, ICombatant target, Vector3 position, List<AbstractActor> allies)
        {
            //LowVisibility.Logger.Debug("FiringPreviewManager:HasLOS:post - entered.");
            for (int i = 0; i < allies.Count; i++)
            {
                //if (allies[i].VisibilityCache.VisibilityToTarget(target).VisibilityLevel == VisibilityLevel.LOSFull) {
                if (allies[i].VisibilityCache.VisibilityToTarget(target).VisibilityLevel >= VisibilityLevel.Blip0Minimum)
                {
                    __result = true;
                    Mod.Log.Trace?.Write($"Allied actor{CombatantUtils.Label(allies[i])} has LOS " +
                        $"to target:{CombatantUtils.Label(target as AbstractActor)}, returning true.");
                    return;
                }
            }

            VisibilityLevel visibilityToTargetWithPositionsAndRotations =
                ___combat.LOS.GetVisibilityToTargetWithPositionsAndRotations(attacker, position, target);
            //__result = visibilityToTargetWithPositionsAndRotations == VisibilityLevel.LOSFull;
            __result = visibilityToTargetWithPositionsAndRotations >= VisibilityLevel.Blip0Minimum;
            Mod.Log.Trace?.Write($"Actor{CombatantUtils.Label(attacker)} has LOS? {__result} " +
                $"to target:{CombatantUtils.Label(target as AbstractActor)}");
        }
    }

}
