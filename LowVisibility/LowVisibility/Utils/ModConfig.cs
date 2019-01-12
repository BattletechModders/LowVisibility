
namespace LowVisibility {
    public class ModConfig {
        // If true, extra logging will be used
        public bool Debug = false;

        // Extreme levels of logging
        public bool Trace = false;

        // How much each level of obscurement reduces map visibility
        // TODO: Implement
        public float ObscurementMultiplier = 3.0f;

        public int MultipleJammerPenalty = 1;

        // The base range (in hexes) for a unit's sensors
        public float SensorRangeMechType = 10;
        public float SensorRangeVehicleType  = 8;
        public float SensorRangeTurretType = 12;
        public float SensorRangeUnknownType = 6;

        // The range (in hexes) from which you can identify some elements of a unit
        public float VisualIDRange = 5;

        // The applied when the attacker has visual but not sensor lock to a target. Multiplies the range penalty.
        public float NoSensorLockRangePenaltyMulti = 0.5f;
        // The applied when the attacker has sensor but not visual lock to a target. Multiplies the range penalty.
        public float NoVisualLockRangePenaltyMulti = 1.0f;

        // TODO: No sensor lock reduces critical / called shot penalties
        public float NoSensorLockCriticalMultiPenalty = 0.0f;
        public float NoVisualLockCriticalMultiPenalty = 0.0f;

        public override string ToString() {
            return $"debug:{Debug}, VisualIDRange:{VisualIDRange}, " +
                $"SensorsOnlyAttackPenalty:{NoSensorLockRangePenaltyMulti}, VisualOnlyAtackPenalty:{NoVisualLockRangePenaltyMulti} " +
                $"SensorRanges= Mech:{SensorRangeMechType} Vehicle:{SensorRangeVehicleType} " +
                $"Turret:{SensorRangeTurretType} UnknownType:{SensorRangeUnknownType}";
                
        }
    }
}
