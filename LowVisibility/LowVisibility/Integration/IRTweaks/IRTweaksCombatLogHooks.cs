using IRBTModUtils;
using LowVisibility.Helper;
using LowVisibility.Integration.IRTweaks;
using LowVisibility.Object;

namespace LowVisibility.Integration
{

    public static class CombatLogIntegration
    {

        public static int NONE = 0;
        public static int SIMPLE = 1;
        public static int REMEMBER = 2;

        private static readonly string PILOT_KEY_SUFFIX = "_pilot";

        public static string LowVisibilityCombatLogUnitNameModifier(string name, AbstractActor abstractActor)
        {
            if (abstractActor == null) { return name; }

            IRTweaksHelper.LogIfEnabled($"Unit GUID {abstractActor.GUID}: Starting name check");


            if (abstractActor.Combat.HostilityMatrix.IsLocalPlayerEnemy(abstractActor.team.GUID))
            {
                IRTweaksHelper.LogIfEnabled($"Unit GUID {abstractActor.GUID}: is hostile");

                VisibilityLevel visLevel = abstractActor.Combat.LocalPlayerTeam.VisibilityToTarget(abstractActor);
                SensorScanType scanType = GetSensorScanType(abstractActor);

                if (Mod.Config.Integrations.IRTweaks.CombatLogNames == CombatLogIntegration.REMEMBER && CombatLogNameCacheHelper.ContainsEqualOrBetterName(abstractActor.GUID, visLevel, scanType))
                {
                    string cachedName = CombatLogNameCacheHelper.Get(abstractActor.GUID);
                    IRTweaksHelper.LogIfEnabled($"Unit GUID {abstractActor.GUID}: returning cached name {cachedName}");
                    return cachedName;
                }

                if (abstractActor is Mech mech) name = GetHostileMechName(mech, visLevel, scanType);
                else if (abstractActor is Turret turret) name = GetHostileTurretName(turret, visLevel, scanType);
                else if (abstractActor is Vehicle vehicle) name = GetHostileVehicleName(vehicle, visLevel, scanType);


                if (Mod.Config.Integrations.IRTweaks.CombatLogNames == CombatLogIntegration.REMEMBER)
                {
                    CombatLogNameCacheHelper.Add(abstractActor.GUID, visLevel, scanType, name);
                }
            }
            else if (abstractActor is Mech mech)
            {
                IRTweaksHelper.LogIfEnabled($"Unit GUID {abstractActor.GUID}: is non-hostile mech");

                name = GetNonHostileMechName(mech);
            }
            IRTweaksHelper.LogIfEnabled($"Unit GUID {abstractActor.GUID}: Returning name {name}");
            return name;
        }

        private static string GetHostileMechName(Mech mech, VisibilityLevel visibilityLevel, SensorScanType sensorScanType)
        {
            string fullName = mech.Description.UIName;

            string chassisName = mech.UnitName;
            string partialName = mech.Nickname;
            string typeName = (mech is ICustomMech custMech) ? custMech.UnitTypeName : string.Empty;
            IRTweaksHelper.LogIfEnabled($"Mech GUID {mech.GUID}: calculating name with Visibility: {visibilityLevel} and Sensors: {sensorScanType}. Actual name is {fullName}");

            return UnitDetectionNameHelper.GetEnemyMechName(visibilityLevel, sensorScanType, typeName, fullName, partialName, chassisName);

        }

        private static string GetHostileTurretName(Turret turret, VisibilityLevel visibilityLevel, SensorScanType sensorScanType)
        {
            string chassisName = turret.UnitName;
            string fullName = turret.Nickname;

            IRTweaksHelper.LogIfEnabled($"Turret GUID {turret.GUID}: calculating name with Visibility: {visibilityLevel} and Sensors: {sensorScanType}. Actual name is {fullName}");

            return UnitDetectionNameHelper.GetTurretName(visibilityLevel, sensorScanType, fullName, chassisName);
        }

        private static string GetHostileVehicleName(Vehicle vehicle, VisibilityLevel visLevel, SensorScanType scanType)
        {
            string chassisName = vehicle.UnitName;
            string fullName = vehicle.Nickname;

            IRTweaksHelper.LogIfEnabled($"Vehicle GUID {vehicle.GUID}: calculating name with Visibility: {visLevel} and Sensors: {scanType}. Actual name is {fullName}");

            return UnitDetectionNameHelper.GetVehicleName(visLevel, scanType, fullName, chassisName);
        }

        private static string GetNonHostileMechName(Mech mech)
        {
            string fullName = mech.Description.UIName;
            string displayName = mech.DisplayName;
            return UnitDetectionNameHelper.GetNonHostileMechName(fullName, displayName);
        }

        public static string LowVisibilityCombatLogPilotNameModifier(string name, AbstractActor abstractActor)
        {
            if (abstractActor == null) { return name; }

            IGuid pilot = abstractActor.GetPilot();
            string pilotGUID = pilot.GUID ?? "<Unknown>";

            IRTweaksHelper.LogIfEnabled($"Pilot {pilotGUID}: Starting name check");

            if (abstractActor.Combat.HostilityMatrix.IsLocalPlayerEnemy(abstractActor.team.GUID))
            {
                IRTweaksHelper.LogIfEnabled($"Pilot {pilotGUID}: is hostile");
                VisibilityLevel visLevel = abstractActor.Combat.LocalPlayerTeam.VisibilityToTarget(abstractActor);

                SensorScanType scanType = GetSensorScanType(abstractActor);

                if (Mod.Config.Integrations.IRTweaks.CombatLogNames == CombatLogIntegration.REMEMBER && CombatLogNameCacheHelper.ContainsEqualOrBetterName(abstractActor.GUID + PILOT_KEY_SUFFIX, visLevel, scanType))
                {
                    string cachedName = CombatLogNameCacheHelper.Get(abstractActor.GUID + PILOT_KEY_SUFFIX);
                    IRTweaksHelper.LogIfEnabled($"Pilot {pilotGUID}: returning cached name {cachedName}");
                    return cachedName;
                }

                IRTweaksHelper.LogIfEnabled($"Pilot {pilotGUID}: calculating name with Visibility: {visLevel} and Sensors: {scanType}");
                name = PilotNameHelper.GetEnemyPilotName(visLevel, scanType, abstractActor);


                if (Mod.Config.Integrations.IRTweaks.CombatLogNames == CombatLogIntegration.REMEMBER)
                {
                    CombatLogNameCacheHelper.Add(abstractActor.GUID + PILOT_KEY_SUFFIX, visLevel, scanType, name);
                }
            }
            IRTweaksHelper.LogIfEnabled($"Pilot {pilotGUID}: returning name {name}");
            return name;
        }

        private static SensorScanType GetSensorScanType(AbstractActor abstractActor)
        {
            SensorScanType scanType = SensorLockHelper.CalculateSharedLock(abstractActor, null);
            if (scanType < SensorScanType.ArmorAndWeaponType)
            {
                bool hasVisualScan = VisualLockHelper.CanSpotTargetUsingCurrentPositions(ModState.LastPlayerActorActivated, abstractActor);
                if (hasVisualScan) scanType = SensorScanType.ArmorAndWeaponType;
            }

            return scanType;
        }
    }
}
