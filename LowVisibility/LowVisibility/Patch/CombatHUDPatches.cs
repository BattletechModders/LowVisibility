using BattleTech;
using BattleTech.UI;
using Harmony;
using LowVisibility.Helper;
using System;
using System.Reflection;
using System.Reflection.Emit;
using UnityEngine;
using static LowVisibility.Helper.ActorHelper;
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
            LowVisibility.Logger.LogIfDebug("CombatHUDTargetingComputer:OnActorHovered:post - entered.");

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

    // Show some elements on the Targeting Computer that are normally hidden from blips
    [HarmonyPatch()]
    public static class CombatHUDActorInfo_UpdateItemVisibility {

        // Private method can't be patched by annotations, so use MethodInfo
        public static MethodInfo TargetMethod() {
            return AccessTools.Method(typeof(CombatHUDActorInfo), "UpdateItemVisibility", new Type[] { });
        }

        public static void Postfix(CombatHUDActorInfo __instance, AbstractActor ___displayedActor, BattleTech.Building ___displayedBuilding, ICombatant ___displayedCombatant) {
            bool isEnemyOrNeutral = false;
            VisibilityLevel visibilityLevel = VisibilityLevel.None;            
            if (___displayedCombatant != null) {
                if (___displayedCombatant.IsForcedVisible) {
                    visibilityLevel = VisibilityLevel.LOSFull;
                } else if (___displayedBuilding != null) {
                    visibilityLevel = __instance.Combat.LocalPlayerTeam.VisibilityToTarget(___displayedBuilding);
                } else if (___displayedActor != null) {
                    if (__instance.Combat.HostilityMatrix.IsLocalPlayerFriendly(___displayedActor.team)) {
                        visibilityLevel = VisibilityLevel.LOSFull;
                    } else {
                        visibilityLevel = __instance.Combat.LocalPlayerTeam.VisibilityToTarget(___displayedActor);
                        isEnemyOrNeutral = true;
                    }
                }
            }
            Traverse setGOActiveMethod = Traverse.Create(__instance).Method("SetGOActive", new Type[] { typeof(UnityEngine.MonoBehaviour), typeof(bool) });

            // The actual method should handle allied and friendly units fine, so we can just change it for enemies
            if (isEnemyOrNeutral && visibilityLevel > VisibilityLevel.Blip0Minimum) {
                LockState lockState = State.GetUnifiedLockStateForTarget(State.GetLastPlayerActivatedActor(___displayedActor.Combat), ___displayedActor);

                // Values that are always displayed
                setGOActiveMethod.GetValue(__instance.NameDisplay, true);
                setGOActiveMethod.GetValue(__instance.ArmorBar, true);
                setGOActiveMethod.GetValue(__instance.StructureBar, true);

                if (lockState.sensorType == SensorLockType.ProbeID) {
                    // Show unit summary
                    setGOActiveMethod.GetValue(__instance.DetailsDisplay, true);

                    // Show active state
                    setGOActiveMethod.GetValue(__instance.InspiredDisplay, false);
                    setGOActiveMethod.GetValue(__instance.StabilityDisplay, true);
                    setGOActiveMethod.GetValue(__instance.HeatDisplay, true);
                    setGOActiveMethod.GetValue(__instance.MarkDisplay, true);

                    // Show init badge (if actor)
                    if (___displayedActor != null) { setGOActiveMethod.GetValue(__instance.PhaseDisplay, true); } 
                    else { setGOActiveMethod.GetValue(__instance.PhaseDisplay, false); }

                    // Show armor and struct
                    setGOActiveMethod.GetValue(__instance.ArmorBar, true);
                    setGOActiveMethod.GetValue(__instance.StructureBar, true);
                } else if (lockState.sensorType == SensorLockType.SensorID) {
                    // Show unit summary
                    setGOActiveMethod.GetValue(__instance.DetailsDisplay, false);

                    // Show active state
                    setGOActiveMethod.GetValue(__instance.InspiredDisplay, false);
                    setGOActiveMethod.GetValue(__instance.StabilityDisplay, false);
                    setGOActiveMethod.GetValue(__instance.HeatDisplay, false);
                    setGOActiveMethod.GetValue(__instance.MarkDisplay, false);

                    // Show init badge (if actor)
                    if (___displayedActor != null) { setGOActiveMethod.GetValue(__instance.PhaseDisplay, true); } 
                    else { setGOActiveMethod.GetValue(__instance.PhaseDisplay, false); }

                    // Show armor and struct
                    setGOActiveMethod.GetValue(__instance.ArmorBar, true);
                    setGOActiveMethod.GetValue(__instance.StructureBar, true);
                } else if (lockState.visionType == VisionLockType.VisualID) {
                    // Hide unit summary
                    setGOActiveMethod.GetValue(__instance.DetailsDisplay, false);

                    // Hide active state
                    setGOActiveMethod.GetValue(__instance.InspiredDisplay, false);
                    setGOActiveMethod.GetValue(__instance.StabilityDisplay, false);
                    setGOActiveMethod.GetValue(__instance.HeatDisplay, false);
                    setGOActiveMethod.GetValue(__instance.MarkDisplay, false);

                    // Hide init badge
                    setGOActiveMethod.GetValue(__instance.PhaseDisplay, false);

                    // Show armor and struct
                    setGOActiveMethod.GetValue(__instance.ArmorBar, true);
                    setGOActiveMethod.GetValue(__instance.StructureBar, true);
                } else if (lockState.visionType == VisionLockType.Silhouette) {
                    // Hide unit summary
                    setGOActiveMethod.GetValue(__instance.DetailsDisplay, false);

                    // Hide active state
                    setGOActiveMethod.GetValue(__instance.InspiredDisplay, false);
                    setGOActiveMethod.GetValue(__instance.StabilityDisplay, false);
                    setGOActiveMethod.GetValue(__instance.HeatDisplay, false);
                    setGOActiveMethod.GetValue(__instance.MarkDisplay, false);

                    // Hide init badge
                    setGOActiveMethod.GetValue(__instance.PhaseDisplay, false);

                    // Hide armor and struct
                    setGOActiveMethod.GetValue(__instance.ArmorBar, false);
                    setGOActiveMethod.GetValue(__instance.StructureBar, false);
                } else {
                    // Hide unit summary
                    setGOActiveMethod.GetValue(__instance.DetailsDisplay, false);

                    // Hide active state
                    setGOActiveMethod.GetValue(__instance.InspiredDisplay, false);
                    setGOActiveMethod.GetValue(__instance.StabilityDisplay, false);
                    setGOActiveMethod.GetValue(__instance.HeatDisplay, false);
                    setGOActiveMethod.GetValue(__instance.MarkDisplay, false);

                    // Hide init badge
                    setGOActiveMethod.GetValue(__instance.PhaseDisplay, false);

                    // Hide armor and struct
                    setGOActiveMethod.GetValue(__instance.ArmorBar, false);
                    setGOActiveMethod.GetValue(__instance.StructureBar, false);
                }

                CombatHUDStateStack stateStack = (CombatHUDStateStack)Traverse.Create(__instance).Property("StateStack").GetValue();
                setGOActiveMethod.GetValue(stateStack, false);
            }
        }
    }
    
    [HarmonyPatch(typeof(CombatHUDWeaponSlot), "SetHitChance", new Type[] { typeof(ICombatant) })]
    public static class CombatHUDWeaponSlot_SetHitChance {

        private static void Postfix(CombatHUDWeaponSlot __instance, ICombatant target) {
            if (__instance == null || target == null) { return;  }

            AbstractActor actor = __instance.DisplayedWeapon.parent;
            AbstractActor targetActor = target as AbstractActor;
            Traverse AddToolTipDetailMethod = Traverse.Create(__instance).Method("AddToolTipDetail", new Type[] { typeof(string), typeof(int) });

            //LowVisibility.Logger.LogIfDebug($"___CombatHUDTargetingComputer - SetHitChance for source:{ActorLabel(targetActor)} target:{ActorLabel(targetActor)}");
            LockState lockState = State.GetUnifiedLockStateForTarget(actor, targetActor);
            if (lockState.sensorType == SensorLockType.None) {
                AddToolTipDetailMethod.GetValue(new object[] { "NO SENSOR LOCK", (int)LowVisibility.Config.NoSensorLockAttackPenalty });
            }
            if (lockState.visionType == VisionLockType.None) {
                AddToolTipDetailMethod.GetValue(new object[] { "NO VISUAL LOCK", (int)LowVisibility.Config.NoVisualLockAttackPenalty });
            }

            ActorEWConfig targetEWConfig = State.GetOrCreateActorEWConfig(target as AbstractActor);
            if (targetEWConfig.HasStealthRangeMod()) {
                float distance = Vector3.Distance(actor.CurrentPosition, targetActor.CurrentPosition);
                Weapon weapon = __instance.DisplayedWeapon;
                int weaponStealthMod = targetEWConfig.StealthRangeModAtDistance(weapon, distance);
                if (weaponStealthMod != 0) {
                    AddToolTipDetailMethod.GetValue(new object[] { "STEALTH", weaponStealthMod });
                }
            }
        }
    }
}
