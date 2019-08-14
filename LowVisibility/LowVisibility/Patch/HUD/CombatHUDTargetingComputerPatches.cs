using BattleTech;
using BattleTech.UI;
using Harmony;
using LowVisibility.Helper;
using LowVisibility.Object;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
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
            //LowVisibility.Logger.Debug("CombatHUDTargetingComputer:Update:pre - entered.");

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
        
        public static void Postfix(CombatHUDTargetingComputer __instance, List<TextMeshProUGUI> ___weaponNames, CombatHUDStatusPanel ___StatusPanel) {
            if (__instance.ActivelyShownCombatant == null ) {
                Mod.Log.Trace($"CHTC:RAI ~~~ target is null, skipping.");
                return;
            } else if (
                __instance.ActivelyShownCombatant.Combat.HostilityMatrix.IsLocalPlayerFriendly(__instance.ActivelyShownCombatant.team.GUID)) {
                Mod.Log.Trace($"CHTC:RAI ~~~ target:{CombatantUtils.Label(__instance.ActivelyShownCombatant)} friendly, resetting.");
                __instance.WeaponList.SetActive(true);
                return;
            } else {
                Mod.Log.Trace($"CHTC:RAI ~~~ target:{CombatantUtils.Label(__instance.ActivelyShownCombatant)} is enemy");
                
                if ((__instance.ActivelyShownCombatant as AbstractActor) != null) {
                    AbstractActor target = __instance.ActivelyShownCombatant as AbstractActor;
                    SensorScanType scanType = SensorLockHelper.CalculateSharedLock(target, State.LastPlayerActorActivated);
                    Mod.Log.Debug($"CHTC:RAI ~~~ LastActivated:{CombatantUtils.Label(State.LastPlayerActorActivated)} vs. enemy:{CombatantUtils.Label(target)} has scanType:{scanType}");

                    if (scanType >= SensorScanType.WeaponAnalysis) {
                        __instance.WeaponList.SetActive(true);
                        SetArmorDisplayActive(__instance, true);
                    } else if (scanType == SensorScanType.SurfaceAnalysis ||
                        State.LastPlayerActorActivated.VisibilityToTargetUnit(target) == VisibilityLevel.LOSFull) {

                        SetArmorDisplayActive(__instance, true);

                        // Update the weapons to show only 
                        for (int i = 0; i < ___weaponNames.Count; i++) {

                            // Update ranged weapons
                            if (i < target.Weapons.Count) {
                                Weapon targetWeapon = target.Weapons[i];

                                string wName;
                                switch(targetWeapon.Type) {
                                    case WeaponType.Autocannon:
                                    case WeaponType.Gauss:
                                    case WeaponType.MachineGun:
                                    case WeaponType.AMS:
                                        wName = "Ballistic";
                                        break;
                                    case WeaponType.Laser:
                                    case WeaponType.PPC:
                                    case WeaponType.Flamer:
                                        wName = "Energy";
                                        break;
                                    case WeaponType.LRM:
                                    case WeaponType.SRM:
                                        wName = "Missile";
                                        break;
                                    case WeaponType.Melee:
                                        wName = "Physical";
                                        break;
                                    default:
                                        wName = "Unidentified";
                                        break;
                                }
                                ___weaponNames[i].SetText(wName);
                            } else if (!___weaponNames[i].text.Equals("XXXXXXXXXXXXXX")) {
                                ___weaponNames[i].SetText("Unidentified");
                            }
                        }                       

                        // Update the summary display
                        __instance.WeaponList.SetActive(true);
                        Transform weaponListT = __instance.WeaponList?.transform?.parent?.Find("tgtWeaponsLabel");
                        GameObject weaponsLabel = weaponListT.gameObject;
                        TextMeshProUGUI labelText = weaponsLabel.GetComponent<TextMeshProUGUI>();
                        labelText.SetText("Unidentified");
                    } else {

                        SetArmorDisplayActive(__instance, false);

                        __instance.WeaponList.SetActive(false);
                        Transform weaponListT = __instance.WeaponList?.transform?.parent?.Find("tgtWeaponsLabel");
                        GameObject weaponsLabel = weaponListT.gameObject;
                        weaponsLabel.SetActive(false);
                    }
                } else if ((__instance.ActivelyShownCombatant as Building) != null) {
                    Building target = __instance.ActivelyShownCombatant as Building;
                    Mod.Log.Debug($"CHTC:RAI ~~~ target:{CombatantUtils.Label(__instance.ActivelyShownCombatant)} is enemy building");

                    SetArmorDisplayActive(__instance, true);

                    __instance.WeaponList.SetActive(false);
                    Transform weaponListT = __instance.WeaponList?.transform?.parent?.Find("tgtWeaponsLabel");
                    GameObject weaponsLabel = weaponListT.gameObject;
                    weaponsLabel.SetActive(false);
                } else {
                    // WTF
                }
            }

        }
    }

}
