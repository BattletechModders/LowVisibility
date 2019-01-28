
using BattleTech;

namespace LowVisibility.Object {
 
    public class Locks {
        public string sourceGUID;
        public string targetGUID;
        public VisualLockType visualLock;
        public SensorLockType sensorLock;

        public Locks() { }

        public Locks(AbstractActor source, ICombatant target) {
            this.sourceGUID = source.GUID;
            this.targetGUID = target.GUID;
            this.visualLock = VisualLockType.None;
            this.sensorLock = SensorLockType.NoInfo;
        }

        public Locks(AbstractActor source, ICombatant target, VisualLockType visualLock, SensorLockType sensorLock) {
            this.sourceGUID = source.GUID;
            this.targetGUID = target.GUID;
            this.visualLock = visualLock;
            this.sensorLock = sensorLock;
        }

        public Locks(Locks source) {
            this.sourceGUID = source.sourceGUID;
            this.targetGUID = source.targetGUID;
            this.visualLock = source.visualLock;
            this.sensorLock = source.sensorLock;
        }

        public override string ToString() {
            return $"visionLockLevel:{visualLock}, sensorLockLevel:{sensorLock}";
        }
    }

    // TODO: Update visionLockType to use same detectionLevel scheme as below
    public enum VisualLockType {
        None,
        Silhouette,
        VisualScan
    }

    static class VisualLockTypeExtensions {
        public static string Label(this VisualLockType visualLock) {
            switch (visualLock) {
                case VisualLockType.Silhouette:
                    return "Silhouette";
                case VisualLockType.VisualScan:
                    return "Visual Scan";
                case VisualLockType.None:
                default:
                    return "No Lock";                
            }
        }

        public static VisibilityLevel Visibility(this VisualLockType level) {
            switch (level) {
                case VisualLockType.Silhouette:
                case VisualLockType.VisualScan:
                    return VisibilityLevel.LOSFull;
                case VisualLockType.None:
                default:
                    return VisibilityLevel.None;
            }
        }

    }

    public enum SensorLockType {
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

    static class SensorLockTypeExtensions {
        public static string Label(this SensorLockType level) {
            switch (level) {
                case SensorLockType.NoInfo:
                    return "No Info";
                case SensorLockType.Location:
                    return "Location";
                case SensorLockType.Type:
                    return "Type";
                case SensorLockType.Silhouette:
                    return "Silhouettte";
                case SensorLockType.Vector:
                    return "Vector";
                case SensorLockType.SurfaceScan:
                    return "SurfaceScan";
                case SensorLockType.SurfaceAnalysis:
                    return "SurfaceAnalysis";
                case SensorLockType.WeaponAnalysis:
                    return "WeaponsAnalysis";
                case SensorLockType.StructureAnalysis:
                    return "StructureAnalysis";
                case SensorLockType.DeepScan:
                    return "DeepScan";
                case SensorLockType.DentalRecords:
                    return "DentalRecords";
                default:
                    return "Unknown";
            }
        }

        public static VisibilityLevel Visibility(this SensorLockType level) {
            switch (level) {
                case SensorLockType.Location:
                    return VisibilityLevel.Blip0Minimum;
                case SensorLockType.Type:
                case SensorLockType.Silhouette:
                case SensorLockType.Vector:
                    return VisibilityLevel.Blip1Type;
                case SensorLockType.SurfaceScan:
                case SensorLockType.SurfaceAnalysis:
                case SensorLockType.WeaponAnalysis:
                case SensorLockType.StructureAnalysis:
                case SensorLockType.DeepScan:
                case SensorLockType.DentalRecords:
                    return VisibilityLevel.Blip4Maximum;
                case SensorLockType.NoInfo:
                default:
                    return VisibilityLevel.None;
            }
        }

    }
   
}
