
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
                    return new Localize.Text(Mod.Config.LocalizedText[ModConfig.LT_DETAILS_NONE]).ToString();
                case SensorScanType.Location:
                    return new Localize.Text(Mod.Config.LocalizedText[ModConfig.LT_DETAILS_LOCATION]).ToString();
                case SensorScanType.Type:
                    return new Localize.Text(Mod.Config.LocalizedText[ModConfig.LT_DETAILS_TYPE]).ToString();
                case SensorScanType.Silhouette:
                    return new Localize.Text(Mod.Config.LocalizedText[ModConfig.LT_DETAILS_SILHOUETTE]).ToString();
                case SensorScanType.Vector:
                    return new Localize.Text(Mod.Config.LocalizedText[ModConfig.LT_DETAILS_VECTOR]).ToString();
                case SensorScanType.SurfaceScan:
                    return new Localize.Text(Mod.Config.LocalizedText[ModConfig.LT_DETAILS_SURFACE_SCAN]).ToString();
                case SensorScanType.SurfaceAnalysis:
                    return new Localize.Text(Mod.Config.LocalizedText[ModConfig.LT_DETAILS_SURFACE_ANALYZE]).ToString();
                case SensorScanType.WeaponAnalysis:
                    return new Localize.Text(Mod.Config.LocalizedText[ModConfig.LT_DETAILS_WEAPON_ANALYZE]).ToString();
                case SensorScanType.StructureAnalysis:
                    return new Localize.Text(Mod.Config.LocalizedText[ModConfig.LT_DETAILS_STRUCTURE_ANALYZE]).ToString();
                case SensorScanType.DeepScan:
                    return new Localize.Text(Mod.Config.LocalizedText[ModConfig.LT_DETAILS_DEEP_SCAN]).ToString();
                case SensorScanType.DentalRecords:
                    return new Localize.Text(Mod.Config.LocalizedText[ModConfig.LT_DETAILS_PILOT]).ToString();
                default:
                    return new Localize.Text(Mod.Config.LocalizedText[ModConfig.LT_DETAILS_UNKNOWN]).ToString();
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
