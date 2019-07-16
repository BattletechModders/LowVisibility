using BattleTech;
using LowVisibility.Object;
using UnityEngine;
using us.frostraptor.modUtils;
using us.frostraptor.modUtils.math;

namespace LowVisibility.Helper {
    class VisualLockHelper {

        // WARNING: DUPLICATE OF HBS CODE. THIS IS LIKELY TO BREAK IF HBS CHANGES THE SOURCE FUNCTIONS
        public static float GetSpotterRange(AbstractActor source) {
            return GetVisualRange(State.GetMapVisionRange(), source);
        }

        public static float GetVisualLockRange(AbstractActor source) {
            return GetVisualRange(State.GetMapVisionRange(), source);
        }

        public static float GetVisualScanRange(AbstractActor source) {
            return GetVisualRange(State.GetVisualIDRange(), source);
        }

        private static float GetVisualRange(float visionRange, AbstractActor source) {
            float visualRange = visionRange;
            if (source.IsShutDown) {
                visualRange = visionRange * source.Combat.Constants.Visibility.ShutdownSpottingDistanceMultiplier;
            } else if (source.IsProne) {
                visualRange = visionRange * source.Combat.Constants.Visibility.ProneSpottingDistanceMultiplier;
            } else {
                float multipliers = VisualLockHelper.GetAllSpotterMultipliers(source);
                float absolutes = VisualLockHelper.GetAllSpotterAbsolutes(source);
                
                visualRange = visionRange * multipliers + absolutes;
                Mod.Log.Trace($" -- source:{CombatantUtils.Label(source)} has spotting " +
                    $"multi:x{multipliers} absolutes:{absolutes} visionRange:{visionRange}");
            }

            if (visualRange < Mod.Config.MinimumVisionRange()) {
                visualRange = Mod.Config.MinimumVisionRange();
            }

            // Round up to the nearest full hex
            float normalizedRange = HexUtils.CountHexes(visualRange, false) * 30f;
            
            //LowVisibility.Logger.Trace($" -- source:{CombatantUtils.Label(source)} visual range is:{normalizedRange}m normalized from:{visualRange}m");
            return normalizedRange;
        }

        // WARNING: DUPLICATE OF HBS CODE. THIS IS LIKELY TO BREAK IF HBS CHANGES THE SOURCE FUNCTIONS
        public static float GetAdjustedSpotterRange(AbstractActor source, ICombatant target) {

            float targetVisibility = 1f;
            AbstractActor abstractActor = target as AbstractActor;
            if (abstractActor != null) {
                targetVisibility = VisualLockHelper.GetTargetVisibility(abstractActor);
            }

            float spotterRange = VisualLockHelper.GetSpotterRange(source);

            float modifiedRange = spotterRange * targetVisibility;
            if (modifiedRange < Mod.Config.MinimumVisionRange()) {
                modifiedRange = Mod.Config.MinimumVisionRange();
            }

            // Round up to the nearest full hex
            float normalizedRange = HexUtils.CountHexes(spotterRange, true) * 30f;

            //LowVisibility.Logger.Trace($" -- source:{CombatantUtils.Label(source)} adjusted spotterRange:{normalizedRange}m normalized from:{spotterRange}m");
            return normalizedRange;
        }

        // WARNING: DUPLICATE OF HBS CODE. THIS IS LIKELY TO BREAK IF HBS CHANGES THE SOURCE FUNCTIONS
        public static float GetAllSpotterMultipliers(AbstractActor source) {
            return source == null ? 1f : source.SpotterDistanceMultiplier;
        }

        // WARNING: DUPLICATE OF HBS CODE. THIS IS LIKELY TO BREAK IF HBS CHANGES THE SOURCE FUNCTIONS
        public static float GetAllSpotterAbsolutes(AbstractActor source) {
            float absoluteModifier = 0f;

            if (source != null) {
                float pilotSkillMod = 0f;
                if (source.IsPilotable) {
                    Pilot pilot = source.GetPilot();
                    if (pilot != null) {
                        EWState parentState = new EWState(source);
                        pilotSkillMod = parentState.GetTacticsVisionBoost();
                    }
                }
                absoluteModifier = pilotSkillMod + source.SpotterDistanceAbsolute;
            }

            return absoluteModifier;
        }

        // WARNING: DUPLICATE OF HBS CODE. THIS IS LIKELY TO BREAK IF HBS CHANGES THE SOURCE FUNCTIONS
        public static float GetTargetVisibility(AbstractActor target) {
            if (target == null) { return 1f; }

            float allTargetVisibilityMultipliers = GetAllTargetVisibilityMultipliers(target);
            float allTargetVisibilityAbsolutes = GetAllTargetVisibilityAbsolutes(target);

            return 1f * allTargetVisibilityMultipliers + allTargetVisibilityAbsolutes;
        }

        // WARNING: DUPLICATE OF HBS CODE. THIS IS LIKELY TO BREAK IF HBS CHANGES THE SOURCE FUNCTIONS
        private static float GetAllTargetVisibilityMultipliers(AbstractActor target) {
            if (target == null) { return 1f; }

            float baseVisMulti = 0f;
            float shutdownVisMulti = (!target.IsShutDown) ? 0f : target.Combat.Constants.Visibility.ShutDownVisibilityModifier;        
            float spottingVisibilityMultiplier = target.SpottingVisibilityMultiplier;

            EWState ewState = new EWState(target);
            float visionStealthMod = ewState.GetVisionStealthVisibilityModifier();

            float targetVisibility = baseVisMulti + shutdownVisMulti + spottingVisibilityMultiplier + visionStealthMod;
            Mod.Log.Trace($" Actor: {CombatantUtils.Label(target)} has visibility: {targetVisibility} = " +
                $"baseVisMulti: {baseVisMulti} +  shutdownVisMulti: {shutdownVisMulti} + spottingVisibilityMultiplier: {spottingVisibilityMultiplier} + visionStealthMod: {visionStealthMod}");

            return targetVisibility;
            //return baseVisMulti + shutdownVisMulti + spottingVisibilityMultiplier;
        }

        // WARNING: DUPLICATE OF HBS CODE. THIS IS LIKELY TO BREAK IF HBS CHANGES THE SOURCE FUNCTIONS
        private static float GetAllTargetVisibilityAbsolutes(AbstractActor target) {
            if (target == null) { return 0f; }

            float baseVisMod = 0f;
            float spottingVisibilityAbsolute = target.SpottingVisibilityAbsolute;

            return baseVisMod + spottingVisibilityAbsolute;
        }


        // Determines if a source has visual lock to a target from a given position. Because units have differnet positions, check all of them.
        //  Typically from head-to-head for mechs, but buildings have multiple positions.
        // TODO: Refactor to eliminate need for LoS instance - put Getter funcs into a helper
        public static VisualScanType CalculateVisualLock(AbstractActor source, Vector3 sourcePos,
                ICombatant target, Vector3 targetPos, Quaternion targetRot, LineOfSight los) {

            float spottingRangeVsTarget = VisualLockHelper.GetAdjustedSpotterRange(source, target);
            float visualScanRange = VisualLockHelper.GetVisualScanRange(source);
            float distance = Vector3.Distance(sourcePos, targetPos);

            // Check range first
            if (distance > spottingRangeVsTarget) {
                return VisualScanType.None;
            }

            // TODO: VisualScanType needs to account for visual stealth

            // I think this is what prevents you from seeing things from behind you - the rotation is set to 0?
            Vector3 forward = targetPos - sourcePos;
            forward.y = 0f;
            Quaternion rotation = Quaternion.LookRotation(forward);

            VisualScanType visualLock = VisualScanType.None;
            if (distance <= spottingRangeVsTarget) {
                Vector3[] lossourcePositions = source.GetLOSSourcePositions(sourcePos, rotation);
                Vector3[] lostargetPositions = target.GetLOSTargetPositions(targetPos, targetRot);
                for (int i = 0; i < lossourcePositions.Length; i++) {
                    for (int j = 0; j < lostargetPositions.Length; j++) {
                        // If you can visually spot the target, you immediately have detection on then
                        if (los.HasLineOfSight(lossourcePositions[i], lostargetPositions[j], spottingRangeVsTarget, target.GUID)) {
                            visualLock = distance <= visualScanRange ? VisualScanType.VisualID : VisualScanType.Silhouette;
                            break;
                        }
                    }

                    if (visualLock != VisualScanType.None) {
                        break;
                    }
                }
            }

            return visualLock;
        }
    }
}
