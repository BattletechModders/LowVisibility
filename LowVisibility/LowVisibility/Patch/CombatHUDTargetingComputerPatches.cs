using BattleTech;
using BattleTech.UI;
using Harmony;
using LowVisibility.Helper;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using TMPro;
using UnityEngine;
using static LowVisibility.Helper.VisibilityHelper;

namespace LowVisibility.Patch {
    // Allow the CombatHUDTargeting computer to be displayed for blips
    [HarmonyPatch()]
    public static class CombatHUDTargetingComputer_OnActorHovered {

        // Private method can't be patched by annotations, so use MethodInfo
        public static MethodInfo TargetMethod() {
            return AccessTools.Method(typeof(CombatHUDTargetingComputer), "OnActorHovered", new Type[] { typeof(MessageCenterMessage) });
        }

        public static void Postfix(CombatHUDTargetingComputer __instance, MessageCenterMessage message, CombatHUD ___HUD) {
//            LowVisibility.Logger.LogIfDebug("CombatHUDTargetingComputer:OnActorHovered:post - entered.");

            if (__instance != null) {

                EncounterObjectMessage encounterObjectMessage = message as EncounterObjectMessage;
                ICombatant combatant = ___HUD.Combat.FindCombatantByGUID(encounterObjectMessage.affectedObjectGuid);
                if (combatant != null) {
                    AbstractActor abstractActor = combatant as AbstractActor;
                    if (combatant.team != ___HUD.Combat.LocalPlayerTeam && (abstractActor == null ||
                        ___HUD.Combat.LocalPlayerTeam.VisibilityToTarget(abstractActor) >= VisibilityLevel.Blip0Minimum)) {
                        Traverse.Create(__instance).Property("HoveredCombatant").SetValue(combatant);
                    }
                    /*
                    if (combatant.team != this.HUD.Combat.LocalPlayerTeam && 
                        (abstractActor == null || this.HUD.Combat.LocalPlayerTeam.VisibilityToTarget(abstractActor) == VisibilityLevel.LOSFull))
				    {
					    this.HoveredCombatant = combatant;
				    }
                     */
                }
            }

        }
    }

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
            //LowVisibility.Logger.LogIfDebug("CombatHUDTargetingComputer:Update:pre - entered.");

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

    // Patch the weapons
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

        // TODO: Need vehicle, turret, building displays
        public static void Postfix(CombatHUDTargetingComputer __instance, List<TextMeshProUGUI> ___weaponNames, CombatHUDStatusPanel ___StatusPanel) {
            //KnowYourFoe.Logger.Log("CombatHUDTargetingComputer:RefreshActorInfo:post - entered.");
            if (__instance.ActivelyShownCombatant != null) {
                // TODO: Make allies share info
                AbstractActor target = __instance.ActivelyShownCombatant as AbstractActor;
                bool isPlayer = target.Combat.HostilityMatrix.IsLocalPlayerEnemy(target.Combat.LocalPlayerTeam.GUID);

                //bool isPlayer = actor.team == actor.Combat.LocalPlayerTeam;
                //bool isFriendly = __instance.Combat.HostilityMatrix.IsFriendly(actor.team, actor.Combat.LocalPlayerTeam);

                if (!isPlayer) {
                    LockState lockState = State.GetUnifiedLockStateForTarget(State.GetLastPlayerActivatedActor(target.Combat), target);
                    LowVisibility.Logger.LogIfDebug($" ~~~ OpFor Actor:{ActorHelper.ActorLabel(target)} has lockState:{lockState}");
                    if (lockState.sensorType == SensorLockType.ProbeID) {
                        __instance.WeaponList.SetActive(true);
                        SetArmorDisplayActive(__instance, true);                            
                    } else if (lockState.visionType == VisionLockType.VisualID || lockState.sensorType == SensorLockType.SensorID) {
                        //KnowYourFoe.Logger.Log($"Detection state:{detectState} for actor:{target.DisplayName}_{target.GetPilot().Name} requires weapons to be hidden.");
                        // Update the summary display
                        Transform weaponListT = __instance.WeaponList?.transform?.parent?.Find("tgtWeaponsLabel");
                        GameObject weaponsLabel = weaponListT.gameObject;
                        TextMeshProUGUI labelText = weaponsLabel.GetComponent<TextMeshProUGUI>();
                        labelText.SetText("???");

                        // Update the weapons
                        for (int i = 0; i < ___weaponNames.Count; i++) {
                            //KnowYourFoe.Logger.Log($"CombatHUDTargetingComputer:RefreshActorInfo:post - iterating weapon:{___weaponNames[i].text}");
                            // Update ranged weapons
                            if (i < target.Weapons.Count) {
                                Weapon targetWeapon = target.Weapons[i];
                                //KnowYourFoe.Logger.Log($"CombatHUDTargetingComputer:RefreshActorInfo:post - hiding weapon:{targetWeapon.Name}");
                                ___weaponNames[i].SetText("???");
                            } else if (!___weaponNames[i].text.Equals("XXXXXXXXXXXXXX")) {
                                // Update melee and dfa without using locale specific strings
                                ___weaponNames[i].SetText("???");
                            }
                        }
                        __instance.WeaponList.SetActive(true);
                        SetArmorDisplayActive(__instance, true);
                    } else {
                        //KnowYourFoe.Logger.Log($"Detection state:{detectState} for actor:{target.DisplayName}_{target.GetPilot().Name} allows weapons to be seen.");
                        __instance.WeaponList.SetActive(false);
                        SetArmorDisplayActive(__instance, false);

                        Transform weaponListT = __instance.WeaponList?.transform?.parent?.Find("tgtWeaponsLabel");
                        GameObject weaponsLabel = weaponListT.gameObject;
                        weaponsLabel.SetActive(false);
                    }
                } else {
                    LowVisibility.Logger.Log($"CombatHUDTargetingComputer:RefreshActorInfo:post - actor:{ActorHelper.ActorLabel(target)} is player, showing panel.");
                }
            }
        }
    }

}
