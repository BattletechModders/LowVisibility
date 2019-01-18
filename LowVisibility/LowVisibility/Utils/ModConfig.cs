
namespace LowVisibility {
    public class ModConfig {
        // If true, extra logging will be used
        public bool Debug = false;
        public bool Trace = false;

        public bool FirstTurnForceFailedChecks = true;

        public int MultipleECMSourceModifier = 1;

        // The base range (in hexes) for a unit's sensors
        public int SensorRangeMechType = 12;
        public int SensorRangeVehicleType  = 9;
        public int SensorRangeTurretType = 15;
        public int SensorRangeUnknownType = 6;

        // The base range (in hexes) for a unit's vision
        public int VisionRangeBaseDaylight = 15;
        public int VisionRangeBaseDimlight = 11;
        public int VisionRangeBaseNight = 7;

        // The multiplier used for weather effects
        public float VisionRangeMultiRainSnow = 0.8f;
        public float VisionRangeMultiLightFog = 0.66f;
        public float VisionRangeMultiHeavyFog = 0.33f;

        // The range (in hexes) from which you can identify some elements of a unit
        public int VisualIDRange = 5;

        // The minium range for vision, no matter the circumstances
        public int VisionRangeMinimum = 2;

        // The minium range for sensors, no matter the circumstances
        public int SensorRangeMinimum = 6;

        // The applied when the attacker has visual but not sensor lock to a target. Multiplies the range penalty.
        public float NoSensorLockRangePenaltyMulti = 0.5f;
        // The applied when the attacker has sensor but not visual lock to a target. Multiplies the range penalty.
        public float NoVisualLockRangePenaltyMulti = 1.0f;

        // TODO: No sensor lock reduces critical / called shot penalties
        public float NoSensorLockCriticalMultiPenalty = 0.0f;
        public float NoVisualLockCriticalMultiPenalty = 0.0f;

        // The inflection point of the probability distribution function.
        public int ProbabilitySigma = 4;
        // The inflection point of the probability distribution function.
        public int ProbabilityMu = -2;

        public override string ToString() {
            return $"Debug:{Debug}, Trace:{Trace}, FirstTurnForceFailedChecks:{FirstTurnForceFailedChecks}, MultipleJammerPenalty:{MultipleECMSourceModifier}," +
                $"SensorRanges= Mech:{SensorRangeMechType} Vehicle:{SensorRangeVehicleType} Turret:{SensorRangeTurretType} UnknownType:{SensorRangeUnknownType}" +
                $"VisionRangeBaseDaylight:{VisionRangeBaseDaylight} VisionRangeBaseDimlight:{VisionRangeBaseDimlight} VisionRangeBaseNight:{VisionRangeBaseNight}" +
                $"VisionRangeMultiRainSnow:{VisionRangeMultiRainSnow} VisionRangeMultiLightFog:{VisionRangeMultiLightFog} VisionRangeMultiHeavyFog:{VisionRangeMultiHeavyFog}" +
                $"VisionRangeMinimum:{VisionRangeMinimum} SensorRangeMinimum:{SensorRangeMinimum}, VisualIDRange:{VisualIDRange} " +
                $"NoSensorLockRangePenaltyMulti:{NoSensorLockRangePenaltyMulti}, NoVisualLockRangePenaltyMulti:{NoVisualLockRangePenaltyMulti} " +
                $"NoSensorLockCriticalMultiPenalty:{NoSensorLockCriticalMultiPenalty}, NoVisualLockCriticalMultiPenalty:{NoVisualLockCriticalMultiPenalty} " +
                $"ProbabilitySigma:{ProbabilitySigma}, ProbabilityMu:{ProbabilityMu}";



        }
    }
}
