using BattleTech;
using LowVisibility.Object;

namespace LowVisibility.Helper {
    public static class ActorHelper {
        
        // Determine an actor's sensor range, plus our special additions
        public static float GetSensorsRange(AbstractActor source) {
            // Determine type
            float rangeForType = 0.0f;
            if (source.GetType() == typeof(Mech)) { rangeForType = LowVisibility.Config.SensorRangeMechType;  }
            else if (source.GetType() == typeof(Vehicle)) { rangeForType = LowVisibility.Config.SensorRangeVehicleType; } 
            else if (source.GetType() == typeof(Turret)) { rangeForType = LowVisibility.Config.SensorRangeTurretType; }
            else { rangeForType = LowVisibility.Config.SensorRangeUnknownType; }

            // Add multipliers and absolute bonuses
            float rangeMulti = GetAllSensorRangeMultipliers(source);
            float rangeMod = GetAllSensorRangeAbsolutes(source);

            StaticEWState staticState = State.GetStaticState(source);
            DynamicEWState dynamicState = State.GetDynamicState(source);
            float checkMulti = 1.0f + ((dynamicState.rangeCheck + staticState.tacticsBonus)/ 10.0f);

            float sensorsRange = ((rangeForType * 30) * rangeMulti + rangeMod) * checkMulti;
            if (sensorsRange < LowVisibility.Config.SensorRangeMinimum) { sensorsRange = LowVisibility.Config.SensorRangeMinimum; }

            LowVisibility.Logger.LogIfTrace($"{CombatantHelper.Label(source)} has sensorsRange:{sensorsRange} = " +
                $"((rangeForType:{rangeForType} * 30.0) * rangeMulti:{rangeMulti} + rangeMod:{rangeMod}) * checkMulti:{checkMulti}");
            return sensorsRange;
        }

        public static float GetVisualLockRange(AbstractActor actor) {
            return GetActorModifiedVisualRange(State.GetMapVisionRange(), actor);
        }

        public static float GetVisualScanRange(AbstractActor actor) {
            return GetActorModifiedVisualRange(State.GetVisualIDRange(), actor);
        }

        private static float GetActorModifiedVisualRange(float visualRange, AbstractActor source) {

            float modifiedRange = visualRange;
            // Can't VisualID when shutdown
            if (source.IsShutDown) {
                modifiedRange = 0f;
            } else if (source.IsProne) {
                modifiedRange = visualRange * source.Combat.Constants.Visibility.ProneSpottingDistanceMultiplier;
            } else {
                float allSpotterMultipliers = GetAllSpotterMultipliers(source);
                float allSpotterAbsolutes = GetAllSpotterAbsolutes(source);
                modifiedRange = visualRange * allSpotterMultipliers + allSpotterAbsolutes;
                LowVisibility.Logger.LogIfTrace($" -- source:{CombatantHelper.Label(source)} with " +
                    $"spotterMulti:{allSpotterMultipliers} spotterAbsolutes:{allSpotterAbsolutes} " +
                    $"and visualRange:{visualRange} has modifiedRange:{modifiedRange}");
            }            
            
            return modifiedRange;
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
                    StaticEWState staticState = State.GetStaticState(source);                    
                    spottingTacticsMultipler = (float)staticState.tacticsBonus * source.Combat.Constants.Visibility.SpotterTacticsMultiplier;
                    //LowVisibility.Logger.LogIfDebug($"  actor:{CombatantHelper.Label(source)} with tactics:{pilot.Tactics}/{normdTactics} x " +
                    //    $"{source.Combat.Constants.Visibility.SpotterTacticsMultiplier} = spottingTacticsMulti:{spottingTacticsMultipler}");
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
