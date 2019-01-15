using BattleTech;
using BattleTech.UI;
using Harmony;
using HBS;
using Localize;
using LowVisibility.Object;
using SVGImporter;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using static LowVisibility.Helper.ActorHelper;
using static LowVisibility.Helper.VisibilityHelper;

namespace LowVisibility.Patch {

    [HarmonyPatch()]
    public static class CombatHUDStatusPanel_RefreshDisplayedCombatant {

        // Private method can't be patched by annotations, so use MethodInfo
        public static MethodInfo TargetMethod() {
            return AccessTools.Method(typeof(CombatHUDStatusPanel), "RefreshDisplayedCombatant", new Type[] { });
        }

        public static void Postfix(CombatHUDStatusPanel __instance, List<CombatHUDStatusIndicator> ___Buffs, List<CombatHUDStatusIndicator> ___Debuffs) {
            //LowVisibility.Logger.LogIfDebug("CombatHUDStatusPanel:RefreshDisplayedCombatant:post - entered.");
            if (__instance != null && __instance.DisplayedCombatant != null) {
                AbstractActor target = __instance.DisplayedCombatant as AbstractActor;
                // We can receive a building here, so 
                if (target != null) {
                    bool isPlayer = target.team == target.Combat.LocalPlayerTeam;
                    if (!isPlayer) {
                        LockState lockState = GetUnifiedLockStateForTarget(State.GetLastPlayerActivatedActor(target.Combat), target);
                        // TODO: This is all fucked up
                        if (lockState.sensorLockLevel >= DetectionLevel.Vector) {
                            // Do nothing - display everything per vanilla
                            Traverse hideEvasionIndicatorMethod = Traverse.Create(__instance).Method("HideEvasiveIndicator", new object[] { });
                            hideEvasionIndicatorMethod.GetValue();
                        } else if (lockState.sensorLockLevel == DetectionLevel.NoInfo && lockState.visionLockLevel < VisionLockType.VisualID) {
                            ___Buffs.ForEach(si => si.gameObject.SetActive(false));
                            ___Debuffs.ForEach(si => si.gameObject.SetActive(false));
                            Traverse hideEvasionIndicatorMethod = Traverse.Create(__instance).Method("HideEvasiveIndicator", new object[] { });
                            hideEvasionIndicatorMethod.GetValue();
                        } else {
                            // All other states - hide the buffs/debuffs
                            ___Buffs.ForEach(si => si.gameObject.SetActive(false));
                            ___Debuffs.ForEach(si => si.gameObject.SetActive(false));

                        }
                    }
                }
            }
        }
    }

    [HarmonyPatch()]
    public static class CombatHUDStatusPanel_ShowActorStatuses {

        // Private method can't be patched by annotations, so use MethodInfo
        public static MethodInfo TargetMethod() {
            return AccessTools.Method(typeof(CombatHUDStatusPanel), "ShowActorStatuses", new Type[] { typeof(AbstractActor) });
        }

        public static void Postfix(CombatHUDStatusPanel __instance) {
            //LowVisibility.Logger.LogIfDebug("___ CombatHUDStatusPanel:ShowActorStatuses:post - entered.");

            if (__instance.DisplayedCombatant != null) {
                Type[] iconMethodParams = new Type[] { typeof(SVGAsset), typeof(Text), typeof(Text), typeof(Vector3), typeof(bool) };
                Traverse showDebuffIconMethod = Traverse.Create(__instance).Method("ShowDebuff", iconMethodParams);
                Traverse showBuffIconMethod = Traverse.Create(__instance).Method("ShowBuff", iconMethodParams);

                Type[] stringMethodParams = new Type[] { typeof(string), typeof(Text), typeof(Text), typeof(Vector3), typeof(bool) };
                Traverse showDebuffStringMethod = Traverse.Create(__instance).Method("ShowDebuff", stringMethodParams);
                Traverse showBuffStringMethod = Traverse.Create(__instance).Method("ShowBuff", stringMethodParams);

                AbstractActor actor = __instance.DisplayedCombatant as AbstractActor;
                StaticEWState staticState = State.GetStaticState(actor);
                DynamicEWState dynamicState = State.GetDynamicState(actor);

                bool isPlayer = actor.team == actor.Combat.LocalPlayerTeam;
                if (isPlayer) {                                        
                    if (dynamicState.sensorDetectLevel == DetectionLevel.NoInfo) {
                        showDebuffIconMethod.GetValue(new object[] {
                            LazySingletonBehavior<UIManager>.Instance.UILookAndColorConstants.StatusSensorLockIcon,
                            new Text("VISUALS ONLY", new object[0]),
                            new Text($"Unit can only visually detect targets within {State.GetMapVisionRange()}m", new object[0]),
                            __instance.effectIconScale,
                            false
                        });
                    } else {                                                
                        float sensorsDistance = CalculateSensorRange(actor);
                        // TODO: Change text/color for probe vs. sensors
                        // TODO: Show level of information you can expect to receive
                        showBuffIconMethod.GetValue(new object[] {
                            LazySingletonBehavior<UIManager>.Instance.UILookAndColorConstants.StatusSensorLockIcon,
                            new Text("SENSORS ACTIVE", new object[0]),
                            new Text($"Unit's sensors will detect targets out {sensorsDistance}m.", new object[0]),
                            __instance.effectIconScale,
                            false
                        });
                        // TODO: Indicate active probe?
                    }

                    // TODO: Better icon
                    if (staticState.ecmMod != 0) {
                        showBuffStringMethod.GetValue(new object[] {
                            "uixSvgIcon_status_sensorsImpaired",
                            new Text("ECM JAMMING", new object[0]),
                            new Text($"Unit has an ECM jammer and will hide allies with {staticState.ecmRange}m.", new object[0]),
                            __instance.effectIconScale,
                            false
                        });
                    }
                }
                
                if (State.IsJammed(actor)) {
                    showDebuffStringMethod.GetValue(new object[] {
                        "uixSvgIcon_status_sensorsImpaired",
                        new Text("ECM JAMMING", new object[0]),
                        new Text($"Unit is within an ECM jamming bubble that will make it difficult to detect other units.", new object[0]),
                        __instance.effectIconScale,
                        false
                    });
                }
            }

            /*
                private void ShowSensorLockIndicator() {
			        this.ShowDebuff(LazySingletonBehavior<UIManager>.Instance.UILookAndColorConstants.StatusSensorLockIcon, new Text("SENSOR LOCKED", new object[0]), new Text("This unit is Sensor Locked. It is visible to both sides.", new object[0]), this.effectIconScale, false);
		        }
             */
        }
    }

}
