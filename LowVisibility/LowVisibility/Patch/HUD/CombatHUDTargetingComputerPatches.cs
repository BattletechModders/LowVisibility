using BattleTech.UI;
using Localize;
using LowVisibility.Helper;
using LowVisibility.Integration;
using LowVisibility.Object;
using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using TMPro;
using UnityEngine;
using us.frostraptor.modUtils;

namespace LowVisibility.Patch
{
    // Allow the CombatHUDTargeting computer to be displayed for blips
    [HarmonyPatch(typeof(CombatHUDTargetingComputer), "OnActorHovered", new Type[] { typeof(MessageCenterMessage) })]
    public static class CombatHUDTargetingComputer_OnActorHovered
    {

        public static void Postfix(CombatHUDTargetingComputer __instance, MessageCenterMessage message, CombatHUD ___HUD)
        {
            // LowVisibility.Logger.Debug("CombatHUDTargetingComputer:OnActorHovered:post - entered.");

            if (__instance != null)
            {

                EncounterObjectMessage encounterObjectMessage = message as EncounterObjectMessage;
                ICombatant combatant = ___HUD.Combat.FindCombatantByGUID(encounterObjectMessage.affectedObjectGuid);
                if (combatant != null)
                {
                    AbstractActor abstractActor = combatant as AbstractActor;
                    if (combatant.team != ___HUD.Combat.LocalPlayerTeam && (abstractActor == null ||
                        ___HUD.Combat.LocalPlayerTeam.VisibilityToTarget(abstractActor) >= VisibilityLevel.Blip0Minimum))
                    {
                        __instance.HoveredCombatant = combatant;
                    }
                }
            }

        }
    }

    // Patch to allow the targeting comp to be shown for a blip
    [HarmonyPatch(typeof(CombatHUDTargetingComputer), "Update")]
    public static class CombatHUDTargetingComputer_Update
    {

        private static Action<CombatHUDTargetingComputer> UIModule_Update;

        public static bool Prepare()
        {
            BuildCHTCOnComplete();
            return true;
        }

        // Shamelessly stolen from https://github.com/janxious/BT-WeaponRealizer/blob/7422573fa69893ae7c16a9d192d85d2152f90fa2/NumberOfShotsEnabler.cs#L32
        private static void BuildCHTCOnComplete()
        {
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
        public static void Prefix(ref bool __runOriginal, CombatHUDTargetingComputer __instance, CombatHUD ___HUD)
        {
            if (!__runOriginal) return;

            //Mod.Log.Trace?.Write("CHUDTC:U:pre - entered.");

            CombatGameState Combat = ___HUD?.Combat;

            UIModule_Update(__instance);
            if (__instance.ActorInfo != null)
            {
                __instance.ActorInfo.DisplayedCombatant = __instance.ActivelyShownCombatant;
            }

            if (__instance.ActivelyShownCombatant == null ||
                (__instance.ActivelyShownCombatant.team != Combat.LocalPlayerTeam
                    && !Combat.HostilityMatrix.IsFriendly(__instance.ActivelyShownCombatant.team.GUID, Combat.LocalPlayerTeamGuid)
                    && Combat.LocalPlayerTeam.VisibilityToTarget(__instance.ActivelyShownCombatant) < VisibilityLevel.Blip0Minimum)
                    )
            {
                if (__instance.Visible)
                {
                    __instance.Visible = false;
                }
            }
            else
            {
                if (!__instance.Visible)
                {
                    __instance.Visible = true;
                }
                if (__instance.ActivelyShownCombatant != null)
                {
                    __instance.UpdateStructureAndArmor();
                }
            }

            __runOriginal = false;
        }
    }

    // Patch the weapons visibility
    [HarmonyPatch(typeof(CombatHUDTargetingComputer), "RefreshActorInfo")]
    public static class CombatHUDTargetingComputer_RefreshActorInfo
    {
        public static void Postfix(CombatHUDTargetingComputer __instance, List<TextMeshProUGUI> ___weaponNames)
        {

            if (__instance == null || __instance.ActivelyShownCombatant == null ||
                __instance.ActivelyShownCombatant.Combat == null || __instance.ActivelyShownCombatant.Combat.HostilityMatrix == null ||
                __instance.WeaponList == null)
            {
                Mod.Log.Debug?.Write($"CHTC:RAI ~~~ TC, target, or WeaponList is null, skipping.");
                return;
            }

            if (ModState.LastPlayerActorActivated == null)
            {
                Mod.Log.Error?.Write("Attempting to refresh ActorInfo, but LastPlayerActorActivated is null. This should never happen!");
            }

            if (__instance.ActivelyShownCombatant.team.IsLocalPlayer ||
                __instance.ActivelyShownCombatant.Combat.HostilityMatrix.IsLocalPlayerFriendly(__instance.ActivelyShownCombatant.team.GUID))
            {
                Mod.Log.Debug?.Write($"CHTC:RAI ~~~ target:{CombatantUtils.Label(__instance.ActivelyShownCombatant)} friendly, resetting.");
                __instance.WeaponList.SetActive(true);
                return;
            }

            // Only enemies or neutrals below this point
            Mod.Log.Debug?.Write($"CHTC:RAI ~~~ target:{CombatantUtils.Label(__instance.ActivelyShownCombatant)} is enemy");

            try
            {
                if (__instance.ActivelyShownCombatant is AbstractActor target)
                {
                    float range = Vector3.Distance(ModState.LastPlayerActorActivated.CurrentPosition, target.CurrentPosition);
                    bool hasVisualScan = VisualLockHelper.CanSpotTarget(ModState.LastPlayerActorActivated, ModState.LastPlayerActorActivated.CurrentPosition,
                        target, target.CurrentPosition, target.CurrentRotation, target.Combat.LOS);
                    SensorScanType scanType = SensorLockHelper.CalculateSharedLock(target, ModState.LastPlayerActorActivated);
                    Mod.Log.Debug?.Write($"CHTC:RAI ~~~ LastActivated:{CombatantUtils.Label(ModState.LastPlayerActorActivated)} vs. enemy:{CombatantUtils.Label(target)} " +
                        $"at range: {range} has scanType:{scanType} visualScan:{hasVisualScan}");

                    // Build the CAC side-panel
                    CACSidePanelHooks.SetCHUDInfoSidePanelInfo(ModState.LastPlayerActorActivated, __instance.ActivelyShownCombatant, range, hasVisualScan, scanType);

                    if (scanType >= SensorScanType.StructAndWeaponID)
                    {
                        __instance.WeaponList.SetActive(true);
                        CUHooks.ToggleTargetingComputerArmorDisplay(__instance, true);
                    }
                    else if (scanType >= SensorScanType.ArmorAndWeaponType || hasVisualScan)
                    {
                        CUHooks.ToggleTargetingComputerArmorDisplay(__instance, true);
                        ObfuscateWeaponLabels(___weaponNames, target);

                        // Update the summary display
                        __instance.WeaponList.SetActive(true);
                        Transform weaponListT = __instance.WeaponList?.transform?.parent?.Find("tgtWeaponsLabel");
                        GameObject weaponsLabel = weaponListT.gameObject;
                        TextMeshProUGUI labelText = weaponsLabel.GetComponent<TextMeshProUGUI>();
                        labelText.SetText(new Text(Mod.LocalizedText.TargetingComputer[ModText.LT_TARG_COMP_UNIDENTIFIED]).ToString());
                    }
                    else
                    {
                        CUHooks.ToggleTargetingComputerArmorDisplay(__instance, false);

                        __instance.WeaponList.SetActive(false);
                        Transform weaponListT = __instance.WeaponList?.transform?.parent?.Find("tgtWeaponsLabel");
                        GameObject weaponsLabel = weaponListT.gameObject;
                        weaponsLabel.SetActive(false);
                    }
                }
                else if (__instance.ActivelyShownCombatant is BattleTech.Building building)
                {
                    Mod.Log.Debug?.Write($"CHTC:RAI ~~~ target:{CombatantUtils.Label(__instance.ActivelyShownCombatant)} is enemy building");

                    CUHooks.ToggleTargetingComputerArmorDisplay(__instance, true);

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
                Mod.Log.Error?.Write(e, "Failed to RefreshActorInfo!");
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
                            wName = new Text(Mod.LocalizedText.TargetingComputer[ModText.LT_TARG_COMP_BALLISTIC]).ToString();
                            break;
                        case WeaponType.Laser:
                        case WeaponType.PPC:
                        case WeaponType.Flamer:
                            wName = new Text(Mod.LocalizedText.TargetingComputer[ModText.LT_TARG_COMP_ENERGY]).ToString();
                            break;
                        case WeaponType.LRM:
                        case WeaponType.SRM:
                            wName = new Text(Mod.LocalizedText.TargetingComputer[ModText.LT_TARG_COMP_MISSILE]).ToString();
                            break;
                        case WeaponType.Melee:
                            wName = new Text(Mod.LocalizedText.TargetingComputer[ModText.LT_TARG_COMP_PHYSICAL]).ToString();
                            break;
                        default:
                            wName = new Text(Mod.LocalizedText.TargetingComputer[ModText.LT_TARG_COMP_UNIDENTIFIED]).ToString();
                            break;
                    }
                    ___weaponNames[i].SetText(wName);
                }
                else if (!___weaponNames[i].text.Equals("XXXXXXXXXXXXXX"))
                {
                    ___weaponNames[i].SetText(new Text(Mod.LocalizedText.TargetingComputer[ModText.LT_TARG_COMP_UNIDENTIFIED]).ToString());
                }
            }
        }
    }

}
