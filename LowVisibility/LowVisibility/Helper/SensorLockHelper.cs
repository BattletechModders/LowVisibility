using LowVisibility.Object;
using System.Collections.Generic;
using UnityEngine;
using us.frostraptor.modUtils;

namespace LowVisibility.Helper
{
    public static class SensorLockHelper
    {

        // WARNING: DUPLICATE OF HBS CODE. THIS IS LIKELY TO BREAK IF HBS CHANGES THE SOURCE FUNCTIONS
        public static float GetSensorsRange(AbstractActor source)
        {

            if (source.StatCollection.ContainsStatistic(ModStats.DisableSensors))
            {
                Mod.Log.Debug?.Write($"Returning minimum sensors range for {CombatantUtils.Label(source)} due to disabled sensors.");
                return Mod.Config.Sensors.MinimumRange;
            }

            // Add multipliers and absolute bonuses
            EWState ewState = source.GetEWState();

            Mod.Log.Trace?.Write($"  == Sensors Range for for actor:{CombatantUtils.Label(source)}");

            float rawRangeMulti = SensorLockHelper.GetAllSensorRangeMultipliers(source);
            float rangeMulti = rawRangeMulti + ewState.GetSensorsRangeMulti();
            Mod.Log.Trace?.Write($"    rangeMulti: {rangeMulti} = rawRangeMulti: {rawRangeMulti} + sensorCheckRangeMulti: {ewState.GetSensorsRangeMulti()}");

            float rawRangeMod = SensorLockHelper.GetAllSensorRangeAbsolutes(source);
            float rangeMod = rawRangeMod * (1 + ewState.GetSensorsRangeMulti());
            Mod.Log.Trace?.Write($"    rangeMod: {rangeMod} = rawRangeMod: {rawRangeMod} + sensorCheckRangeMulti: {ewState.GetSensorsRangeMulti()}");

            float sensorsRange = ewState.GetSensorsBaseRange() * rangeMulti + rangeMod;
            Mod.Log.Trace?.Write($"    sensorsRange: {sensorsRange} = baseRange: {ewState.GetSensorsBaseRange()} * rangeMult: {rangeMulti} + rangeMod: {rangeMod}");

            if (sensorsRange < Mod.Config.Sensors.MinimumRange) sensorsRange = Mod.Config.Sensors.MinimumRange;

            return sensorsRange;
        }

        public static float GetAdjustedSensorRange(AbstractActor source, ICombatant target)
        {
            EWState sourceState = source.GetEWState();
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
        public static float GetAllSensorRangeMultipliers(AbstractActor source)
        {
            if (source == null)
            {
                return 1f;
            }
            float sensorMulti = source.SensorDistanceMultiplier;

            DesignMaskDef occupiedDesignMask = source.occupiedDesignMask;
            if (occupiedDesignMask != null) { sensorMulti *= occupiedDesignMask.sensorRangeMultiplier; }

            return sensorMulti;
        }

        // WARNING: DUPLICATE OF HBS CODE. THIS IS LIKELY TO BREAK IF HBS CHANGES THE SOURCE FUNCTIONS
        private static float GetAllSensorRangeAbsolutes(AbstractActor source)
        {
            return source == null ? 0f : source.SensorDistanceAbsolute;
        }

        // WARNING: DUPLICATE OF HBS CODE. THIS IS LIKELY TO BREAK IF HBS CHANGES THE SOURCE FUNCTIONS
        public static float GetTargetSignature(ICombatant target, EWState sourceState)
        {

            if (target == null || (target as AbstractActor) == null) { return 1f; }

            AbstractActor targetActor = target as AbstractActor;
            float allTargetSignatureModifiers = GetAllTargetSignatureModifiers(targetActor, sourceState);
            float staticSignature = 1f * allTargetSignatureModifiers;

            // Add in any design mask boosts
            DesignMaskDef occupiedDesignMask = targetActor.occupiedDesignMask;
            if (occupiedDesignMask != null) { staticSignature *= occupiedDesignMask.signatureMultiplier; }

            if (staticSignature < Mod.Config.Sensors.MinSignature)
                staticSignature = Mod.Config.Sensors.MinSignature;

            return staticSignature;
        }

        // WARNING: DUPLICATE OF HBS CODE. THIS IS LIKELY TO BREAK IF HBS CHANGES THE SOURCE FUNCTIONS
        private static float GetAllTargetSignatureModifiers(AbstractActor target, EWState sourceState)
        {
            if (target == null) { return 1f; }

            float shutdownMod = (!target.IsShutDown) ? 1f : target.Combat.Constants.Visibility.ShutDownSignatureModifier;

            float chassisSignature = target.StatCollection.GetValue<float>(ModStats.HBS_SensorSignatureModifier);
            if (chassisSignature == 0f) chassisSignature = 1.0f; // Normalize 0 (vanilla state) for BEX; RT has updated their chassisDefs

            EWState ewState = target.GetEWState();
            float ecmShieldMod = ewState.ECMSignatureMod(sourceState);
            float stealthMod = ewState.StealthSignatureMod(sourceState);
            float narcMod = ewState.NarcSignatureMod(sourceState);
            float tagMod = ewState.TagSignatureMod(sourceState);

            float signatureMods = (1.0f + stealthMod) * (1.0f + ecmShieldMod) * (1.0f + narcMod) * (1.0f + tagMod);
            float targetSignature = chassisSignature * shutdownMod * signatureMods;
            Mod.Log.Trace?.Write($" Actor: {CombatantUtils.Label(target)} has signature: {targetSignature} = " +
                $"rawSignature: {chassisSignature} x shutdown: {shutdownMod}" +
                $" x (1.0 + ecmShield: {ecmShieldMod})" +
                $" x (1.0 + stealthMod: {stealthMod})" +
                $" x (1.0 + narc: {narcMod})" +
                $" x (1.0 + tag: {tagMod})");

            if (targetSignature < Mod.Config.Sensors.MinSignature) targetSignature = Mod.Config.Sensors.MinSignature;

            return targetSignature;
        }

        // Iterates over all player-allied units, checks for highest lock level to determine unit 
        public static SensorScanType CalculateSharedLock(ICombatant target, AbstractActor source)
        {
            return CalculateSharedLock(target, source, Vector3.zero);
        }

        public static SensorScanType CalculateSharedLock(ICombatant target, AbstractActor source, Vector3 previewPos)
        {

            SensorScanType scanType = SensorScanType.NoInfo;

            List<AbstractActor> sensorSources = new List<AbstractActor>();
            if (source == null)
            {
                // We don't have an active source, so treat the entire allied faction as sources
                Mod.Log.Trace?.Write($"No primary lock source, assuming all allies.");
                sensorSources.AddRange(target.Combat.GetAllAlliesOf(target.Combat.LocalPlayerTeam));
            }
            else
            {
                // We have an active source, so only use that model plus any 'shares sensors' models
                Mod.Log.Trace?.Write($"Actor:({CombatantUtils.Label(source)}) is primary lock source.");
                sensorSources.Add(source);
            }

            Mod.Log.Trace?.Write($"Checking locks from {sensorSources.Count} sources.");
            foreach (AbstractActor actor in sensorSources)
            {
                SensorScanType actorScanType = CalculateSensorInfoLevel(actor, target);
                if (actorScanType > scanType)
                {
                    Mod.Log.Trace?.Write($"Increasing scanType to: ({actorScanType}) from actor:({CombatantUtils.Label(actor)}) ");
                    scanType = actorScanType;
                }
            }

            // If we have a preview pos, the regular VisibilityCache hooks won't fire. Calculate the current actor's moves directly
            if (previewPos != Vector3.zero)
            {
                SensorScanType sourceType = CalculateSensorLock(source, previewPos, target, target.CurrentPosition);
                if (sourceType > scanType)
                {
                    Mod.Log.Trace?.Write($"Increasing scanType to: ({sourceType}) from source:({CombatantUtils.Label(source)}) ");
                    scanType = sourceType;
                }
            }

            Mod.Log.Debug?.Write($"Shared lock to target:({CombatantUtils.Label(target)}) is type: ({scanType})");
            return scanType;
        }

        public static SensorScanType CalculateSensorLock(AbstractActor source, Vector3 sourcePos, ICombatant target, Vector3 targetPos)
        {

            if (source.GUID == target.GUID || source.Combat.HostilityMatrix.IsFriendly(source.TeamId, target.team.GUID))
            {
                // If they are us, or allied, automatically give sensor details
                Mod.Log.Trace?.Write($"  source:{CombatantUtils.Label(source)} is friendly to target:{CombatantUtils.Label(target)}. Forcing full visibility.");
                return SensorScanType.AllInformation;
            }

            if (source.IsDead || source.IsFlaggedForDeath)
            {
                // If we're dead, we can't have vision or sensors. If we're off the map, we can't either. If the target is off the map, we can't see it.                
                Mod.Log.Trace?.Write($"  source:{CombatantUtils.Label(source)} is dead or dying. Forcing no visibility.");
                return SensorScanType.NoInfo;
            }

            if (target.IsDead || target.IsFlaggedForDeath)
            {
                // If the target is dead, we can't have sensor but we have vision 
                Mod.Log.Trace?.Write($"  target:{CombatantUtils.Label(target)} is dead or dying. Forcing no sensor lock, vision based upon visibility.");
                return SensorScanType.NoInfo;
            }

            if (source.IsTeleportedOffScreen)
            {
                Mod.Log.Trace?.Write($"  source as is teleported off screen. Skipping.");
                return SensorScanType.NoInfo;
            }

            if (source.StatCollection.ContainsStatistic(ModStats.DisableSensors))
            {
                Mod.Log.Debug?.Write($"Sensors disabled for source: {CombatantUtils.Label(source)}, returning no info.");
                return SensorScanType.NoInfo;
            }

            EWState sourceState = source.GetEWState();
            float distance = Vector3.Distance(sourcePos, targetPos);
            float sensorRangeVsTarget = SensorLockHelper.GetAdjustedSensorRange(source, target);
            Mod.Log.Trace?.Write($"SensorLockHelper - source: {CombatantUtils.Label(source)} sensorRangeVsTarget: {sensorRangeVsTarget} vs distance: {distance}");
            if (target is BattleTech.Building targetBuilding)
            {
                // If the target is a building, show them so long as they are in sensor distance
                // TODO: ADD FRIENDLY ECM CHECK HERE?

                // TODO: This should be calculated more fully! Major bug here!
                SensorScanType buildingLock = sourceState.GetCurrentEWCheck() > 0 ? SensorScanType.ArmorAndWeaponType : SensorScanType.NoInfo;
                Mod.Log.Trace?.Write($"  target:{CombatantUtils.Label(targetBuilding)} is a building with lockState:{buildingLock}");
                return buildingLock;
            }
            else if ((target as AbstractActor) != null)
            {
                AbstractActor targetActor = target as AbstractActor;
                EWState targetState = targetActor.GetEWState();

                if (distance > sensorRangeVsTarget)
                {
                    // Check for Narc effect that will show the target regardless of range
                    SensorScanType narcLock = HasNarcBeaconDetection(target, sourceState, targetState) ? SensorScanType.LocationAndType : SensorScanType.NoInfo;
                    Mod.Log.Trace?.Write($"  source:{CombatantUtils.Label(source)} is out of range, lock from Narc is:{narcLock}");
                    return narcLock;
                }
                else
                {
                    SensorScanType sensorLock = SensorScanType.NoInfo;
                    if (targetActor.IsTeleportedOffScreen)
                    {
                        Mod.Log.Trace?.Write($"  target is teleported off screen. Skipping.");
                    }
                    else
                    {
                        // TODO: Re-add shadowing logic
                        // TODO: SensorLock adds a boost from friendlies if they have shares sensors?
                        // We are within range, but check to see if the sensorInfoCheck failed
                        sensorLock = CalculateSensorInfoLevel(source, target);

                        // Check for Narc effect overriding detection
                        if (sensorLock < SensorScanType.LocationAndType && HasNarcBeaconDetection(targetActor, sourceState, targetState))
                        {
                            sensorLock = SensorScanType.LocationAndType;
                        }
                    }
                    Mod.Log.Trace?.Write($"SensorLockHelper - source:{CombatantUtils.Label(source)} has sensorLock:({sensorLock}) vs " +
                        $"target:{CombatantUtils.Label(target)}");
                    return sensorLock;
                }
            }
            else
            {
                Mod.Log.Info?.Write($"SensorLockHelper - fallthrough case for target: {CombatantUtils.Label(target)} with type: {target.GetType()}. Returning NoLock!");
                return SensorScanType.NoInfo;
            }
        }

        private static bool HasNarcBeaconDetection(ICombatant target, EWState sourceState, EWState targetState)
        {
            bool hasDetection = false;
            if (target != null && targetState != null && targetState.IsNarced(sourceState))
            {
                hasDetection = true;
            }
            return hasDetection;
        }

        private static SensorScanType CalculateSensorInfoLevel(AbstractActor source, ICombatant target)
        {
            Mod.Log.Trace?.Write($"Calculating SensorInfo from source: ({CombatantUtils.Label(source)}) to target: ({CombatantUtils.Label(target)})");

            if (source.StatCollection.ContainsStatistic(ModStats.DisableSensors)) return SensorScanType.NoInfo;

            // Determine modified check against target
            EWState sourceState = source.GetEWState();

            int positiveMods = 0;
            int negativeMods = 0;
            int ecmNegativeMods = 0;

            positiveMods += sourceState.GetCurrentEWCheck();
            Mod.Log.Trace?.Write($" == detailsLevel from EW check = {positiveMods}");

            // --- Source: Advanced Sensors
            if (sourceState.AdvancedSensorsMod() > 0)
            {
                Mod.Log.Trace?.Write($" == source has advanced sensors, detailsLevel = {positiveMods} + {sourceState.AdvancedSensorsMod()}");
                positiveMods += sourceState.AdvancedSensorsMod();
            }

            // --- Source: ECM Jamming
            if (sourceState.GetRawECMJammed() > 0)
            {
                Mod.Log.Trace?.Write($" == source is jammed by ECM, detailsLevel = {ecmNegativeMods} - {sourceState.GetRawECMJammed()}");
                ecmNegativeMods -= sourceState.GetRawECMJammed();
            }

            // --- Target: Stealth, Narc, Tag
            AbstractActor targetActor = target as AbstractActor;
            if (targetActor != null)
            {
                EWState targetState = targetActor.GetEWState();

                // ECM Shield reduces sensor info
                if (targetState.ECMDetailsMod(sourceState) > 0)
                {
                    Mod.Log.Trace?.Write($" == target is shielded by ECM, detailsLevel = {ecmNegativeMods} - {targetState.ECMDetailsMod(sourceState)}");
                    ecmNegativeMods -= targetState.ECMDetailsMod(sourceState);
                }

                // Stealth reduces sensor info
                if (targetState.HasStealth())
                {
                    Mod.Log.Trace?.Write($" == target has stealth, detailsLevel = {negativeMods} - {targetState.StealthDetailsMod()}");
                    negativeMods -= targetState.StealthDetailsMod();
                }

                // A Narc effect increases sensor info
                // TODO: Narc should effect buildings
                if (targetState.IsNarced(sourceState))
                {
                    Mod.Log.Trace?.Write($" == target is NARC'd, detailsLevel = {positiveMods} + {targetState.NarcDetailsMod(sourceState)}");
                    positiveMods += targetState.NarcDetailsMod(sourceState);
                }

                // A TAG effect increases sensor info
                // TODO: TAG should effect buildings
                if (targetState.IsTagged(sourceState))
                {
                    Mod.Log.Trace?.Write($" == target is tagged, detailsLevel = {positiveMods} + {targetState.TagDetailsMod(sourceState)}");
                    positiveMods += targetState.TagDetailsMod(sourceState);
                }

                // Active Probe ping acts as sensors boost as well
                if (targetState.PingedByProbeMod() != 0)
                {
                    Mod.Log.Trace?.Write($" == target is pinged by probe, detailsLevel = {positiveMods} + {targetState.PingedByProbeMod()}");
                    positiveMods += targetState.PingedByProbeMod();
                }

            }

            if (ecmNegativeMods < Mod.Config.Sensors.MaxECMDetailsPenalty)
            {
                Mod.Log.Trace?.Write($"  == negatives exceed cap, setting negative penalty to Sensors.MaxECMDetailsPenalty: {Mod.Config.Sensors.MaxECMDetailsPenalty}");
                ecmNegativeMods = Mod.Config.Sensors.MaxECMDetailsPenalty;
            }

            int detailLevel = positiveMods + negativeMods + ecmNegativeMods;
            Mod.Log.Trace?.Write($"  == detailsTotal: {detailLevel} = positiveMods: {positiveMods} + negativeMods: {negativeMods}");

            SensorScanType sensorInfo = SensorScanTypeHelper.DetectionLevelForCheck(detailLevel);
            Mod.Log.Trace?.Write($" == Calculated sensorInfo as: ({sensorInfo})");

            return sensorInfo;
        }


    }
}
