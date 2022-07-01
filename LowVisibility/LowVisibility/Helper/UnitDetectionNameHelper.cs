using BattleTech;
using Localize;
using LowVisibility.Object;

namespace LowVisibility.Helper
{
    public static class UnitDetectionNameHelper
    {
        /*
            Helper method used to find the label text for any enemy turrets based on the visiblity and senors levels.

            chassisName -> ICombatant.UnitName = TurretDef.Chassis.Description.Name -> ArrowIV Chassis
            fullname -> ICombatant.NickName = TurretDef.Description.Name -> Arrow IV Turret
        */
        public static string GetTurretName(VisibilityLevel visLevel, SensorScanType sensorScanType, string fullName, string chassisName)
        {
            return GetEnemyUnitName(visLevel, sensorScanType, null, fullName, null, chassisName, ModText.LT_UNIT_TYPE_TURRET);
        }

        /*
            Helper method used to find the label text for any enemy vehicles based on the visiblity and senors levels.

            chassisName -> ICombatant.UnitName = VehicleDef.Chassis.Description.Name -> Carrier / Vargr APC
            fullname -> ICombatant.NickName = VehicleDef.Description.Name -> AC/2 Carrier / Vargr APC
        */
        public static string GetVehicleName(VisibilityLevel visLevel, SensorScanType sensorScanType, string fullName, string chassisName)
        {
            return GetEnemyUnitName(visLevel, sensorScanType, null, fullName, null, chassisName, ModText.LT_UNIT_TYPE_VEHICLE);
        }

        /*
           Helper method used to find the label text for any enemy mechs based on the visiblity and senors levels.

           Parameters:
            chassisName -> Mech.UnitName = MechDef.Chassis.Description.Name -> Shadow Hawk / Atlas / Marauder                 
                    (The name of the base chassis, even if customized chassis (such as RogueOmnis))

            partialName -> Mech.NickName = MechDef.Description.Name -> Shadow Hawk SHD-2D / Atlas AS7-D / Marauder ANU-O      
                    (Partial name, most cases chassis and variant name combined, but for some elite mechs can be "less precise" to trick the player which mech it is)

            fullname -> Mech.NickName = MechDef.Description.UIName -> Shadow Hawk SHD-2D / Atlas AS7-D Danielle / Anand ANU-O
                    (Full name, will almost always display the full actual name, and if a hero/elite mech the chassis name is replaced by its custom name. ONly exception is LA's hidden nasty surprises, such as Nuke mechs)
        */
        public static string GetEnemyMechName(VisibilityLevel visLevel, SensorScanType sensorScanType, string typeName, string fullName, string partialName, string chassisName)
        {
            return GetEnemyUnitName(visLevel, sensorScanType, typeName, fullName, partialName, chassisName, ModText.LT_UNIT_TYPE_MECH);
        }

        /*
           Helper method used to find the label text for any non-hostile mechs. 
           If a custom name has been set by player, the fullName will be null and displayName used instead.

            displayName -> Mech.DisplayName -> Custom name of the mech

            fullName -> Mech.NickName = MechDef.Description.UIName -> Shadow Hawk SHD-2D / Atlas AS7-D Danielle / Anand ANU-O
                (Full name, will almost always display the full actual name, and if a hero/elite mech the chassis name is replaced by its custom name. ONly exception is LA's hidden nasty surprises, such as Nuke mechs)
        */
        public static string GetNonHostileMechName(string fullName, string displayName)
        {
            string name;
            if (string.IsNullOrEmpty(fullName))
            {
                name = displayName;
            }
            else
            {
                name = fullName;
            }
            Mod.Log.Debug?.Write($"GetNonHostileMechName - name:({name}) from displayName: ({displayName}) fullName:({fullName})");
            return name;
        }


        private static string GetEnemyUnitName(VisibilityLevel visLevel, SensorScanType sensorScanType, string typeName, string fullName, string partialName, string chassisName, string unitTypeKey)
        {
            string name = "???";

            if (visLevel >= VisibilityLevel.Blip0Minimum)
            {
                Mod.Log.Trace?.Write($"GetEnemyUnitName - visLevel {visLevel} >= VisibilityLevel.Blip0Minimum");
                if (sensorScanType == SensorScanType.LocationAndType)
                {
                    Mod.Log.Trace?.Write($"GetEnemyUnitName - sensorScanType {sensorScanType} == SensorScanType.LocationAndType");
                    name = GetTypeName(typeName, unitTypeKey);
                }
                else if (sensorScanType == SensorScanType.ArmorAndWeaponType)
                {
                    Mod.Log.Trace?.Write($"GetEnemyUnitName - sensorScanType {sensorScanType} == SensorScanType.ArmorAndWeaponType");
                    name = chassisName;
                }
                else if (sensorScanType == SensorScanType.StructAndWeaponID)
                {
                    Mod.Log.Trace?.Write($"GetEnemyUnitName - sensorScanType {sensorScanType} == SensorScanType.StructAndWeaponID");
                    name = partialName ?? fullName;
                }
                else if (sensorScanType == SensorScanType.AllInformation)
                {
                    Mod.Log.Trace?.Write($"GetEnemyUnitName - sensorScanType {sensorScanType} == SensorScanType.AllInformation");
                    name = fullName;
                }
            }
            Mod.Log.Debug?.Write($"GetEnemyUnitName - name:({name}) from VisibilityLevel: ({visLevel}) SensorScanType: ({sensorScanType}) fullName: ({fullName}) partialName:({partialName}) chassisName: ({chassisName}) UnitType: ({unitTypeKey})");
            return name;
        }

        private static string GetTypeName(string typeName, string labelKey)
        {
            return string.IsNullOrEmpty(typeName) ? Mod.LocalizedText.StatusPanel[labelKey] : typeName;
        }
    }

    public static class PilotNameHelper
    {
        public static string GetEnemyPilotName(VisibilityLevel visLevel, SensorScanType scanType, AbstractActor abstractActor)
        {
            string pilotName = "";

            if (visLevel >= VisibilityLevel.Blip0Minimum)
            {
                if (scanType >= SensorScanType.AllInformation) pilotName = abstractActor.GetPilot().Name;
            }
            return pilotName;
        }
    }
}
