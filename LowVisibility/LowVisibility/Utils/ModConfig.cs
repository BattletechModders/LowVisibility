
namespace LowVisibility {
    public class ModConfig {
        // If true, extra logging will be used
        public bool Debug = false;

        // Short, medium, long
        public float[] UnknownSensorRanges  = new float[] { 150, 250, 350 };
        public float[] TurretSensorRanges   = new float[] { 240, 480, 720 };
        public float[] VehicleSensorRanges  = new float[] { 270, 350, 450 };
        public float[] MechSensorRanges     = new float[] { 300, 450, 550 };

        // The range from which you can identify a unit using visuals only
        public float VisualIDRange = 3.0f * 30;

        // The penalty applied when the attacker only has LoS to the target (but not detection)
        public float NoSensorLockAttackPenalty = 2.0f;

        // The penalty applied when the attacker only has detection to the target (but not Los)
        public float NoVisualLockAttackPenalty = 2.0f;

        public override string ToString() {
            return $"debug:{Debug}, VisualIDRange:{VisualIDRange}, SensorsOnlyAttackPenalty:{NoSensorLockAttackPenalty}, VisualOnlyAttackPenalty:{NoVisualLockAttackPenalty} " +
                $"MechSensorRanges:{MechSensorRanges[0]}/{MechSensorRanges[1]}/{MechSensorRanges[2]} " +
                $"VehicleSensorRanges:{VehicleSensorRanges[0]}/{VehicleSensorRanges[1]}/{VehicleSensorRanges[2]} " +
                $"TurretSensorRanges:{TurretSensorRanges[0]}/{TurretSensorRanges[1]}/{TurretSensorRanges[2]} " +
                $"UnknownSensorRanges:{UnknownSensorRanges[0]}/{UnknownSensorRanges[1]}/{UnknownSensorRanges[2]} ";
        }
    }
}
