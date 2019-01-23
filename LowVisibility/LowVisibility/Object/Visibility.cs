
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

    static class DetectionLevelExtensions {
        public static string Label(this DetectionLevel level) {
            switch (level) {
                case DetectionLevel.NoInfo:
                    return "No Info";
                case DetectionLevel.Location:
                    return "Location";
                case DetectionLevel.Type:
                    return "Type";
                case DetectionLevel.Silhouette:
                    return "Silhouettte";
                case DetectionLevel.Vector:
                    return "Vector";
                case DetectionLevel.SurfaceScan:
                    return "SurfaceScan";
                case DetectionLevel.SurfaceAnalysis:
                    return "SurfaceAnalysis";
                case DetectionLevel.WeaponAnalysis:
                    return "WeaponsAnalysis";
                case DetectionLevel.StructureAnalysis:
                    return "StructureAnalysis";
                case DetectionLevel.DeepScan:
                    return "DeepScan";
                case DetectionLevel.DentalRecords:
                    return "DentalRecords";
                default:
                    return "Unknown";

            }
        }      
                
    }
   
}
