
namespace LowVisibility.Object {

    // TODO: Update visionLockType to use same detectionLevel scheme as below
    public enum VisionLockType {
        None,
        Silhouette,
        VisualID
    }

    public enum DetectionLevel {
        NoInfo,
        Location,
        Type,
        Silhouette,
        Vector,
        SurfaceScan,
        SurfaceAnalysis,
        WeaponAnalysis,
        StructureAnalysis,
        DeepScan,
        DentalRecords
    }

    public class LockState {
        public string sourceGUID;
        public string targetGUID;
        public VisionLockType visionLockLevel;
        public DetectionLevel sensorLockLevel;

        public LockState() { }

        public LockState(LockState source) {
            this.sourceGUID = source.sourceGUID;
            this.targetGUID = source.targetGUID;
            this.visionLockLevel = source.visionLockLevel;
            this.sensorLockLevel = source.sensorLockLevel;
        }

        public override string ToString() {
            return $"visionLockLevel:{visionLockLevel}, sensorLockLevel:{sensorLockLevel}";
        }
    }


}
