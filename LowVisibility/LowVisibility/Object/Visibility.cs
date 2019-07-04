
using BattleTech;
using System.Collections.Generic;

namespace LowVisibility.Object {
 
    public class Locks {
        public string sourceGUID;
        public string targetGUID;
        public VisualScanType visualLock;
        public SensorScanType sensorLock;

        public Locks() { }

        public Locks(AbstractActor source, ICombatant target) {
            this.sourceGUID = source.GUID;
            this.targetGUID = target.GUID;
            this.visualLock = VisualScanType.None;
            this.sensorLock = SensorScanType.NoInfo;
        }

        public Locks(AbstractActor source, ICombatant target, VisualScanType visualLock, SensorScanType sensorLock) {
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

    public class AggregateLocks {
        public string targetGUID;
        public VisualScanType visualLock;
        public SensorScanType sensorLock;

        public AggregateLocks() { }

        public static AggregateLocks Aggregate(List<Locks> allLocks) {
            AggregateLocks aggregatedLocks = new AggregateLocks();
            foreach (Locks locks in allLocks) {
                if (locks.visualLock > aggregatedLocks.visualLock) {
                    aggregatedLocks.visualLock = locks.visualLock;
                    aggregatedLocks.targetGUID = locks.targetGUID;
                }
                if (locks.sensorLock > aggregatedLocks.sensorLock) {
                    aggregatedLocks.sensorLock = locks.sensorLock;
                    aggregatedLocks.targetGUID = locks.targetGUID;
                }
            }
            return aggregatedLocks;
        }
    }

    // TODO: Update visionLockType to use same detectionLevel scheme as below
    public enum VisualScanType {
        None,
        Silhouette,
        Chassis,
        VisualID
    }

    static class VisualLockTypeExtensions {
        public static string Label(this VisualScanType visualLock) {
            switch (visualLock) {
                case VisualScanType.Silhouette:
                    return "Silhouette";
                case VisualScanType.VisualID:
                    return "Visual ID";
                case VisualScanType.None:
                default:
                    return "No Lock";                
            }
        }

        public static VisibilityLevel Visibility(this VisualScanType level) {
            switch (level) {
                case VisualScanType.Silhouette:
                case VisualScanType.VisualID:
                    return VisibilityLevel.LOSFull;
                case VisualScanType.None:
                default:
                    return VisibilityLevel.None;
            }
        }

    }

    public enum SensorScanType {
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
        public static string Label(this SensorScanType level) {
            switch (level) {
                case SensorScanType.NoInfo:
                    return "No Info";
                case SensorScanType.Location:
                    return "Location";
                case SensorScanType.Type:
                    return "Type";
                case SensorScanType.Silhouette:
                    return "Silhouettte";
                case SensorScanType.Vector:
                    return "Vector";
                case SensorScanType.SurfaceScan:
                    return "SurfaceScan";
                case SensorScanType.SurfaceAnalysis:
                    return "SurfaceAnalysis";
                case SensorScanType.WeaponAnalysis:
                    return "WeaponsAnalysis";
                case SensorScanType.StructureAnalysis:
                    return "StructureAnalysis";
                case SensorScanType.DeepScan:
                    return "DeepScan";
                case SensorScanType.DentalRecords:
                    return "DentalRecords";
                default:
                    return "Unknown";
            }
        }

        public static VisibilityLevel Visibility(this SensorScanType level) {
            switch (level) {
                case SensorScanType.Location:
                case SensorScanType.Type:
                    return VisibilityLevel.BlobSmall;
                case SensorScanType.Silhouette:                    
                case SensorScanType.Vector:
                    return VisibilityLevel.Blip0Minimum;
                case SensorScanType.SurfaceScan:
                case SensorScanType.SurfaceAnalysis:
                case SensorScanType.WeaponAnalysis:
                case SensorScanType.StructureAnalysis:
                    return VisibilityLevel.Blip1Type;
                case SensorScanType.DeepScan:
                case SensorScanType.DentalRecords:
                    return VisibilityLevel.Blip4Maximum;
                case SensorScanType.NoInfo:
                default:
                    return VisibilityLevel.None;
            }
        }

    }
   
}
