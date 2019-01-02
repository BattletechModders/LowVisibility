
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

    }
}
