
using BattleTech;
using System.Collections.Generic;

namespace LowVisibility.Object {
 
    public class Locks {
        public string sourceGUID;
        public string targetGUID;
        public bool hasLineOfSight;
        public SensorScanType sensorLock;

        public Locks() { }

        public Locks(AbstractActor source, ICombatant target) {
            this.sourceGUID = source.GUID;
            this.targetGUID = target.GUID;
            this.hasLineOfSight = false;
            this.sensorLock = SensorScanType.NoInfo;
        }

        public Locks(AbstractActor source, ICombatant target, bool hasLineOfSight, SensorScanType sensorLock) {
            this.sourceGUID = source.GUID;
            this.targetGUID = target.GUID;
            this.hasLineOfSight = hasLineOfSight;
            this.sensorLock = sensorLock;
        }

        public Locks(Locks source) {
            this.sourceGUID = source.sourceGUID;
            this.targetGUID = source.targetGUID;
            this.hasLineOfSight= source.hasLineOfSight;
            this.sensorLock = source.sensorLock;
        }

        public override string ToString() {
            return $"hasLineOfSight:{hasLineOfSight}, sensorLockLevel:{sensorLock}";
        }
    }

    public class AggregateLocks {
        public string targetGUID;
        public SensorScanType sensorLock;

        public AggregateLocks() { }

        public static AggregateLocks Aggregate(List<Locks> allLocks) {
            AggregateLocks aggregatedLocks = new AggregateLocks();
            foreach (Locks locks in allLocks) {
                if (locks.sensorLock > aggregatedLocks.sensorLock) {
                    aggregatedLocks.sensorLock = locks.sensorLock;
                    aggregatedLocks.targetGUID = locks.targetGUID;
                }
            }
            return aggregatedLocks;
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
