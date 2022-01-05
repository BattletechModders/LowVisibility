using BattleTech;
using LowVisibility.Object;
using UnityEngine;
using us.frostraptor.modUtils;
using us.frostraptor.modUtils.math;

namespace LowVisibility.Helper {
    public static class VisualLockHelper {

        // WARNING: DUPLICATE OF HBS CODE. THIS IS LIKELY TO BREAK IF HBS CHANGES THE SOURCE FUNCTIONS
        public static float GetSpotterRange(AbstractActor source) {
            // FIXME: Dirty hack here. Assuming that night vision mode only comes on during a unit's turn / selection, then goes away
            float visRange = ModState.IsNightVisionMode ? 
                ModState.GetMapConfig().nightVisionSpotterRange : ModState.GetMapConfig().spotterRange;
            return GetVisualRange(visRange, source);
        }

        public static float GetVisualLockRange(AbstractActor source) {
            // FIXME: Dirty hack here. Assuming that night vision mode only comes on during a unit's turn / selection, then goes away
            float visRange = ModState.IsNightVisionMode ? ModState.GetMapConfig().nightVisionSpotterRange : ModState.GetMapConfig().spotterRange;
            return GetVisualRange(visRange, source);
        }

        public static float GetVisualScanRange(AbstractActor source) {
            // FIXME: Dirty hack here. Assuming that night vision mode only comes on during a unit's turn / selection, then goes away
            float visRange = ModState.IsNightVisionMode ? ModState.GetMapConfig().nightVisionVisualIDRange : ModState.GetMapConfig().visualIDRange;
            return GetVisualRange(visRange, source);
        }

        private static float GetVisualRange(float visionRange, AbstractActor source) {
            float visualRange;
            if (source.IsShutDown) {
                visualRange = visionRange * source.Combat.Constants.Visibility.ShutdownSpottingDistanceMultiplier;
            } else if (source.IsProne) {
                visualRange = visionRange * source.Combat.Constants.Visibility.ProneSpottingDistanceMultiplier;
            } else {
                float multipliers = VisualLockHelper.GetAllSpotterMultipliers(source);
                float absolutes = VisualLockHelper.GetAllSpotterAbsolutes(source);
                
                visualRange = visionRange * multipliers + absolutes;
                //Mod.Log.Trace?.Write($" -- source:{CombatantUtils.Label(source)} has spotting " +
                //    $"multi:x{multipliers} absolutes:{absolutes} visionRange:{visionRange}");
            }

            if (visualRange < Mod.Config.Vision.MinimumRange) {
                visualRange = Mod.Config.Vision.MinimumRange;
            }
            
            //LowVisibility.Logger.Trace($" -- source:{CombatantUtils.Label(source)} visual range is:{normalizedRange}m normalized from:{visualRange}m");
            return visualRange;
        }

        // WARNING: DUPLICATE OF HBS CODE. THIS IS LIKELY TO BREAK IF HBS CHANGES THE SOURCE FUNCTIONS
        public static float GetAdjustedSpotterRange(AbstractActor source, ICombatant target) {

            float targetVisibility = 1f;
            AbstractActor targetActor = target as AbstractActor;
            if (targetActor != null) {
                EWState sourceState = source.GetEWState();
                targetVisibility = VisualLockHelper.GetTargetVisibility(targetActor, sourceState);
            }

            float spotterRange = VisualLockHelper.GetSpotterRange(source);

            float modifiedRange = spotterRange * targetVisibility;
            if (modifiedRange < Mod.Config.Vision.MinimumRange) {
                modifiedRange = Mod.Config.Vision.MinimumRange;
            }

            Mod.Log.Trace?.Write($" -- source:{CombatantUtils.Label(source)} adjusted spotterRange: {modifiedRange}m");
            return modifiedRange;
        }

        // WARNING: DUPLICATE OF HBS CODE. THIS IS LIKELY TO BREAK IF HBS CHANGES THE SOURCE FUNCTIONS
        public static float GetAllSpotterMultipliers(AbstractActor source) {
            return source == null ? 1f : source.SpotterDistanceMultiplier;
        }

        // WARNING: DUPLICATE OF HBS CODE. THIS IS LIKELY TO BREAK IF HBS CHANGES THE SOURCE FUNCTIONS
        public static float GetAllSpotterAbsolutes(AbstractActor source) {

            // Intentionally don't allow tactics to influence spotting range. Tactics gives enough other
            //   benefits, no need to add it here.

            //float absoluteModifier = 0f;

            //if (source != null) {
            //    float pilotSkillMod = 0f;
            //    if (source.IsPilotable) {
            //        Pilot pilot = source.GetPilot();
            //        if (pilot != null) {
            //            EWState parentState = new EWState(source);
            //            pilotSkillMod = parentState.GetTacticsVisionBoost();
            //        }
            //    }
            //    absoluteModifier = pilotSkillMod + source.SpotterDistanceAbsolute;
            //}

            return source.SpotterDistanceAbsolute;
        }

        // WARNING: DUPLICATE OF HBS CODE. THIS IS LIKELY TO BREAK IF HBS CHANGES THE SOURCE FUNCTIONS
        public static float GetTargetVisibility(AbstractActor target, EWState sourceState) {
            if (target == null) { return 1f; }

            float allTargetVisibilityMultipliers = GetAllTargetVisibilityMultipliers(target, sourceState);
            float allTargetVisibilityAbsolutes = GetAllTargetVisibilityAbsolutes(target);

            return 1f * allTargetVisibilityMultipliers + allTargetVisibilityAbsolutes;
        }

        // WARNING: DUPLICATE OF HBS CODE. THIS IS LIKELY TO BREAK IF HBS CHANGES THE SOURCE FUNCTIONS
        private static float GetAllTargetVisibilityMultipliers(AbstractActor target, EWState sourceState) {
            if (target == null) { return 1f; }

            float baseVisMulti = 1f;
            float shutdownVisMulti = (!target.IsShutDown) ? 1f : target.Combat.Constants.Visibility.ShutDownVisibilityModifier;        
            float spottingVisibilityMultiplier = target.SpottingVisibilityMultiplier;

            EWState ewState = target.GetEWState();
            float mimeticMod = ewState.MimeticVisibilityMod(sourceState);

            float targetVisibility = baseVisMulti * shutdownVisMulti * spottingVisibilityMultiplier * mimeticMod;
            Mod.Log.Trace?.Write($" Actor: {CombatantUtils.Label(target)} has visibility: {targetVisibility} = " +
                $"baseVisMulti: {baseVisMulti} * shutdownVisMulti: {shutdownVisMulti} * spottingVisibilityMultiplier: {spottingVisibilityMultiplier} * visionStealthMod: {mimeticMod}");

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
        public static bool CanSpotTarget(AbstractActor source, Vector3 sourcePos,
                ICombatant target, Vector3 targetPos, Quaternion targetRot, LineOfSight los) {

            float spottingRangeVsTarget = VisualLockHelper.GetAdjustedSpotterRange(source, target);
            float distance = Vector3.Distance(sourcePos, targetPos);
            //Mod.Log.Info?.Write($" COMPARING SPOTTING_RANGE: {spottingRangeVsTarget} VS DISTANCE: {distance}");

            // Check range first
            if (distance > spottingRangeVsTarget) {
                return false;
            }

            // I think this is what prevents you from seeing things from behind you - the rotation is set to 0?
            Vector3 forward = targetPos - sourcePos;
            forward.y = 0f;
            //Quaternion rotation = Quaternion.LookRotation(forward);
            Quaternion rotation = (forward != Vector3.zero) ? Quaternion.LookRotation(forward) : source.CurrentRotation;

            if (distance <= spottingRangeVsTarget) {
                Vector3[] lossourcePositions = source.GetLOSSourcePositions(sourcePos, rotation);
                Vector3[] lostargetPositions = target.GetLOSTargetPositions(targetPos, targetRot);
                for (int i = 0; i < lossourcePositions.Length; i++) {
                    for (int j = 0; j < lostargetPositions.Length; j++) {                        
                        if (los.HasLineOfSight(lossourcePositions[i], lostargetPositions[j], spottingRangeVsTarget, target.GUID)) {
                            return true;
                        }
                    }
                }
            }

            return false;
        }

        public static bool CanSpotTargetUsingCurrentPositions(AbstractActor source, ICombatant target)
        {
            if (source == null || target == null) return false;

            return CanSpotTarget(source, source.CurrentPosition, target, target.CurrentPosition, target.CurrentRotation, source.Combat.LOS);
        }
    }
}
