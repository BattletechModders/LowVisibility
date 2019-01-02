using BattleTech;
using HBS.Collections;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace LowVisibility.Helper {
    public static class ActorHelper {

        // none = 0-12, short = 13-19, medium  = 20-26, long = 27-36
        public const int LongRangeRollBound = 27;
        public const int MediumRangeRollBound = 20;
        public const int ShortRangeRollBound = 13;

        public class ActorEWConfig {
            // ECM Equipment = ecm_t0, Guardian ECM = ecm_t1, Angel ECM = ecm_t2, CEWS = ecm_t3. -1 means none.
            public int ecmTier = -1;
            public float ecmRange = 0;
            public int ecmModifier = 0; // Any additional modifier to opposed ECM modifier for the sensor check

            // Pirate = activeprobe_t0, Beagle = activeprobe_t1, Bloodhound = activeprobe_t2, CEWS = activeprobe_t3. -1 means none.
            public int probeTier = -1;
            public float probeRange = 0;
            public int probeModifier = 0; // The sensor check modifier used in opposed cases (see MaxTech 55)

            // The amount of tactics bonus to the sensor check
            public int tacticsBonus = 0;

            public override string ToString() {
                return $"tacticsBonus:+{tacticsBonus} ecmTier:{ecmTier} ecmRange:{ecmRange} probeTier:{probeTier} probeRange:{probeRange}";
            }
        };

        // The level an enemy has been identified to
        public enum IDState {
            None,
            Silhouette,
            VisualID,
            SensorID,
            ProbeID
        }

        // The range a unit can detect enemies out to
        public enum RoundDetectRange {
            VisualOnly = 0,
            SensorsShort = 1,
            SensorsMedium = 2,
            SensorsLong = 3
        };

        // See MaxTech pg.55 for detect range check. We adjust scale from 12 to 36 to allow tactics modifier to apply
        public static RoundDetectRange MakeSensorRangeCheck(AbstractActor actor) {
            ActorEWConfig config = State.GetOrCreateActorEWConfig(actor);
            int randomRoll = LowVisibility.Random.Next(0, 36);
            int jammingMod = State.IsJammed(actor) ? config.probeModifier - State.JammingStrength(actor) : 0;            
            int modifiedRoll = randomRoll + config.tacticsBonus + jammingMod;
            LowVisibility.Logger.LogIfDebug($"Actor:{actor.DisplayName}_{actor.GetPilot().Name} rolled:{randomRoll} + tactics:{config.tacticsBonus} + jamming:{jammingMod} = {modifiedRoll}");

            // none = 0-12, short = 13-19, medium  = 20-26, long = 27-36
            RoundDetectRange detectRange = RoundDetectRange.VisualOnly;
            if (modifiedRoll >= LongRangeRollBound) {
                LowVisibility.Logger.LogIfDebug($"Actor:{actor.DisplayName}_{actor.GetPilot().Name} rolled LONG range");
                detectRange = RoundDetectRange.SensorsLong;
            } else if (modifiedRoll >= MediumRangeRollBound) {
                LowVisibility.Logger.LogIfDebug($"Actor:{actor.DisplayName}_{actor.GetPilot().Name} rolled MEDIUM range");
                detectRange = RoundDetectRange.SensorsMedium;
            } else if (modifiedRoll >= ShortRangeRollBound) {
                LowVisibility.Logger.LogIfDebug($"Actor:{actor.DisplayName}_{actor.GetPilot().Name} rolled SHORT range");
                detectRange = RoundDetectRange.SensorsShort;
            } else {
                LowVisibility.Logger.LogIfDebug($"Actor:{actor.DisplayName}_{actor.GetPilot().Name} FAILED their check.");
                detectRange = RoundDetectRange.VisualOnly;
                actor.Combat.MessageCenter.PublishMessage(new FloatieMessage(actor.GUID, actor.GUID, "Sensor Check Failed - visuals only!", FloatieMessage.MessageNature.Neutral));
            }

            // TODO: Should this move to another place
            LowVisibility.Logger.LogIfDebug($"Actor:{actor.DisplayName}_{actor.GetPilot().Name} has a detect range of: {detectRange}");
            return detectRange;
        }

        public static ActorEWConfig CalculateEWConfig(AbstractActor actor) {
            // Check tags for any ecm/sensors
            // TODO: Check for stealth
            int actorEcmTier = -1;
            float actorEcmRange = 0;
            int actorEcmModifier = 0;

            int actorProbeTier = -1;
            float actorProbeRange = 0;
            int actorProbeModifier = 0;
             
            foreach (MechComponent component in actor.allComponents) {
                MechComponentRef componentRef = component?.mechComponentRef;
                MechComponentDef componentDef = componentRef.Def;
                TagSet componentTags = componentDef.ComponentTags;
                foreach (string tag in componentTags) {
                    if (tag.StartsWith("ecm_t")) {
                        LowVisibility.Logger.LogIfDebug($"Actor:{actor.DisplayName}_{actor.GetPilot().Name} has ECM component:{componentRef.ComponentDefID} with tag:{tag}");
                        string[] split = tag.Split('_');
                        int tier = Int32.Parse(split[1].Substring(1));
                        int range = Int32.Parse(split[2].Substring(1));
                        int modifier = Int32.Parse(split[3].Substring(1));
                        if (tier >= actorEcmTier) {
                            actorEcmTier = tier;
                            actorEcmRange = range * 30.0f;
                            actorEcmModifier = modifier;
                        }
                    } else if (tag.StartsWith("activeprobe_t")) {
                        LowVisibility.Logger.LogIfDebug($"Actor:{actor.DisplayName}_{actor.GetPilot().Name} has Probe component:{componentRef.ComponentDefID} with tag:{tag}");
                        string[] split = tag.Split('_');
                        int tier = Int32.Parse(split[1].Substring(1));
                        int range = Int32.Parse(split[2].Substring(1));
                        int modifier = Int32.Parse(split[3].Substring(1));
                        if (tier >= actorProbeTier) {
                            actorProbeTier = tier;
                            actorProbeRange = range * 30.0f;
                            actorProbeModifier = modifier;
                        }
                    }
                }
            }

            // Determine pilot bonus
            int pilotTactics = actor.GetPilot().Tactics;
            int unitTacticsBonus = NormalizeSkill(pilotTactics);

            ActorEWConfig config = new ActorEWConfig {
                ecmTier = actorEcmTier,
                ecmRange = actorEcmRange,
                ecmModifier = actorEcmModifier,
                probeTier = actorProbeTier,                
                probeRange = actorProbeRange,
                probeModifier = actorProbeModifier,
                tacticsBonus = unitTacticsBonus
            };
            LowVisibility.Logger.LogIfDebug($"Actor:{actor.DisplayName}_{actor.GetPilot().Name} EWConfig is:{config}");

            return config;
        }

        // TODO: Allies don't impact this calculation
        public static IDState CalculateTargetIDLevel(AbstractActor target) {
            if (target.team == target.Combat.LocalPlayerTeam) { return IDState.ProbeID;  }

            IDState idState = IDState.None;
            float targetSignature = CalculateTargetSignature(target);
            float targetVisibility = CalculateTargetVisibility(target);
            foreach (AbstractActor actor in target.Combat.LocalPlayerTeam.units) {                
                ActorEWConfig ewConfig = State.GetOrCreateActorEWConfig(actor);
                RoundDetectRange roundDetect = State.GetOrCreateRoundDetectResults(actor);
                VisibilityLevelAndAttribution visLevelAndAttrib = actor.VisibilityCache.VisibilityToTarget(target);

                // TODO: Need to handle target signature reductions for sensors

                // Check for visibility 
                if (visLevelAndAttrib.VisibilityLevel == VisibilityLevel.LOSFull) {
                    if (idState < IDState.Silhouette) { idState = IDState.Silhouette; }
                }

                // Check for visual ID
                float distance = Vector3.Distance(actor.CurrentPosition, target.CurrentPosition);
                float visionRange = State.GetVisualIDRange() * targetVisibility;
                LowVisibility.Logger.Log($"actor:{actor.DisplayName}_{actor.GetPilot().Name} has vision range:{visionRange} and is distance:{distance} " +
                    $"from target:{target.DisplayName}_{target.GetPilot().Name} with visibiilty:{targetVisibility}");
                if (distance <= visionRange && idState < IDState.VisualID) { idState = IDState.VisualID; }

                // Check for sensors
                float sensorsRange = CalculateSensorRange(actor) * targetSignature;
                LowVisibility.Logger.Log($"actor:{actor.DisplayName}_{actor.GetPilot().Name} has sensorsRange:{sensorsRange} vs distance:{distance}");
                if (distance <= sensorsRange && idState < IDState.SensorID) { idState = IDState.SensorID; }

                // Check for probes
                if (ewConfig.probeTier >= 0) {
                    LowVisibility.Logger.Log($"actor:{actor.DisplayName}_{actor.GetPilot().Name} has probeRange:{sensorsRange} vs distance:{distance}");
                    if (distance <= sensorsRange && idState < IDState.ProbeID) { idState = IDState.ProbeID; }
                }
            }           
            LowVisibility.Logger.Log($"Target:{target.DisplayName}_{target.GetPilot().Name} has IDstate:{idState} from one or more player units.");
            return idState;
        }

        // Determine an actor's sensor range, plus our special additions
        public static float CalculateSensorRange(AbstractActor source) {
            // Determine type
            float[] sensorRanges = null;
            if (source.GetType() == typeof(Mech)) { sensorRanges = LowVisibility.Config.MechSensorRanges;  }
            else if (source.GetType() == typeof(Vehicle)) { sensorRanges = LowVisibility.Config.VehicleSensorRanges; } 
            else if (source.GetType() == typeof(Turret)) { sensorRanges = LowVisibility.Config.TurretSensorRanges; }
            else { sensorRanges = LowVisibility.Config.UnknownSensorRanges; }

            // Determine base range from type
            float baseSensorRange = 0.0f;
            RoundDetectRange detectRange = State.GetOrCreateRoundDetectResults(source);
            if (detectRange == RoundDetectRange.SensorsLong) { baseSensorRange = sensorRanges[2]; } 
            else if (detectRange == RoundDetectRange.SensorsMedium) { baseSensorRange = sensorRanges[1]; } 
            else if (detectRange == RoundDetectRange.SensorsShort) { baseSensorRange = sensorRanges[0]; }

            // Add multipliers and absolute bonuses
            float staticSensorRangeMultis = GetAllSensorRangeMultipliers(source);
            float staticSensorRangeMods = GetAllSensorRangeAbsolutes(source);

            // Add the probe range if present
            ActorEWConfig ewConfig = State.GetOrCreateActorEWConfig(source);
            float probeRange = ewConfig.probeTier > -1 ? ewConfig.probeRange * 30.0f : 0.0f;

            return baseSensorRange * staticSensorRangeMultis + staticSensorRangeMods + probeRange;
        }

        // Copy of LineOfSight::GetAllSensorRangeMultipliers
        private static float GetAllSensorRangeMultipliers(AbstractActor source) {
            if (source == null) {
                return 1f;
            }
            float sensorMulti = source.SensorDistanceMultiplier;

            DesignMaskDef occupiedDesignMask = source.occupiedDesignMask;
            if (occupiedDesignMask != null) { sensorMulti *= occupiedDesignMask.sensorRangeMultiplier; }

            return sensorMulti;
        }

        // Copy of LineOfSight::GetAllSensorRangeAbsolutes
        private static float GetAllSensorRangeAbsolutes(AbstractActor source) {
            if (source == null) { return 0f; }

            return source.SensorDistanceAbsolute;
        }

        // Determine a target's visual profile, including our additions
        public static float CalculateTargetVisibility(AbstractActor target) {
            if (target == null) { return 1f; }

            float allTargetVisibilityMultipliers = GetAllTargetVisibilityMultipliers(target);
            float allTargetVisibilityAbsolutes = GetAllTargetVisibilityAbsolutes(target);

            // TODO: Add stealth armor/NSS modifiers

            return 1f * allTargetVisibilityMultipliers + allTargetVisibilityAbsolutes;
        }

        // Copy of LineOfSight::GetAllTargetVisibilityMultipliers
        private static float GetAllTargetVisibilityMultipliers(AbstractActor target) {
            if (target == null) { return 1f; }

            float baseVisMulti = 0f;
            float shutdownVisMulti = (!target.IsShutDown) ? 0f : target.Combat.Constants.Visibility.ShutDownVisibilityModifier;
            float spottingVisibilityMultiplier = target.SpottingVisibilityMultiplier;            

            return baseVisMulti + shutdownVisMulti + spottingVisibilityMultiplier;
        }

        // Copy of LineOfSight::GetAllTargetVisibilityAbsolutes
        private static float GetAllTargetVisibilityAbsolutes(AbstractActor target) {
            if (target == null) { return 0f; }

            float baseVisMod = 0f;
            float spottingVisibilityAbsolute = target.SpottingVisibilityAbsolute;            

            return baseVisMod + spottingVisibilityAbsolute;
        }

        // Determine a target's sensor signature (plus our additions)
        public static float CalculateTargetSignature(AbstractActor target) {
            if (target == null) { return 1f; }

            float allTargetSignatureModifiers = GetAllTargetSignatureModifiers(target);
            float staticSignature = 1f + allTargetSignatureModifiers;

            // Add in any design mask boosts
            DesignMaskDef occupiedDesignMask = target.occupiedDesignMask;
            if (occupiedDesignMask != null) { staticSignature *= occupiedDesignMask.signatureMultiplier; }
            if (staticSignature < 0.01f) { staticSignature = 0.01f; }

            return staticSignature;
        }

        // Copy of LineOfSight::GetAllTargetSignatureModifiers
        private static float GetAllTargetSignatureModifiers(AbstractActor target) {
            if (target == null) { return 0f; }

            float shutdownSignatureMod = (!target.IsShutDown) ? 0f : target.Combat.Constants.Visibility.ShutDownSignatureModifier;
            float sensorSignatureModifier = target.SensorSignatureModifier;

            return shutdownSignatureMod + sensorSignatureModifier;
        }


        // A mapping of skill level to modifier
        private static readonly Dictionary<int, int> ModifierBySkill = new Dictionary<int, int> {
            { 1, 0 },
            { 2, 1 },
            { 3, 1 },
            { 4, 2 },
            { 5, 2 },
            { 6, 3 },
            { 7, 3 },
            { 8, 4 },
            { 9, 4 },
            { 10, 5 },
            { 11, 6 },
            { 12, 7 },
            { 13, 8 }
        };

        private static int NormalizeSkill(int rawValue) {
            int normalizedVal = rawValue;
            if (rawValue >= 11 && rawValue <= 14) {
                // 11, 12, 13, 14 normalizes to 11
                normalizedVal = 11;
            } else if (rawValue >= 15 && rawValue <= 18) {
                // 15, 16, 17, 18 normalizes to 14
                normalizedVal = 12;
            } else if (rawValue == 19 || rawValue == 20) {
                // 19, 20 normalizes to 13
                normalizedVal = 13;
            } else if (rawValue <= 0) {
                normalizedVal = 1;
            } else if (rawValue > 20) {
                normalizedVal = 13;
            }
            return normalizedVal;
        }
    }

}
