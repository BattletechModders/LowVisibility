
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
                    return "No Information";
                case DetectionLevel.Location:
                    return "Location";
                case DetectionLevel.Type:
                    return "Location, Type";
                case DetectionLevel.Silhouette:
                    return "Location, Chassis";
                case DetectionLevel.Vector:
                    return "Location, Chassis, Evasion";
                case DetectionLevel.SurfaceScan:
                    return "Location, Chassis, Evasion, Armor/Struct";
                case DetectionLevel.SurfaceAnalysis:
                    return "Location, Chassis, Evasion, Armor/Struct, Weapon Types";
                case DetectionLevel.WeaponAnalysis:
                    return "Location, Chassis, Evasion, Armor/Struct, Weapons";
                case DetectionLevel.StructureAnalysis:
                    return "Location, Chassis, Evasion, Armor/Struct, Weapons, Heat & Stability";
                case DetectionLevel.DeepScan:
                    return "All Mech Info, No Pilot Info";
                case DetectionLevel.DentalRecords:
                    return "All Mech and Pilot Info";
                default:
                    return "Unknown";

            }
        }         
    }
   
}
