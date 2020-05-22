using BattleTech;
using BattleTech.UI;
using Harmony;
using Localize;
using LowVisibility.Helper;
using LowVisibility.Object;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using TMPro;
using UnityEngine;
using us.frostraptor.modUtils;

namespace LowVisibility.Patch {
    // Allow the CombatHUDTargeting computer to be displayed for blips
    [HarmonyPatch()]
    public static class CombatHUDTargetingComputer_OnActorHovered {

        // Private method can't be patched by annotations, so use MethodInfo
        public static MethodInfo TargetMethod() {
            return AccessTools.Method(typeof(CombatHUDTargetingComputer), "OnActorHovered", new Type[] { typeof(MessageCenterMessage) });
        }

        public static void Postfix(CombatHUDTargetingComputer __instance, MessageCenterMessage message, CombatHUD ___HUD) {
            // LowVisibility.Logger.Debug("CombatHUDTargetingComputer:OnActorHovered:post - entered.");

            if (__instance != null) {

                EncounterObjectMessage encounterObjectMessage = message as EncounterObjectMessage;
                ICombatant combatant = ___HUD.Combat.FindCombatantByGUID(encounterObjectMessage.affectedObjectGuid);
                if (combatant != null) {
                    AbstractActor abstractActor = combatant as AbstractActor;
                    if (combatant.team != ___HUD.Combat.LocalPlayerTeam && (abstractActor == null ||
                        ___HUD.Combat.LocalPlayerTeam.VisibilityToTarget(abstractActor) >= VisibilityLevel.Blip0Minimum)) {
                        Traverse.Create(__instance).Property("HoveredCombatant").SetValue(combatant);
                    }
                }
            }

        }
    }

    // Patch to allow the targeting comp to be shown for a blip
    [HarmonyPatch(typeof(CombatHUDTargetingComputer), "Update")]
    public static class CombatHUDTargetingComputer_Update {

        private static Action<CombatHUDTargetingComputer> UIModule_Update;

        public static bool Prepare() {
            BuildCHTCOnComplete();
            return true;
        }

        // Shamelessly stolen from https://github.com/janxious/BT-WeaponRealizer/blob/7422573fa69893ae7c16a9d192d85d2152f90fa2/NumberOfShotsEnabler.cs#L32
        private static void BuildCHTCOnComplete() {
            // build a call to WeaponEffect.OnComplete() so it can be called
            // a la base.OnComplete() from the context of a BallisticEffect
            // https://blogs.msdn.microsoft.com/rmbyers/2008/08/16/invoking-a-virtual-method-non-virtually/
            // https://docs.microsoft.com/en-us/dotnet/api/system.activator?view=netframework-3.5
            // https://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.dynamicmethod.-ctor?view=netframework-3.5#System_Reflection_Emit_DynamicMethod__ctor_System_String_System_Type_System_Type___System_Type_
            // https://stackoverflow.com/a/4358250/1976
            var method = typeof(UIModule).GetMethod("Update", AccessTools.all);
            var dm = new DynamicMethod("CombatHUDTargetingComputerOnComplete", null, new Type[] { typeof(CombatHUDTargetingComputer) }, typeof(CombatHUDTargetingComputer));
            var gen = dm.GetILGenerator();
            gen.Emit(OpCodes.Ldarg_0);
            gen.Emit(OpCodes.Call, method);
            gen.Emit(OpCodes.Ret);
            UIModule_Update = (Action<CombatHUDTargetingComputer>)dm.CreateDelegate(typeof(Action<CombatHUDTargetingComputer>));
        }

        // TODO: Dangerous PREFIX false here!
        public static bool Prefix(CombatHUDTargetingComputer __instance, CombatHUD ___HUD) {
            //Mod.Log.Trace("CHUDTC:U:pre - entered.");

            CombatGameState Combat = ___HUD?.Combat;

            UIModule_Update(__instance);
            if (__instance.ActorInfo != null) {
                __instance.ActorInfo.DisplayedCombatant = __instance.ActivelyShownCombatant;
            }

            if (__instance.ActivelyShownCombatant == null ||
                (__instance.ActivelyShownCombatant.team != Combat.LocalPlayerTeam
                    && !Combat.HostilityMatrix.IsFriendly(__instance.ActivelyShownCombatant.team.GUID, Combat.LocalPlayerTeamGuid)
                    && Combat.LocalPlayerTeam.VisibilityToTarget(__instance.ActivelyShownCombatant) < VisibilityLevel.Blip0Minimum)
                    ) {
                if (__instance.Visible) {
                    __instance.Visible = false;
                }
            } else {
                if (!__instance.Visible) {
                    __instance.Visible = true;
                }
                if (__instance.ActivelyShownCombatant != null) {
                    Traverse method = Traverse.Create(__instance).Method("UpdateStructureAndArmor", new Type[] { });
                    method.GetValue();
                }
            }

            return false;
        }
    }

    // Patch the weapons visibility
    [HarmonyPatch(typeof(CombatHUDTargetingComputer), "RefreshActorInfo")]
    public static class CombatHUDTargetingComputer_RefreshActorInfo {

        private static void SetArmorDisplayActive(CombatHUDTargetingComputer __instance, bool active) {
            Mech mech = __instance.ActivelyShownCombatant as Mech;
            Vehicle vehicle = __instance.ActivelyShownCombatant as Vehicle;
            Turret turret = __instance.ActivelyShownCombatant as Turret;
            Building building = __instance.ActivelyShownCombatant as Building;

            if (mech != null) { __instance.MechArmorDisplay.gameObject.SetActive(active); }
            else if (vehicle != null) { __instance.VehicleArmorDisplay.gameObject.SetActive(active); }
            else if (turret != null) { __instance.TurretArmorDisplay.gameObject.SetActive(active); }
            else if (building != null) { __instance.BuildingArmorDisplay.gameObject.SetActive(active); }        
        }

        private static void BuildCACDialogForTarget(AbstractActor source, ICombatant target, float range, bool hasVisualScan, SensorScanType scanType)
        {
            StringBuilder sb = new StringBuilder();

            VisibilityLevel visLevel = source.VisibilityToTargetUnit(target);
            if (target is Mech mech)
            {
                string fullName = mech.Description.UIName;
                string chassisName = mech.UnitName;
                string partialName = mech.Nickname;
                string localName = CombatNameHelper.GetEnemyMechDetectionLabel(visLevel, scanType, fullName, partialName, chassisName).ToString();

                string tonnage = "?";
                if (scanType > SensorScanType.LocationAndType)
                {
                    tonnage = new Text(Mod.Config.LocalizedText[ModConfig.LT_CAC_SIDEPANEL_WEIGHT], new object[] { (int)Math.Floor(mech.tonnage) }).ToString();
                }

                string titleText = new Text(Mod.Config.LocalizedText[ModConfig.LT_CAC_SIDEPANEL_TITLE],
                    new object[] { localName, tonnage }).ToString();
                sb.Append(titleText);

                if (scanType > SensorScanType.StructAndWeaponID)
                {
                    // Movement
                    sb.Append(new Text(Mod.Config.LocalizedText[ModConfig.LT_CAC_SIDEPANEL_MOVE_MECH],
                        new object[] { mech.WalkSpeed, mech.RunSpeed, mech.JumpDistance })
                        .ToString()
                        );

                    // Heat
                    sb.Append(new Text(Mod.Config.LocalizedText[ModConfig.LT_CAC_SIDEPANEL_HEAT],
                        new object[] { mech.CurrentHeat, mech.MaxHeat })
                        .ToString()
                        );

                    // Stability
                    sb.Append(new Text(Mod.Config.LocalizedText[ModConfig.LT_CAC_SIDEPANEL_STAB],
                        new object[] { mech.CurrentStability, mech.MaxStability })
                        .ToString()
                        );

                }

            }
            else if (target is Turret turret)
            {
                string chassisName = turret.UnitName;
                string fullName = turret.Nickname;
                string localName = CombatNameHelper.GetTurretOrVehicleDetectionLabel(visLevel, scanType, fullName, chassisName, false).ToString();

                string titleText = new Text(Mod.Config.LocalizedText[ModConfig.LT_CAC_SIDEPANEL_TITLE],
                    new object[] { localName, "" }).ToString();
                sb.Append(titleText);
            }
            else if (target is Vehicle vehicle)
            {
                string chassisName = vehicle.UnitName;
                string fullName = vehicle.Nickname;
                string localName = CombatNameHelper.GetTurretOrVehicleDetectionLabel(visLevel, scanType, fullName, chassisName, true).ToString();

                string tonnage = "?";
                if (scanType > SensorScanType.LocationAndType)
                {
                    tonnage = new Text(Mod.Config.LocalizedText[ModConfig.LT_CAC_SIDEPANEL_WEIGHT], new object[] { (int)Math.Floor(vehicle.tonnage) }).ToString();
                }

                string titleText = new Text(Mod.Config.LocalizedText[ModConfig.LT_CAC_SIDEPANEL_TITLE],
                    new object[] { localName, tonnage }).ToString();
                sb.Append(titleText);

                if (scanType > SensorScanType.StructAndWeaponID)
                {
                    // Movement
                    sb.Append(new Text(Mod.Config.LocalizedText[ModConfig.LT_CAC_SIDEPANEL_MOVE_VEHICLE],
                        new object[] { vehicle.CruiseSpeed, vehicle.FlankSpeed })
                        .ToString()
                        );
                }

            }


            string distance = new Text(Mod.Config.LocalizedText[ModConfig.LT_CAC_SIDEPANEL_DIST], 
                new object[] { (int)Math.Ceiling(range) }).ToString();
            sb.Append(distance);

            Text panelText = new Text(sb.ToString(), new object[] { });

            CustAmmoCategories.CombatHUDInfoSidePanelHelper.SetTargetInfo(source, target, panelText);
        }
        
        public static void Postfix(CombatHUDTargetingComputer __instance, List<TextMeshProUGUI> ___weaponNames) {

            if (__instance == null || __instance.ActivelyShownCombatant == null || 
                __instance.ActivelyShownCombatant.Combat == null || __instance.ActivelyShownCombatant.Combat.HostilityMatrix == null ||
                __instance.WeaponList == null) 
            {
                Mod.Log.Debug($"CHTC:RAI ~~~ TC, target, or WeaponList is null, skipping.");
                return;
            }

            if (ModState.LastPlayerActorActivated == null)
            {
                Mod.Log.Error("Attempting to refresh ActorInfo, but LastPlayerActorActivated is null. This should never happen!");
            }

            if (__instance.ActivelyShownCombatant.Combat.HostilityMatrix.IsLocalPlayerFriendly(__instance.ActivelyShownCombatant.team.GUID)) 
            {
                Mod.Log.Debug($"CHTC:RAI ~~~ target:{CombatantUtils.Label(__instance.ActivelyShownCombatant)} friendly, resetting.");
                __instance.WeaponList.SetActive(true);
                return;
            } 

            // Only enemies or neutrals below this point
            Mod.Log.Debug($"CHTC:RAI ~~~ target:{CombatantUtils.Label(__instance.ActivelyShownCombatant)} is enemy");

            try
            {
                if ((__instance.ActivelyShownCombatant as AbstractActor) != null)
                {
                    AbstractActor target = __instance.ActivelyShownCombatant as AbstractActor;

                    float range = Vector3.Distance(ModState.LastPlayerActorActivated.CurrentPosition, target.CurrentPosition);
                    bool hasVisualScan = VisualLockHelper.CanSpotTarget(ModState.LastPlayerActorActivated, ModState.LastPlayerActorActivated.CurrentPosition, 
                        target, target.CurrentPosition, target.CurrentRotation, target.Combat.LOS);
                    SensorScanType scanType = SensorLockHelper.CalculateSharedLock(target, ModState.LastPlayerActorActivated);
                    Mod.Log.Debug($"CHTC:RAI ~~~ LastActivated:{CombatantUtils.Label(ModState.LastPlayerActorActivated)} vs. enemy:{CombatantUtils.Label(target)} " +
                        $"at range: {range} has scanType:{scanType} visualScan:{hasVisualScan}");

                    // Build the CAC side-panel
                    try
                    {
                        BuildCACDialogForTarget(ModState.LastPlayerActorActivated, __instance.ActivelyShownCombatant, range, hasVisualScan, scanType);
                    }
                    catch (Exception e)
                    {
                        Mod.Log.Error($"Failed to initialize CAC SidePanel for source: {CombatantUtils.Label(ModState.LastPlayerActorActivated)} and " +
                            $"target: {CombatantUtils.Label(__instance.ActivelyShownCombatant)}!", e);
                    }

                    if (scanType >= SensorScanType.StructAndWeaponID)
                    {
                        __instance.WeaponList.SetActive(true);
                        SetArmorDisplayActive(__instance, true);
                    }
                    else if (scanType >= SensorScanType.ArmorAndWeaponType || hasVisualScan)
                    {
                        SetArmorDisplayActive(__instance, true);
                        ObfuscateWeaponLabels(___weaponNames, target);

                        // Update the summary display
                        __instance.WeaponList.SetActive(true);
                        Transform weaponListT = __instance.WeaponList?.transform?.parent?.Find("tgtWeaponsLabel");
                        GameObject weaponsLabel = weaponListT.gameObject;
                        TextMeshProUGUI labelText = weaponsLabel.GetComponent<TextMeshProUGUI>();
                        labelText.SetText(new Text(Mod.Config.LocalizedText[ModConfig.LT_TARG_COMP_UNIDENTIFIED]).ToString());
                    }
                    else
                    {
                        SetArmorDisplayActive(__instance, false);

                        __instance.WeaponList.SetActive(false);
                        Transform weaponListT = __instance.WeaponList?.transform?.parent?.Find("tgtWeaponsLabel");
                        GameObject weaponsLabel = weaponListT.gameObject;
                        weaponsLabel.SetActive(false);
                    }
                }
                else if ((__instance.ActivelyShownCombatant as Building) != null)
                {
                    Mod.Log.Debug($"CHTC:RAI ~~~ target:{CombatantUtils.Label(__instance.ActivelyShownCombatant)} is enemy building");

                    SetArmorDisplayActive(__instance, true);

                    __instance.WeaponList.SetActive(false);
                    Transform weaponListT = __instance.WeaponList?.transform?.parent?.Find("tgtWeaponsLabel");
                    GameObject weaponsLabel = weaponListT.gameObject;
                    weaponsLabel.SetActive(false);
                }
                else
                {
                    // WTF
                }
            } 
            catch (Exception e)
            {
                Mod.Log.Error("Failed to RefreshActorInfo!", e);
            }

        }

        // Replaces weapon names with a static label that hints at the type, but not the specifics
        private static void ObfuscateWeaponLabels(List<TextMeshProUGUI> ___weaponNames, AbstractActor target)
        {
            for (int i = 0; i < ___weaponNames.Count; i++)
            {

                // Update ranged weapons
                if (i < target.Weapons.Count)
                {
                    Weapon targetWeapon = target.Weapons[i];

                    string wName;
                    switch (targetWeapon.Type)
                    {
                        case WeaponType.Autocannon:
                        case WeaponType.Gauss:
                        case WeaponType.MachineGun:
                        case WeaponType.AMS:
                            wName = new Text(Mod.Config.LocalizedText[ModConfig.LT_TARG_COMP_BALLISTIC]).ToString();
                            break;
                        case WeaponType.Laser:
                        case WeaponType.PPC:
                        case WeaponType.Flamer:
                            wName = new Text(Mod.Config.LocalizedText[ModConfig.LT_TARG_COMP_ENERGY]).ToString();
                            break;
                        case WeaponType.LRM:
                        case WeaponType.SRM:
                            wName = new Text(Mod.Config.LocalizedText[ModConfig.LT_TARG_COMP_MISSILE]).ToString();
                            break;
                        case WeaponType.Melee:
                            wName = new Text(Mod.Config.LocalizedText[ModConfig.LT_TARG_COMP_PHYSICAL]).ToString();
                            break;
                        default:
                            wName = new Text(Mod.Config.LocalizedText[ModConfig.LT_TARG_COMP_UNIDENTIFIED]).ToString();
                            break;
                    }
                    ___weaponNames[i].SetText(wName);
                }
                else if (!___weaponNames[i].text.Equals("XXXXXXXXXXXXXX"))
                {
                    ___weaponNames[i].SetText(new Text(Mod.Config.LocalizedText[ModConfig.LT_TARG_COMP_UNIDENTIFIED]).ToString());
                }
            }
        }
    }

}
