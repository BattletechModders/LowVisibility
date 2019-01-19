
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

        // When an attacker only has visual lock to the target, apply the peanlty for each N hexes away the target is
        public int VisionOnlyRangeStep = 3;
        public int VisionOnlyPenalty = -1;
        public float VisionOnlyCriticalPenalty = 0.0f;

        // The applied when the attacker has sensor but not visual lock to a target. Multiplies the range penalty.
        public float SensorsOnlyPenalty = -2;
        // TODO: No sensor lock reduces critical / called shot penalties
        public float SensorsOnlyCriticalPenalty = 0.0f;

        // The inflection point of the probability distribution function.
        public int ProbabilitySigma = 4;
        // The inflection point of the probability distribution function.
        public int ProbabilityMu = -1;

        public override string ToString() {
            return $"Debug:{Debug}, Trace:{Trace}, FirstTurnForceFailedChecks:{FirstTurnForceFailedChecks}, MultipleJammerPenalty:{MultipleECMSourceModifier}," +
                $"SensorRanges= Mech:{SensorRangeMechType} Vehicle:{SensorRangeVehicleType} Turret:{SensorRangeTurretType} UnknownType:{SensorRangeUnknownType}" +
                $"VisionRangeBaseDaylight:{VisionRangeBaseDaylight} VisionRangeBaseDimlight:{VisionRangeBaseDimlight} VisionRangeBaseNight:{VisionRangeBaseNight}" +
                $"VisionRangeMultiRainSnow:{VisionRangeMultiRainSnow} VisionRangeMultiLightFog:{VisionRangeMultiLightFog} VisionRangeMultiHeavyFog:{VisionRangeMultiHeavyFog}" +
                $"VisionRangeMinimum:{VisionRangeMinimum} SensorRangeMinimum:{SensorRangeMinimum}, VisualIDRange:{VisualIDRange} " +
                $"VisionOnlyRangeStep:{VisionOnlyRangeStep}, VisionOnlyPenalty:{VisionOnlyPenalty} SensorsOnlyPenalty:{SensorsOnlyPenalty} " +
                $"VisionOnlyCriticalPenalty:{VisionOnlyCriticalPenalty}, SensorsOnlyCriticalPenalty:{SensorsOnlyCriticalPenalty} " +
                $"ProbabilitySigma:{ProbabilitySigma}, ProbabilityMu:{ProbabilityMu}";



        }
    }
}
