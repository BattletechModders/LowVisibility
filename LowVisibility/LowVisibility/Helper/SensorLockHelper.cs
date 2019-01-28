using BattleTech;
using LowVisibility.Object;
using UnityEngine;

namespace LowVisibility.Helper {
    class SensorLockHelper {


        // WARNING: DUPLICATE OF HBS CODE. THIS IS LIKELY TO BREAK IF HBS CHANGES THE SOURCE FUNCTIONS
        public static float GetSensorsRange(AbstractActor source) {

            // Add multipliers and absolute bonuses
            float rangeMulti = SensorLockHelper.GetAllSensorRangeMultipliers(source);
            float rangeMod = SensorLockHelper.GetAllSensorRangeAbsolutes(source);

            EWState ewState = State.GetEWState(source);

            float sensorsRange = (ewState.sensorsBaseRange * rangeMulti + rangeMod) * ewState.SensorCheckMultiplier();

            if (sensorsRange < LowVisibility.Config.MinimumSensorRange()) {
                sensorsRange = LowVisibility.Config.MinimumSensorRange();
            }

            // Round up to the nearest full hex
            float normalizedRange = MathHelper.CountHexes(sensorsRange, true) * 30f;

            LowVisibility.Logger.LogIfTrace($" -- source:{CombatantHelper.Label(source)} sensorsRange:{normalizedRange}m normalized from:{sensorsRange}m");
            return normalizedRange;
        }

        public static float GetAdjustedSensorRange(AbstractActor source, ICombatant target) {
            float sourceSensorRange = SensorLockHelper.GetSensorsRange(source);
            float targetSignature = SensorLockHelper.GetTargetSignature(target);

            //if (target != null && source.VisibilityToTargetUnit(target) > VisibilityLevel.None) {
            //    // If is sensor lock, add the Hysterisis modifier
            //    signatureModifiedRange += ___Combat.Constants.Visibility.SensorHysteresisAdditive;
            //}

            float modifiedRange = sourceSensorRange * targetSignature;
            if (modifiedRange < LowVisibility.Config.MinimumSensorRange()) {
                modifiedRange = LowVisibility.Config.MinimumSensorRange();
            }

            // Round up to the nearest full hex
            float normalizedRange = MathHelper.CountHexes(modifiedRange, true) * 30f;

            LowVisibility.Logger.LogIfTrace($" -- source:{CombatantHelper.Label(source)} adjusted sensorRange:{normalizedRange}m normalized from:{modifiedRange}m");
            return normalizedRange;
        }

        // WARNING: DUPLICATE OF HBS CODE. THIS IS LIKELY TO BREAK IF HBS CHANGES THE SOURCE FUNCTIONS
        private static float GetAllSensorRangeMultipliers(AbstractActor source) {
            if (source == null) {
                return 1f;
            }
            float sensorMulti = source.SensorDistanceMultiplier;

            DesignMaskDef occupiedDesignMask = source.occupiedDesignMask;
            if (occupiedDesignMask != null) { sensorMulti *= occupiedDesignMask.sensorRangeMultiplier; }

            return sensorMulti;
        }

        // WARNING: DUPLICATE OF HBS CODE. THIS IS LIKELY TO BREAK IF HBS CHANGES THE SOURCE FUNCTIONS
        private static float GetAllSensorRangeAbsolutes(AbstractActor source) {
            return source == null ? 0f : source.SensorDistanceAbsolute;
        }

        // WARNING: DUPLICATE OF HBS CODE. THIS IS LIKELY TO BREAK IF HBS CHANGES THE SOURCE FUNCTIONS
        public static float GetTargetSignature(ICombatant target) {
            if (target == null || (target as AbstractActor) == null) { return 1f; }

            AbstractActor targetActor = target as AbstractActor;
            float allTargetSignatureModifiers = GetAllTargetSignatureModifiers(targetActor);
            float staticSignature = 1f + allTargetSignatureModifiers;

            // Add in any design mask boosts
            DesignMaskDef occupiedDesignMask = targetActor.occupiedDesignMask;
            if (occupiedDesignMask != null) { staticSignature *= occupiedDesignMask.signatureMultiplier; }
            if (staticSignature < 0.01f) { staticSignature = 0.01f; }

            return staticSignature;
        }

        // WARNING: DUPLICATE OF HBS CODE. THIS IS LIKELY TO BREAK IF HBS CHANGES THE SOURCE FUNCTIONS
        private static float GetAllTargetSignatureModifiers(AbstractActor target) {
            if (target == null) { return 0f; }

            float shutdownSignatureMod = (!target.IsShutDown) ? 0f : target.Combat.Constants.Visibility.ShutDownSignatureModifier;
            float sensorSignatureModifier = target.SensorSignatureModifier;

            return shutdownSignatureMod + sensorSignatureModifier;
        }

        public static SensorLockType CalculateSensorLock(AbstractActor source, Vector3 sourcePos, ICombatant target, Vector3 targetPos) {
            
            float distance = Vector3.Distance(sourcePos, targetPos);
            float sensorRangeVsTarget = SensorLockHelper.GetAdjustedSensorRange(source, target);
            LowVisibility.Logger.LogIfDebug($"SensorLockHelper - source:{CombatantHelper.Label(source)} sensorRangeVsTarget:{sensorRangeVsTarget} vs distance:{distance}");

            SensorLockType sensorLock = SensorLockType.NoInfo;
            EWState sourceState = State.GetEWState(source);
            AbstractActor targetActor = target as AbstractActor;
            if (distance > sensorRangeVsTarget) {
                // Check for Narc effect that will show the target regardless of range
                sensorLock = HasNarcBeaconDetection(targetActor) ? SensorLockType.Location : SensorLockType.NoInfo;
            } else if (targetActor == null && (target as Building) != null) {
                // If the target is a building, show them regardless of sensor distance
                // TODO: ADD FRIENDLY ECM CHECK HERE?
                sensorLock = sourceState.detailCheck > 0 ? SensorLockType.SurfaceScan: SensorLockType.NoInfo;
            } else if (targetActor != null) {
                // TODO: Re-add shadowing logic
                // TODO: SensorLock adds a boost from friendlies if they have shares sensors?
                // We are within range, but check to see if the sensorInfoCheck failed
                sensorLock = CalculateSensorInfoLevel(source, target);                

                // Check for Narc effect overriding detection
                if (sensorLock < SensorLockType.Location && HasNarcBeaconDetection(targetActor)) {
                    sensorLock = SensorLockType.Location;
                }
            }

            LowVisibility.Logger.LogIfDebug($"SensorLockHelper - source:{CombatantHelper.Label(source)} has sensorLock:({sensorLock}) vs " +
                $"target:{CombatantHelper.Label(target)}");
    
            return sensorLock;
        }

        private static bool HasNarcBeaconDetection(AbstractActor target) {
            bool hasDetection = false;
            if (target != null && State.NARCEffect(target) != 0) {
                int delta = State.NARCEffect(target) - State.ECMProtection(target);
                LowVisibility.Logger.LogIfDebug($"GVTTWPAR - target:{CombatantHelper.Label(target)} has an active " +
                    $"narc effect {State.NARCEffect(target)} vs. ECM Protection:{State.ECMProtection(target)}, delta is:{delta}");

                if (delta >= 1) {
                    LowVisibility.Logger.LogIfDebug($"GVTTWPAR - target:{CombatantHelper.Label(target)} has an active NARC effect, " +
                        $"marking them visible!");
                    hasDetection = true;
                }
            }
            return hasDetection;
        }

        private static SensorLockType CalculateSensorInfoLevel(AbstractActor source, ICombatant target) {
            SensorLockType sensorInfo = SensorLockType.NoInfo;

            AbstractActor targetActor = target as AbstractActor;
            if (source.IsDead || source.IsFlaggedForDeath) {
                // If we're dead, we can't have vision or sensors. If we're off the map, we can't either. If the target is off the map, we can't see it.                
                LowVisibility.Logger.Log($"  -- source:{CombatantHelper.Label(source)} is dead or dying. Forcing no visibility.");
                return SensorLockType.NoInfo;
            } else if (source.IsTeleportedOffScreen || targetActor != null && targetActor.IsTeleportedOffScreen) {
                LowVisibility.Logger.Log($"  -- source or target is teleported off screen. Skipping.");
                return SensorLockType.NoInfo;
            } else if (target.IsDead || target.IsFlaggedForDeath) {
                // If the target is dead, we can't have sensor but we have vision 
                LowVisibility.Logger.Log($"  -- target:{CombatantHelper.Label(target)} is dead or dying. Forcing no sensor lock, vision based upon visibility.");
                return SensorLockType.NoInfo;
            } else if (source.GUID == target.GUID || source.Combat.HostilityMatrix.IsFriendly(source.TeamId, target.team.GUID)) {
                // If they are us, or allied, automatically give sensor details
                LowVisibility.Logger.Log($"  -- source:{CombatantHelper.Label(source)} is friendly to target:{CombatantHelper.Label(target)}. Forcing full visibility.");
                return SensorLockType.DentalRecords;
            }

            // Determine modified check against target
            EWState sourceEWState = State.GetEWState(source);
            int baseSourceCheck = sourceEWState.detailCheck + sourceEWState.SensorCheckModifier();
            int modifiedSourceCheck = baseSourceCheck;

            // --- Source modifier: ECM Jamming
            if (State.ECMJamming(source) != 0) {
                modifiedSourceCheck -= State.ECMJamming(source);
                LowVisibility.Logger.Log($"  -- source:{CombatantHelper.Label(source)} is jammed with strength:{State.ECMJamming(source)}, " +
                    $"reducing sourceCheckResult to:{modifiedSourceCheck}");
            }

            // --- Target Modifiers: Stealth, Narc, Tag
            if (targetActor != null) {
                EWState targetStaticState = State.GetEWState(targetActor);

                // ECM protection reduces sensor info
                if (State.ECMProtection(targetActor) != 0) {
                    modifiedSourceCheck -= State.ECMProtection(targetActor);
                    LowVisibility.Logger.Log($"  -- target:{CombatantHelper.Label(target)} has ECM protection with strength:{State.ECMProtection(targetActor)}, " +
                        $"reducing sourceCheckResult to:{modifiedSourceCheck}");
                }

                // A Stealth reduces sensor info
                if (targetStaticState.stealthMod != 0) {
                    modifiedSourceCheck -= targetStaticState.stealthMod;
                    LowVisibility.Logger.Log($"  -- target:{CombatantHelper.Label(target)} has stealthMod:{targetStaticState.stealthMod}, " +
                        $"reducing sourceCheckResult to:{modifiedSourceCheck}");
                }

                // A scrambler reduces sensor info
                if (targetStaticState.scramblerMod != 0) {
                    modifiedSourceCheck -= targetStaticState.scramblerMod;
                    LowVisibility.Logger.Log($"  -- target:{CombatantHelper.Label(target)} has scramblerMod:{targetStaticState.scramblerMod}, " +
                        $"reducing sourceCheckResult to:{modifiedSourceCheck}");
                }

                // A Narc effect increases sensor info
                if (State.NARCEffect(targetActor) != 0) {
                    modifiedSourceCheck += State.NARCEffect(targetActor);
                    LowVisibility.Logger.Log($"  -- target:{CombatantHelper.Label(target)} has NARC effect:{State.NARCEffect(targetActor)}, " +
                        $"increasing sourceCheckResult to:{modifiedSourceCheck}");
                }

                // A TAG effect increases sensor info
                if (State.TAGEffect(targetActor) != 0) {
                    modifiedSourceCheck += State.TAGEffect(targetActor);
                    LowVisibility.Logger.Log($"  -- target:{CombatantHelper.Label(target)} has TAG effect:{State.TAGEffect(targetActor)}, " +
                        $"increasing sourceCheckResult to:{modifiedSourceCheck}");
                }

            }

            sensorInfo = DetectionLevelForCheck(modifiedSourceCheck);
            LowVisibility.Logger.LogIfDebug($"Calculated sensorInfo:{sensorInfo} for modifiedCheck:{modifiedSourceCheck} (from baseCheck:{baseSourceCheck}");

            return sensorInfo;
        }

        public static SensorLockType DetectionLevelForCheck(int checkResult) {
            SensorLockType level = SensorLockType.NoInfo;
            if (checkResult == 0) {
                level = SensorLockType.Location;
            } else if (checkResult == 1) {
                level = SensorLockType.Type;
            } else if (checkResult == 2) {
                level = SensorLockType.Silhouette;
            } else if (checkResult == 3) {
                level = SensorLockType.Vector;
            } else if (checkResult == 4 || checkResult == 5) {
                level = SensorLockType.SurfaceScan;
            } else if (checkResult == 6 || checkResult == 7) {
                level = SensorLockType.SurfaceAnalysis;
            } else if (checkResult == 8) {
                level = SensorLockType.WeaponAnalysis;
            } else if (checkResult == 9) {
                level = SensorLockType.StructureAnalysis;
            } else if (checkResult == 10) {
                level = SensorLockType.DeepScan;
            } else if (checkResult >= 11) {
                level = SensorLockType.DentalRecords;
            }
            return level;
        }
    }
}
