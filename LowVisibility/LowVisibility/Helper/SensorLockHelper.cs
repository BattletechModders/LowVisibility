using BattleTech;
using LowVisibility.Object;
using System.Collections.Generic;
using UnityEngine;
using us.frostraptor.modUtils;

namespace LowVisibility.Helper {
    class SensorLockHelper {

        // WARNING: DUPLICATE OF HBS CODE. THIS IS LIKELY TO BREAK IF HBS CHANGES THE SOURCE FUNCTIONS
        public static float GetSensorsRange(AbstractActor source) {

            if (source.StatCollection.ContainsStatistic(ModStats.DisableSensors) && source.StatCollection.GetValue<bool>(ModStats.DisableSensors))
            {
                Mod.Log.Trace($"Returning minimum sensors range for {CombatantUtils.Label(source)} due to disabled sensors.");
                return Mod.Config.Sensors.MinimumSensorRange();
            }

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

            if (sensorsRange < Mod.Config.Sensors.MinimumSensorRange()) sensorsRange = Mod.Config.Sensors.MinimumSensorRange();

            return sensorsRange;
        }

        public static float GetAdjustedSensorRange(AbstractActor source, ICombatant target) {
            EWState sourceState = new EWState(source);
            float sourceSensorRange = SensorLockHelper.GetSensorsRange(source);
            float targetSignature = SensorLockHelper.GetTargetSignature(target, sourceState);
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
        public static float GetTargetSignature(ICombatant target, EWState sourceState) {
            if (target == null || (target as AbstractActor) == null) { return 1f; }

            AbstractActor targetActor = target as AbstractActor;
            float allTargetSignatureModifiers = GetAllTargetSignatureModifiers(targetActor, sourceState);
            float staticSignature = 1f + allTargetSignatureModifiers;

            // Add in any design mask boosts
            DesignMaskDef occupiedDesignMask = targetActor.occupiedDesignMask;
            if (occupiedDesignMask != null) { staticSignature *= occupiedDesignMask.signatureMultiplier; }
            if (staticSignature < 0.01f) { staticSignature = 0.01f; }

            return staticSignature;
        }

        // WARNING: DUPLICATE OF HBS CODE. THIS IS LIKELY TO BREAK IF HBS CHANGES THE SOURCE FUNCTIONS
        private static float GetAllTargetSignatureModifiers(AbstractActor target, EWState sourceState) {
            if (target == null) { return 0f; }

            float shutdownMod = (!target.IsShutDown) ? 0f : target.Combat.Constants.Visibility.ShutDownSignatureModifier;
            float rawSignature = target.SensorSignatureModifier;

            EWState ewState = new EWState(target);
            float ecmShieldMod = ewState.ECMSignatureMod(sourceState);
            float stealthMod = ewState.StealthSignatureMod(sourceState);
            float narcMod = ewState.NarcSignatureMod(sourceState);
            float tagMod = ewState.TagSignatureMod(sourceState);

            float targetSignature = rawSignature + shutdownMod + ecmShieldMod + narcMod + tagMod;
            Mod.Log.Trace($" Actor: {CombatantUtils.Label(target)} has signature: {targetSignature} = " +
                $"rawSignature: {rawSignature} +  shutdown: {shutdownMod} + ecmShield: {ecmShieldMod} " +
                $"+ stealth: {stealthMod} + narc: {narcMod} + tag: {tagMod}");

            return targetSignature;
        }

        // Iterates over all player-allied units, checks for highest lock level to determine unit 
        public static SensorScanType CalculateSharedLock(ICombatant target, AbstractActor source) {
            SensorScanType scanType = SensorScanType.NoInfo;

            List<AbstractActor> sensorSources = new List<AbstractActor>();
            if (source == null) {
                // We don't have an active source, so treat the entire allied faction as sources
                Mod.Log.Trace($"No primary lock source, assuming all allies.");
                sensorSources.AddRange(target.Combat.GetAllAlliesOf(target.Combat.LocalPlayerTeam));
            } else {
                // We have an active source, so only use that model plus any 'shares sensors' models
                Mod.Log.Trace($"Actor:({CombatantUtils.Label(source)}) is primary lock source.");
                sensorSources.Add(source);
            }

            Mod.Log.Trace($"Checking locks from {sensorSources.Count} sources.");
            foreach (AbstractActor actor in sensorSources) {
                SensorScanType actorScanType = CalculateSensorInfoLevel(actor, target);
                if (actorScanType > scanType) {
                    Mod.Log.Trace($"Increasing scanType to: ({actorScanType}) from source:({CombatantUtils.Label(actor)}) ");
                    scanType = actorScanType;
                }
            }

            Mod.Log.Trace($"Shared lock to target:({CombatantUtils.Label(target)}) is type: ({scanType})");
            return scanType;
        }

        public static SensorScanType CalculateSensorLock(AbstractActor source, Vector3 sourcePos, ICombatant target, Vector3 targetPos) {

            if (source.GUID == target.GUID || source.Combat.HostilityMatrix.IsFriendly(source.TeamId, target.team.GUID)) {
                // If they are us, or allied, automatically give sensor details
                Mod.Log.Trace($"  source:{CombatantUtils.Label(source)} is friendly to target:{CombatantUtils.Label(target)}. Forcing full visibility.");
                return SensorScanType.AllInformation;
            }

            if (source.IsDead || source.IsFlaggedForDeath) {
                // If we're dead, we can't have vision or sensors. If we're off the map, we can't either. If the target is off the map, we can't see it.                
                Mod.Log.Trace($"  source:{CombatantUtils.Label(source)} is dead or dying. Forcing no visibility.");
                return SensorScanType.NoInfo;
            }

            if (target.IsDead || target.IsFlaggedForDeath) {
                // If the target is dead, we can't have sensor but we have vision 
                Mod.Log.Trace($"  target:{CombatantUtils.Label(target)} is dead or dying. Forcing no sensor lock, vision based upon visibility.");
                return SensorScanType.NoInfo;
            }

            if (source.IsTeleportedOffScreen) {
                Mod.Log.Trace($"  source as is teleported off screen. Skipping.");
                return SensorScanType.NoInfo;
            }

            if (source.StatCollection.ContainsStatistic(ModStats.DisableSensors) &&
                source.StatCollection.GetValue<bool>(ModStats.DisableSensors))
            {
                Mod.Log.Trace($"  sensors disabled for source, returning no info.");
                return SensorScanType.NoInfo;
            }

            EWState sourceState = new EWState(source);
            float distance = Vector3.Distance(sourcePos, targetPos);
            float sensorRangeVsTarget = SensorLockHelper.GetAdjustedSensorRange(source, target);
            Mod.Log.Trace($"SensorLockHelper - source: {CombatantUtils.Label(source)} sensorRangeVsTarget: {sensorRangeVsTarget} vs distance: {distance}");
            if (target is BattleTech.Building targetBuilding) {
                // If the target is a building, show them so long as they are in sensor distance
                // TODO: ADD FRIENDLY ECM CHECK HERE?

                // TODO: This should be calculated more fully! Major bug here!
                SensorScanType buildingLock = sourceState.GetCurrentEWCheck() > 0 ? SensorScanType.ArmorAndWeaponType : SensorScanType.NoInfo;
                Mod.Log.Trace($"  target:{CombatantUtils.Label(targetBuilding)} is a building with lockState:{buildingLock}");
                return buildingLock;
            } else if ((target as AbstractActor) != null) {
                AbstractActor targetActor = target as AbstractActor;
                EWState targetState = new EWState(targetActor);

                if (distance > sensorRangeVsTarget) {
                    // Check for Narc effect that will show the target regardless of range
                    SensorScanType narcLock = HasNarcBeaconDetection(target, sourceState, targetState) ? SensorScanType.LocationAndType : SensorScanType.NoInfo;
                    Mod.Log.Trace($"  source:{CombatantUtils.Label(source)} is out of range, lock from Narc is:{narcLock}");
                    return narcLock;
                } else {
                    SensorScanType sensorLock = SensorScanType.NoInfo;
                    if (targetActor.IsTeleportedOffScreen) {
                        Mod.Log.Trace($"  target is teleported off screen. Skipping.");
                    } else {
                        // TODO: Re-add shadowing logic
                        // TODO: SensorLock adds a boost from friendlies if they have shares sensors?
                        // We are within range, but check to see if the sensorInfoCheck failed
                        sensorLock = CalculateSensorInfoLevel(source, target);

                        // Check for Narc effect overriding detection
                        if (sensorLock < SensorScanType.LocationAndType && HasNarcBeaconDetection(targetActor, sourceState, targetState)) {
                            sensorLock = SensorScanType.LocationAndType;
                        }
                    }
                    Mod.Log.Trace($"SensorLockHelper - source:{CombatantUtils.Label(source)} has sensorLock:({sensorLock}) vs " +
                        $"target:{CombatantUtils.Label(target)}");
                    return sensorLock;
                }
            } else {
                Mod.Log.Info($"SensorLockHelper - fallthrough case for target: {CombatantUtils.Label(target)} with type: {target.GetType()}. Returning NoLock!");
                return SensorScanType.NoInfo;
            }
        }

        private static bool HasNarcBeaconDetection(ICombatant target, EWState sourceState, EWState targetState) {
            bool hasDetection = false;
            if (target != null && targetState != null && targetState.IsNarced(sourceState)) {
                hasDetection = true; 
            }
            return hasDetection;
        }

        private static SensorScanType CalculateSensorInfoLevel(AbstractActor source, ICombatant target) {
            Mod.Log.Trace($"Calculating SensorInfo from source: ({CombatantUtils.Label(source)}) to target: ({CombatantUtils.Label(target)})");

            // Determine modified check against target
            EWState sourceState = new EWState(source);

            int positiveMods = 0;
            int negativeMods = 0;
            int ecmNegativeMods = 0;

            positiveMods += sourceState.GetCurrentEWCheck(); 
            Mod.Log.Trace($" == detailsLevel from EW check = {positiveMods}");

            // --- Source: Advanced Sensors
            if (sourceState.AdvancedSensorsMod() > 0) {
                Mod.Log.Trace($" == source has advanced sensors, detailsLevel = {positiveMods} + {sourceState.AdvancedSensorsMod()}");
                positiveMods += sourceState.AdvancedSensorsMod();
            }

            // --- Source: ECM Jamming
            if (sourceState.GetRawECMJammed() > 0) {
                Mod.Log.Trace($" == source is jammed by ECM, detailsLevel = {ecmNegativeMods} - {sourceState.GetRawECMJammed()}");
                ecmNegativeMods -= sourceState.GetRawECMJammed();
            }

            // --- Target: Stealth, Narc, Tag
            AbstractActor targetActor = target as AbstractActor;
            if (targetActor != null) {
                EWState targetState = new EWState(targetActor);

                // ECM Shield reduces sensor info
                if (targetState.ECMDetailsMod(sourceState) > 0) {
                    Mod.Log.Trace($" == target is shielded by ECM, detailsLevel = {ecmNegativeMods} - {targetState.ECMDetailsMod(sourceState)}");
                    ecmNegativeMods -= targetState.ECMDetailsMod(sourceState);
                }

                // Stealth reduces sensor info
                if (targetState.HasStealth()) {
                    Mod.Log.Trace($" == target has stealth, detailsLevel = {negativeMods} - {targetState.StealthDetailsMod()}");
                    negativeMods -= targetState.StealthDetailsMod();
                }

                // A Narc effect increases sensor info
                // TODO: Narc should effect buildings
                if (targetState.IsNarced(sourceState)) {
                    Mod.Log.Trace($" == target is NARC'd, detailsLevel = {positiveMods} + {targetState.NarcDetailsMod(sourceState)}");
                    positiveMods += targetState.NarcDetailsMod(sourceState);
                }

                // A TAG effect increases sensor info
                // TODO: TAG should effect buildings
                if (targetState.IsTagged(sourceState)) {
                    Mod.Log.Trace($" == target is tagged, detailsLevel = {positiveMods} + {targetState.TagDetailsMod(sourceState)}");
                    positiveMods += targetState.TagDetailsMod(sourceState);
                }

                // Active Probe ping acts as sensors boost as well
                if (targetState.PingedByProbeMod() != 0) {
                    Mod.Log.Trace($" == target is pinged by probe, detailsLevel = {positiveMods} + {targetState.PingedByProbeMod()}");
                    positiveMods += targetState.PingedByProbeMod();
                }

            }

            if (ecmNegativeMods < Mod.Config.Sensors.MaxECMDetailsPenalty) {
                Mod.Log.Trace($"  == negatives exceed cap, setting negative penalty to Sensors.MaxECMDetailsPenalty: {Mod.Config.Sensors.MaxECMDetailsPenalty }");
                ecmNegativeMods = Mod.Config.Sensors.MaxECMDetailsPenalty; 
            }

            int detailLevel = positiveMods + negativeMods + ecmNegativeMods;
            Mod.Log.Trace($"  == detailsTotal: {detailLevel} = positiveMods: {positiveMods} + negativeMods: {negativeMods}");

            SensorScanType sensorInfo = SensorScanTypeHelper.DetectionLevelForCheck(detailLevel);
            Mod.Log.Trace($" == Calculated sensorInfo as: ({sensorInfo})");

            return sensorInfo;
        }


    }
}
