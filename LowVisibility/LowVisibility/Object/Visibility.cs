using System.Collections.Generic;

namespace LowVisibility.Object
{

    public class Locks
    {
        public string sourceGUID;
        public string targetGUID;
        public bool hasLineOfSight;
        public SensorScanType sensorLock;

        public Locks() { }

        public Locks(AbstractActor source, ICombatant target)
        {
            this.sourceGUID = source.GUID;
            this.targetGUID = target.GUID;
            this.hasLineOfSight = false;
            this.sensorLock = SensorScanType.NoInfo;
        }

        public Locks(AbstractActor source, ICombatant target, bool hasLineOfSight, SensorScanType sensorLock)
        {
            this.sourceGUID = source.GUID;
            this.targetGUID = target.GUID;
            this.hasLineOfSight = hasLineOfSight;
            this.sensorLock = sensorLock;
        }

        public Locks(Locks source)
        {
            this.sourceGUID = source.sourceGUID;
            this.targetGUID = source.targetGUID;
            this.hasLineOfSight = source.hasLineOfSight;
            this.sensorLock = source.sensorLock;
        }

        public override string ToString()
        {
            return $"hasLineOfSight:{hasLineOfSight}, sensorLockLevel:{sensorLock}";
        }
    }

    public class AggregateLocks
    {
        public string targetGUID;
        public SensorScanType sensorLock;

        public AggregateLocks() { }

        public static AggregateLocks Aggregate(List<Locks> allLocks)
        {
            AggregateLocks aggregatedLocks = new AggregateLocks();
            foreach (Locks locks in allLocks)
            {
                if (locks.sensorLock > aggregatedLocks.sensorLock)
                {
                    aggregatedLocks.sensorLock = locks.sensorLock;
                    aggregatedLocks.targetGUID = locks.targetGUID;
                }
            }
            return aggregatedLocks;
        }
    }

    public enum SensorScanType
    {
        NoInfo,
        LocationAndType,
        ArmorAndWeaponType,
        StructAndWeaponID,
        AllInformation

    }

    static class SensorLockTypeExtensions
    {
        public static string Label(this SensorScanType level)
        {
            switch (level)
            {
                case SensorScanType.NoInfo:
                    return new Localize.Text(Mod.LocalizedText.StatusPanel[ModText.LT_DETAILS_NONE]).ToString();
                case SensorScanType.LocationAndType:
                    return new Localize.Text(Mod.LocalizedText.StatusPanel[ModText.LT_DETAILS_LOCATION_AND_TYPE]).ToString();
                case SensorScanType.ArmorAndWeaponType:
                    return new Localize.Text(Mod.LocalizedText.StatusPanel[ModText.LT_DETAILS_ARMOR_AND_WEAPON_TYPE]).ToString();
                case SensorScanType.StructAndWeaponID:
                    return new Localize.Text(Mod.LocalizedText.StatusPanel[ModText.LT_DETAILS_STRUCT_AND_WEAPON_ID]).ToString();
                case SensorScanType.AllInformation:
                    return new Localize.Text(Mod.LocalizedText.StatusPanel[ModText.LT_DETAILS_ALL_INFO]).ToString();
                default:
                    return new Localize.Text(Mod.LocalizedText.StatusPanel[ModText.LT_DETAILS_UNKNOWN]).ToString();
            }
        }

        public static VisibilityLevel Visibility(this SensorScanType level)
        {
            switch (level)
            {
                case SensorScanType.AllInformation:
                case SensorScanType.StructAndWeaponID:
                    return VisibilityLevel.Blip4Maximum;
                case SensorScanType.ArmorAndWeaponType:
                    return VisibilityLevel.Blip1Type;
                case SensorScanType.LocationAndType:
                    return VisibilityLevel.Blip0Minimum;
                case SensorScanType.NoInfo:
                default:
                    return VisibilityLevel.None;
            }
        }

    }

    public static class SensorScanTypeHelper
    {
        public static SensorScanType DetectionLevelForCheck(int checkResult)
        {
            SensorScanType level = SensorScanType.NoInfo;
            if (checkResult >= 9) level = SensorScanType.AllInformation;
            else if (checkResult >= 6) level = SensorScanType.StructAndWeaponID;
            else if (checkResult >= 3) level = SensorScanType.ArmorAndWeaponType;
            else if (checkResult >= 0) level = SensorScanType.LocationAndType;

            Mod.Log.Trace?.Write($" For EW check result: {checkResult} detectionLevel is: {level} ");
            return level;
        }
    }

}
