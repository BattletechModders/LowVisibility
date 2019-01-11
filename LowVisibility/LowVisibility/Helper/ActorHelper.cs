using BattleTech;

namespace LowVisibility.Helper {
    public static class ActorHelper {

        // none = 0-12, short = 13-19, medium  = 20-26, long = 27-36
        public const int LongRangeRollBound = 27;
        public const int MediumRangeRollBound = 20;
        public const int ShortRangeRollBound = 13;

        // The range a unit can detect enemies out to
        public enum RoundDetectRange {
            VisualOnly = 0,
            SensorsShort = 1,
            SensorsMedium = 2,
            SensorsLong = 3
        };

        // See MaxTech pg.55 for detect range check. We adjust scale from 12 to 36 to allow tactics modifier to apply
        public static RoundDetectRange MakeSensorRangeCheck(AbstractActor actor, bool doFloatie=false) {
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
                if (doFloatie) {
                    actor.Combat.MessageCenter.PublishMessage(
                        new FloatieMessage(actor.GUID, actor.GUID, "Sensor Check Failed!", FloatieMessage.MessageNature.Neutral));
                }
            }

            // TODO: Should this move to another place
            LowVisibility.Logger.LogIfDebug($"Actor:{actor.DisplayName}_{actor.GetPilot().Name} has a detect range of: {detectRange}");
            return detectRange;
        }

        public static string ActorLabel(AbstractActor actor) {
            return $"{actor.DisplayName}_{actor.GetPilot().Name}";
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
            float probeRange = ewConfig.probeTier > -1 ? ewConfig.probeRange : 0.0f;

            return (baseSensorRange + probeRange) * staticSensorRangeMultis + staticSensorRangeMods;
        }

        public static float GetVisualIDRangeForActor(AbstractActor source) {
            float mapVisualIDRange = State.GetVisualIDRange();

            float modifiedVisualIDRange = mapVisualIDRange;
            // Can't VisualID when shutdown
            if (source.IsShutDown) {
                modifiedVisualIDRange = 0f;
            } else if (source.IsProne) {
                mapVisualIDRange =  mapVisualIDRange * source.Combat.Constants.Visibility.ProneSpottingDistanceMultiplier;
            } else {
                float allSpotterMultipliers = GetAllSpotterMultipliers(source);
                float allSpotterAbsolutes = GetAllSpotterAbsolutes(source);
                modifiedVisualIDRange = mapVisualIDRange * allSpotterMultipliers + allSpotterAbsolutes;
                LowVisibility.Logger.LogIfDebug($"  actor:{ActorLabel(source)} with spotterMulti:{allSpotterMultipliers} spotterAbsolutes:{allSpotterAbsolutes} " +
                    $"and mapVisualIDRange:{mapVisualIDRange} has visualIDRange:{modifiedVisualIDRange}");
            }            
            
            return modifiedVisualIDRange; ;
        }

        // Copy of LineOfSight::GetAllSpotterMultipliers
        public static float GetAllSpotterMultipliers(AbstractActor source) {
            if (source == null) { return 1f;}
            float num = 0f;
            float spotterDistanceMultiplier = source.SpotterDistanceMultiplier;
            float num2 = 0f;
            return num + spotterDistanceMultiplier + num2;
        }

        // Token: 0x06007B85 RID: 31621 RVA: 0x002493E4 File Offset: 0x002475E4
        public static float GetAllSpotterAbsolutes(AbstractActor source) {
            if (source == null) { return 0f; }
            float spottingTacticsMultipler = 0f;
            if (source.IsPilotable) {
                Pilot pilot = source.GetPilot();
                if (pilot != null) {
                    int normdTactics = SkillHelper.NormalizeSkill(pilot.Tactics);
                    spottingTacticsMultipler = (float)normdTactics * source.Combat.Constants.Visibility.SpotterTacticsMultiplier;
                    LowVisibility.Logger.LogIfDebug($"  actor:{ActorLabel(source)} with tactics:{pilot.Tactics}/{normdTactics} x " +
                        $"{source.Combat.Constants.Visibility.SpotterTacticsMultiplier} = spottingTacticsMulti:{spottingTacticsMultipler}");
                }
            }
            float spotterDistanceAbsolute = source.SpotterDistanceAbsolute;
            float num2 = 0f;
            return spottingTacticsMultipler + spotterDistanceAbsolute + num2;
        }
        // Copy of LineOfSight::GetAllSpotterAbsolutes

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

        // Copy of LineOfSight::GetVisibilityLevelForTactics
        public static VisibilityLevel VisibilityLevelByTactics(int tacticsSkill) {
            if (tacticsSkill >= 7) {
                return VisibilityLevel.Blip4Maximum;
            }
            if (tacticsSkill >= 4) {
                return VisibilityLevel.Blip1Type;
            }
            return VisibilityLevel.Blip0Minimum;
        }

    }

}
