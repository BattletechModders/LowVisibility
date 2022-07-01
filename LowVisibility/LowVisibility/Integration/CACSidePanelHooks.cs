using BattleTech;
using IRBTModUtils;
using Localize;
using LowVisibility.Helper;
using LowVisibility.Object;
using LowVisibility.Patch;
using System;
using System.Text;
using us.frostraptor.modUtils;

namespace LowVisibility.Integration
{
    public class CACSidePanelHooks
    {
        public static void SetCHUDInfoSidePanelInfo(AbstractActor source, ICombatant target, float range, bool hasVisualScan, SensorScanType scanType)
        {
            try
            {
                StringBuilder sb = new StringBuilder();

                VisibilityLevel visLevel = source.VisibilityToTargetUnit(target);
                if (target is Mech mech)
                {
                    string fullName = mech.Description.UIName;
                    string chassisName = mech.UnitName;
                    string partialName = mech.Nickname;
                    string typeName = (target is ICustomMech custMech) ? custMech.UnitTypeName : string.Empty;
                    string localName = UnitDetectionNameHelper.GetEnemyMechName(visLevel, scanType, typeName, fullName, partialName, chassisName);

                    string tonnage = "?";
                    if (scanType > SensorScanType.LocationAndType)
                    {
                        tonnage = new Text(Mod.LocalizedText.CACSidePanel[ModText.LT_CAC_SIDEPANEL_WEIGHT], new object[] { (int)Math.Floor(mech.tonnage) }).ToString();
                    }

                    string titleText = new Text(Mod.LocalizedText.CACSidePanel[ModText.LT_CAC_SIDEPANEL_TITLE],
                        new object[] { localName, tonnage }).ToString();
                    sb.Append(titleText);

                    if (scanType > SensorScanType.StructAndWeaponID)
                    {
                        // Movement
                        sb.Append(new Text(Mod.LocalizedText.CACSidePanel[ModText.LT_CAC_SIDEPANEL_MOVE_MECH],
                            new object[] { mech.WalkSpeed, mech.RunSpeed, mech.JumpDistance })
                            .ToString()
                            );

                        // Heat
                        sb.Append(new Text(Mod.LocalizedText.CACSidePanel[ModText.LT_CAC_SIDEPANEL_HEAT],
                            new object[] { mech.CurrentHeat, mech.MaxHeat })
                            .ToString()
                            );

                        // Stability
                        sb.Append(new Text(Mod.LocalizedText.CACSidePanel[ModText.LT_CAC_SIDEPANEL_STAB],
                            new object[] { mech.CurrentStability, mech.MaxStability })
                            .ToString()
                            );

                    }

                }
                else if (target is Turret turret)
                {
                    string chassisName = turret.UnitName;
                    string fullName = turret.Nickname;
                    string localName = UnitDetectionNameHelper.GetTurretName(visLevel, scanType, fullName, chassisName);

                    string titleText = new Text(Mod.LocalizedText.CACSidePanel[ModText.LT_CAC_SIDEPANEL_TITLE],
                        new object[] { localName, "" }).ToString();
                    sb.Append(titleText);
                }
                else if (target is Vehicle vehicle)
                {
                    string chassisName = vehicle.UnitName;
                    string fullName = vehicle.Nickname;
                    string localName = UnitDetectionNameHelper.GetVehicleName(visLevel, scanType, fullName, chassisName);

                    string tonnage = "?";
                    if (scanType > SensorScanType.LocationAndType)
                    {
                        tonnage = new Text(Mod.LocalizedText.CACSidePanel[ModText.LT_CAC_SIDEPANEL_WEIGHT], new object[] { (int)Math.Floor(vehicle.tonnage) }).ToString();
                    }

                    string titleText = new Text(Mod.LocalizedText.CACSidePanel[ModText.LT_CAC_SIDEPANEL_TITLE],
                        new object[] { localName, tonnage }).ToString();
                    sb.Append(titleText);

                    if (scanType > SensorScanType.StructAndWeaponID)
                    {
                        // Movement
                        sb.Append(new Text(Mod.LocalizedText.CACSidePanel[ModText.LT_CAC_SIDEPANEL_MOVE_VEHICLE],
                            new object[] { vehicle.CruiseSpeed, vehicle.FlankSpeed })
                            .ToString()
                            );
                    }

                }


                string distance = new Text(Mod.LocalizedText.CACSidePanel[ModText.LT_CAC_SIDEPANEL_DIST],
                    new object[] { (int)Math.Ceiling(range) }).ToString();
                sb.Append(distance);

                Text panelText = new Text(sb.ToString(), new object[] { });

                CustAmmoCategories.CombatHUDInfoSidePanelHelper.SetTargetInfo(source, target, panelText);
            }
            catch (Exception e)
            {
                Mod.Log.Error?.Write(e, $"Failed to initialize CAC SidePanel for source: {CombatantUtils.Label(source)} and " +
                    $"target: {CombatantUtils.Label(target)}!");
            }
            
        }
    }
}
