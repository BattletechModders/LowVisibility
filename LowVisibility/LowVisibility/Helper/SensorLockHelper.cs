using BattleTech;
using LowVisibility.Object;
using UnityEngine;
using us.frostraptor.modUtils;

namespace LowVisibility.Helper {
    class SensorLockHelper {


        // WARNING: DUPLICATE OF HBS CODE. THIS IS LIKELY TO BREAK IF HBS CHANGES THE SOURCE FUNCTIONS
        public static float GetSensorsRange(AbstractActor source) {

            // Add multipliers and absolute bonuses

            EWState ewState = new EWState(source);

            Mod.Log.Trace($"  == Sensors Range for for actor:{CombatantUtils.Label(source)}");

            float rawRangeMulti = SensorLockHelper.GetAllSensorRangeMultipliers(source);
            float rangeMulti = rawRangeMulti + ewState.GetSensorsRangeMulti();
            Mod.Log.Trace($"    rangeMulti: {rangeMulti} = rawRangeMulti: {rawRangeMulti} + sensorCheckRangeMulti: {ewState.GetSensorsRangeMulti()}");

            float rawRangeMod = SensorLockHelper.GetAllSensorRangeAbsolutes(source);
            float rangeMod = rawRangeMod * (1 + ewState.GetSensorsRangeMulti());
            Mod.Log.Trace($"    rangeMod: {rangeMod} = rawRangeMod: {rawRangeMod} + sensorCheckRangeMulti: {ewState.GetSensorsRangeMulti()}");

            float sensorsRange = ewState.GetSensorsBaseRange() * rangeMulti + rangeMod;
            Mod.Log.Trace($"    sensorsRange: { sensorsRange} = baseRange: {ewState.GetSensorsBaseRange()} * rangeMult: {rangeMulti} + rangeMod: {rangeMod}");

            if (sensorsRange < Mod.Config.MinimumSensorRange() ||
                source.Combat.TurnDirector.CurrentRound <= 1 && Mod.Config.FirstTurnForceFailedChecks) {
                sensorsRange = Mod.Config.MinimumSensorRange();
            }

            return sensorsRange;
        }

        public static float GetAdjustedSensorRange(AbstractActor source, ICombatant target) {
            float sourceSensorRange = SensorLockHelper.GetSensorsRange(source);
            float targetSignature = SensorLockHelper.GetTargetSignature(target);
            //LowVisibility.Logger.Debug($"   source:{CombatantUtils.Label(source)} sensorRange:{sourceSensorRange}m vs targetSignature:x{targetSignature}");

            //if (target != null && source.VisibilityToTargetUnit(target) > VisibilityLevel.None) {
            //    // If is sensor lock, add the Hysterisis modifier
            //    signatureModifiedRange += ___Combat.Constants.Visibility.SensorHysteresisAdditive;
            //}

            float modifiedRange = sourceSensorRange * targetSignature;
   
            return modifiedRange;
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

            float shutdownMod = (!target.IsShutDown) ? 0f : target.Combat.Constants.Visibility.ShutDownSignatureModifier;
            float sensorMod = target.SensorSignatureModifier;

            EWState ewState = new EWState(target);
            float ecmShieldMod = ewState.GetECMShieldSignatureModifier();
            float sensorStealthMod = ewState.GetSensorStealthSignatureModifier();

            float targetSignature = sensorMod + shutdownMod + ecmShieldMod;
            Mod.Log.Trace($" Actor: {CombatantUtils.Label(target)} has signature: {targetSignature} = " +
                $"sensorSignature: {sensorMod} +  shutdown: {shutdownMod} + ecmShield: {ecmShieldMod} + sensorStealth: {sensorStealthMod}");

            return targetSignature;
        }

        public static SensorScanType CalculateSensorLock(AbstractActor source, Vector3 sourcePos, ICombatant target, Vector3 targetPos) {

            if (source.GUID == target.GUID || source.Combat.HostilityMatrix.IsFriendly(source.TeamId, target.team.GUID)) {
                // If they are us, or allied, automatically give sensor details
                Mod.Log.Trace($"  source:{CombatantUtils.Label(source)} is friendly to target:{CombatantUtils.Label(target)}. Forcing full visibility.");
                return SensorScanType.DentalRecords;
            }

            if (source.IsDead || source.IsFlaggedForDeath) {
                // If we're dead, we can't have vision or sensors. If we're off the map, we can't either. If the target is off the map, we can't see it.                
                Mod.Log.Debug($"  source:{CombatantUtils.Label(source)} is dead or dying. Forcing no visibility.");
                return SensorScanType.NoInfo;
            }

            if (target.IsDead || target.IsFlaggedForDeath) {
                // If the target is dead, we can't have sensor but we have vision 
                Mod.Log.Debug($"  target:{CombatantUtils.Label(target)} is dead or dying. Forcing no sensor lock, vision based upon visibility.");
                return SensorScanType.NoInfo;
            }

            if (source.IsTeleportedOffScreen) {
                Mod.Log.Debug($"  source is teleported off screen. Skipping.");
                return SensorScanType.NoInfo;
            }

            if (source.IsGhosted) {
                Mod.Log.Debug($"  source is ghosted. Treating as noInfo.");
                return SensorScanType.NoInfo;
            }

            float distance = Vector3.Distance(sourcePos, targetPos);
            float sensorRangeVsTarget = SensorLockHelper.GetAdjustedSensorRange(source, target);
            Mod.Log.Trace($"SensorLockHelper - source: {CombatantUtils.Label(source)} sensorRangeVsTarget: {sensorRangeVsTarget} vs distance: {distance}");
            if (distance > sensorRangeVsTarget) {
                // Check for Narc effect that will show the target regardless of range
                SensorScanType narcLock = HasNarcBeaconDetection(target) ? SensorScanType.Location : SensorScanType.NoInfo;
                Mod.Log.Trace($"  source:{CombatantUtils.Label(source)} is out of range, lock from Narc is:{narcLock}");
                return narcLock;
            } else if ((target as Building) != null) {
                // If the target is a building, show them so long as they are in sensor distance
                // TODO: ADD FRIENDLY ECM CHECK HERE?
                EWState sourceState = new EWState(source);
                Building targetBuilding = target as Building;
                // TODO: This should be calculated more fully! Major bug here!
                SensorScanType buildingLock = sourceState.GetCurrentEWCheck() > 0 ? SensorScanType.SurfaceScan: SensorScanType.NoInfo;
                Mod.Log.Debug($"  target:{CombatantUtils.Label(target)} is a building with lockState:{buildingLock}");
                return buildingLock;
            } else if ((target as AbstractActor) != null) {
                AbstractActor targetActor = target as AbstractActor;

                SensorScanType sensorLock = SensorScanType.NoInfo;
                if (targetActor.IsTeleportedOffScreen) {
                    Mod.Log.Debug($"  target is teleported off screen. Skipping.");                
                } else {
                    // TODO: Re-add shadowing logic
                    // TODO: SensorLock adds a boost from friendlies if they have shares sensors?
                    // We are within range, but check to see if the sensorInfoCheck failed
                    sensorLock = CalculateSensorInfoLevel(source, target);

                    // Check for Narc effect overriding detection
                    if (sensorLock < SensorScanType.Location && HasNarcBeaconDetection(targetActor)) {
                        sensorLock = SensorScanType.Location;
                    }
                }
                Mod.Log.Trace($"SensorLockHelper - source:{CombatantUtils.Label(source)} has sensorLock:({sensorLock}) vs " +
                    $"target:{CombatantUtils.Label(target)}");
                return sensorLock;
            } else {
                Mod.Log.Info($"SensorLockHelper - fallthrough case we don't know how to handle. Returning NoLock!");
                return SensorScanType.NoInfo;
            }
        }

        private static bool HasNarcBeaconDetection(ICombatant target) {
            bool hasDetection = false;
            if (target != null && State.NARCEffect(target) != 0) {
                int delta = State.NARCEffect(target) - State.ECMProtection(target);
                Mod.Log.Debug($"  target:{CombatantUtils.Label(target)} has an active " +
                    $"narc effect {State.NARCEffect(target)} vs. ECM Protection:{State.ECMProtection(target)}, delta is:{delta}");

                if (delta >= 1) {
                    Mod.Log.Debug($"  target:{CombatantUtils.Label(target)} has an active NARC effect, " +
                        $"marking them visible!");
                    hasDetection = true;
                }
            }
            return hasDetection;
        }

        private static SensorScanType CalculateSensorInfoLevel(AbstractActor source, ICombatant target) {
            SensorScanType sensorInfo = SensorScanType.NoInfo;
            Mod.Log.Debug($"Calculating SensorInfo from source: ({CombatantUtils.Label(source)}) to target: ({CombatantUtils.Label(target)})");

            // Determine modified check against target
            EWState sourceState = new EWState(source);
            int detailsLevel = sourceState.GetCurrentEWCheck();

            // --- Source: Advanced Sensors
            if (sourceState.GetAdvancedSensorsMod() > 0) {
                Mod.Log.Debug($" == source has advanced sensors, detailsLevel = {detailsLevel} + {sourceState.GetAdvancedSensorsMod()}");
                detailsLevel += sourceState.GetAdvancedSensorsMod();
            }

            // --- Source: ECM Jamming
            if (sourceState.GetECMJammedDetailsModifier() > 0) {
                Mod.Log.Debug($" == source is jammed by ECM, detailsLevel = {detailsLevel} - {sourceState.GetECMJammedDetailsModifier()}");
                detailsLevel -= sourceState.GetECMJammedDetailsModifier();
            }

            // --- Target: Stealth, Narc, Tag
            AbstractActor targetActor = target as AbstractActor;
            if (targetActor != null) {
                EWState targetState = new EWState(targetActor);

                // ECM Shield reduces sensor info
                if (targetState.GetECMShieldSignatureModifier() > 0) {
                    Mod.Log.Debug($" == target is shielded by ECM, detailsLevel = {detailsLevel} - {sourceState.GetECMShieldDetailsModifier()}");
                    detailsLevel -= sourceState.GetECMShieldDetailsModifier();
                }

                // Sensor Stealth reduces sensor info
                if (targetState.HasSensorStealth()) {
                    Mod.Log.Debug($" == target has sensor stealth, detailsLevel = {detailsLevel} - {sourceState.GetSensorStealthDetailsModifier()}");
                    detailsLevel -= targetState.GetSensorStealthDetailsModifier();
                }

                // A Narc effect increases sensor info
                // TODO: Narc should effect buildings
                if (State.NARCEffect(targetActor) != 0) {
                    Mod.Log.Debug($" == target is narc'd, detailsLevel = {detailsLevel} - {State.NARCEffect(targetActor)}");
                    detailsLevel += State.NARCEffect(targetActor);
                }

                // A TAG effect increases sensor info
                // TODO: TAG should effect buildings
                if (State.TAGEffect(targetActor) != 0) {
                    Mod.Log.Debug($" == target is tagged, detailsLevel = {detailsLevel} + {State.TAGEffect(targetActor)}");
                    detailsLevel += State.TAGEffect(targetActor);
                }

            }

            sensorInfo = DetectionLevelForCheck(detailsLevel);
            Mod.Log.Debug($"Calculated )");

            return sensorInfo;
        }

        public static SensorScanType DetectionLevelForCheck(int checkResult) {
            SensorScanType level = SensorScanType.NoInfo;
            if (checkResult == 0) {
                level = SensorScanType.Location;
            } else if (checkResult == 1) {
                level = SensorScanType.Type;
            } else if (checkResult == 2) {
                level = SensorScanType.Silhouette;
            } else if (checkResult == 3) {
                level = SensorScanType.Vector;
            } else if (checkResult == 4 || checkResult == 5) {
                level = SensorScanType.SurfaceScan;
            } else if (checkResult == 6 || checkResult == 7) {
                level = SensorScanType.SurfaceAnalysis;
            } else if (checkResult == 8) {
                level = SensorScanType.WeaponAnalysis;
            } else if (checkResult == 9) {
                level = SensorScanType.StructureAnalysis;
            } else if (checkResult == 10) {
                level = SensorScanType.DeepScan;
            } else if (checkResult >= 11) {
                level = SensorScanType.DentalRecords;
            }
            return level;
        }
    }
}
