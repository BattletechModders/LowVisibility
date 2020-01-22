using BattleTech;
using BattleTech.UI;
using Harmony;
using LowVisibility.Helper;
using LowVisibility.Object;
using System;
using us.frostraptor.modUtils;

namespace LowVisibility.Patch.HUD {

    [HarmonyPatch(typeof(CombatHUDWeaponPanel), "RefreshDisplayedWeapons")]
    public static class CombatHUDWeaponPanel_RefreshDisplayedWeapons {
        public static void Prefix(CombatHUDWeaponPanel __instance, AbstractActor ___displayedActor) {

            if (__instance == null || ___displayedActor == null) { return; }
            Mod.Log.Trace("CHUDWP:RDW - entered.");

            Traverse targetT = Traverse.Create(__instance).Property("target");
            Traverse hoveredTargetT = Traverse.Create(__instance).Property("hoveredTarget");

            Traverse HUDT = Traverse.Create(__instance).Property("HUD");
            CombatHUD HUD = HUDT.GetValue<CombatHUD>();
            SelectionState activeState = HUD.SelectionHandler.ActiveState;
            ICombatant target;
            if (activeState != null && activeState is SelectionStateMove) {
                target = hoveredTargetT.GetValue<ICombatant>();
                if (target == null) { target = targetT.GetValue<ICombatant>(); }
            } else {
                target = targetT.GetValue<ICombatant>();
                if (target == null) { target = hoveredTargetT.GetValue<ICombatant>(); }
            }

            if (target == null) { return; }

            EWState attackerState = new EWState(___displayedActor);
            Mod.Log.Debug($"Attacker ({CombatantUtils.Label(___displayedActor)} => EWState: {attackerState}");
            bool canSpotTarget = VisualLockHelper.CanSpotTarget(___displayedActor, ___displayedActor.CurrentPosition,
                target, target.CurrentPosition, target.CurrentRotation, ___displayedActor.Combat.LOS);
            SensorScanType sensorScan = SensorLockHelper.CalculateSharedLock(target, ___displayedActor);
            Mod.Log.Debug($"  canSpotTarget: {canSpotTarget}  sensorScan: {sensorScan}");

            if (target is AbstractActor targetActor) {
                EWState targetState = new EWState(targetActor);
                Mod.Log.Debug($"Target ({CombatantUtils.Label(targetActor)} => EWState: {targetState}");
            }

        }
    }

    [HarmonyPatch(typeof(CombatHUDWeaponSlot), "SetHitChance")]
    [HarmonyPatch(new Type[] {  typeof(ICombatant) })]
    [HarmonyBefore("us.frostraptor.CBTBehaviorsEnhanced", "dZ.Zappo.Pilot_Quirks")]
    public static class CombatHUDWeaponSlot_SetHitChance {
        //public static void Prefix(CombatHUDWeaponSlot __instance, ICombatant target, Weapon ___displayedWeapon, CombatHUD ___HUD) {
            
        //    if (__instance == null || ___displayedWeapon == null || ___HUD.SelectedActor == null || target == null) { return; }
        //    Mod.Log.Trace("CHUDWS:SHC - entered.");

        //    EWState attackerState = new EWState(___HUD.SelectedActor);
        //    Mod.Log.Debug($"Attacker ({CombatantUtils.Label(___HUD.SelectedActor)} => EWState: {attackerState}");
        //    bool canSpotTarget = VisualLockHelper.CanSpotTarget(___HUD.SelectedActor, ___HUD.SelectedActor.CurrentPosition, 
        //        target, target.CurrentPosition, target.CurrentRotation, ___HUD.SelectedActor.Combat.LOS);
        //    SensorScanType sensorScan = SensorLockHelper.CalculateSharedLock(target, ___HUD.SelectedActor);
        //    Mod.Log.Debug($"  canSpotTarget: {canSpotTarget}  sensorScan: {sensorScan}");

        //    if (target is AbstractActor targetActor) {
        //        EWState targetState = new EWState(targetActor);
        //        Mod.Log.Debug($"Target ({CombatantUtils.Label(targetActor)} => EWState: {targetState}");
        //    }
        //}

        private static void Postfix(CombatHUDWeaponSlot __instance, ICombatant target, Weapon ___displayedWeapon, CombatHUD ___HUD) {

            if (__instance == null || ___displayedWeapon == null || ___HUD.SelectedActor == null || target == null) { return; }

            Mod.Log.Trace("CHUDWS:SHC - entered.");

            AbstractActor attacker = __instance.DisplayedWeapon.parent;
            Traverse AddToolTipDetailMethod = Traverse.Create(__instance).Method("AddToolTipDetail", 
                new Type[] { typeof(string), typeof(int) });

            if (target is AbstractActor targetActor && __instance.DisplayedWeapon != null) {
                float magnitude = (attacker.CurrentPosition - target.CurrentPosition).magnitude;
                EWState attackerState = new EWState(attacker);
                EWState targetState = new EWState(targetActor);

                // If we can't see the target, apply the No Visuals penalty
                bool canSpotTarget = VisualLockHelper.CanSpotTarget(attacker, attacker.CurrentPosition, target, target.CurrentPosition, target.CurrentRotation, attacker.Combat.LOS);
                int mimeticMod = targetState.MimeticAttackMod(attackerState);
                int eyeballAttackMod = canSpotTarget ? mimeticMod : Mod.Config.Attack.NoVisualsPenalty;

                // Zoom applies independently of visibility (request from Harkonnen)
                int zoomVisionMod = attackerState.GetZoomVisionAttackMod(__instance.DisplayedWeapon, magnitude);
                int zoomAttackMod = attackerState.HasZoomVisionToTarget(__instance.DisplayedWeapon, magnitude) ? zoomVisionMod - mimeticMod : Mod.Config.Attack.NoVisualsPenalty;
                Mod.Log.Debug($"  Visual attack == eyeball: {eyeballAttackMod} mimetic: {mimeticMod} zoomAtack: {zoomAttackMod}");

                bool hasVisualAttack = (eyeballAttackMod < Mod.Config.Attack.NoVisualsPenalty || zoomAttackMod < Mod.Config.Attack.NoVisualsPenalty);

                // Sensor attack bucket.  Sensors always fallback, so roll everything up and cap
                int narcAttackMod = targetState.NarcAttackMod(attackerState);
                int tagAttackMod = targetState.TagAttackMod(attackerState);
                int ecmShieldAttackMod = targetState.ECMAttackMod(attackerState);
                int stealthAttackMod = targetState.StealthAttackMod(attackerState, __instance.DisplayedWeapon, magnitude);
                Mod.Log.Debug($"  Sensor attack penalties == narc: {narcAttackMod}  tag: {tagAttackMod}  ecmShield: {ecmShieldAttackMod}  stealth: {stealthAttackMod}");

                bool hasSensorAttack = SensorLockHelper.CalculateSharedLock(targetActor, attacker) > SensorScanType.NoInfo;
                int sensorsAttackMod = Mod.Config.Attack.NoSensorsPenalty;
                if (hasSensorAttack) {
                    sensorsAttackMod = 0;
                    sensorsAttackMod -= narcAttackMod;
                    sensorsAttackMod -= tagAttackMod;
                    sensorsAttackMod += ecmShieldAttackMod;
                    sensorsAttackMod += stealthAttackMod;
                }
                if (sensorsAttackMod > Mod.Config.Attack.NoSensorsPenalty) {
                    Mod.Log.Debug($"  Rollup of penalties {sensorsAttackMod} is > than NoSensors, defaulting to {Mod.Config.Attack.NoSensorsPenalty} ");
                    sensorsAttackMod = Mod.Config.Attack.NoSensorsPenalty;
                    hasSensorAttack = false;
                }

                // Check firing blind
                if (!hasVisualAttack && !hasSensorAttack) {
                    string localText = new Localize.Text(Mod.Config.LocalizedText[ModConfig.LT_ATTACK_FIRING_BLIND]).ToString();
                    AddToolTipDetailMethod.GetValue(new object[] { localText, Mod.Config.Attack.FiringBlindPenalty });
                } else {
                    // Visual attacks
                    if (!hasVisualAttack) {
                        string localText = new Localize.Text(Mod.Config.LocalizedText[ModConfig.LT_ATTACK_NO_VISUALS]).ToString();
                        AddToolTipDetailMethod.GetValue(new object[] { localText, Mod.Config.Attack.NoVisualsPenalty });
                    } else {
                        // If the zoom + mimetic is better than eyeball, use that. Otherwise, we're using the good ol mk.1 eyeball
                        if (zoomAttackMod < eyeballAttackMod) {
                            string localText = new Localize.Text(Mod.Config.LocalizedText[ModConfig.LT_ATTACK_ZOOM_VISION]).ToString();
                            AddToolTipDetailMethod.GetValue(new object[] { localText, zoomVisionMod });
                        }

                        if (mimeticMod != 0) {
                            string localText = new Localize.Text(Mod.Config.LocalizedText[ModConfig.LT_ATTACK_MIMETIC]).ToString();
                            AddToolTipDetailMethod.GetValue(new object[] { localText, mimeticMod });
                        }
                    }

                    if (attackerState.HasHeatVisionToTarget(__instance.DisplayedWeapon, magnitude)) {
                        int heatAttackMod = attackerState.GetHeatVisionAttackMod(targetActor, magnitude, __instance.DisplayedWeapon);
                        string localText = new Localize.Text(Mod.Config.LocalizedText[ModConfig.LT_ATTACK_HEAT_VISION]).ToString();
                        AddToolTipDetailMethod.GetValue(new object[] { localText, heatAttackMod });
                    }

                    if (!hasSensorAttack) {
                        string localText = new Localize.Text(Mod.Config.LocalizedText[ModConfig.LT_ATTACK_NO_SENSORS]).ToString();
                        AddToolTipDetailMethod.GetValue(new object[] { localText, Mod.Config.Attack.NoSensorsPenalty });
                    } else {

                        if (ecmShieldAttackMod != 0) {
                            string localText = new Localize.Text(Mod.Config.LocalizedText[ModConfig.LT_ATTACK_ECM_SHEILD]).ToString();
                            AddToolTipDetailMethod.GetValue(new object[] { localText, ecmShieldAttackMod });
                        }
                        if (narcAttackMod != 0) {
                            string localText = new Localize.Text(Mod.Config.LocalizedText[ModConfig.LT_ATTACK_NARCED]).ToString();
                            AddToolTipDetailMethod.GetValue(new object[] { localText, narcAttackMod });
                        }
                        if (tagAttackMod != 0) {
                            string localText = new Localize.Text(Mod.Config.LocalizedText[ModConfig.LT_ATTACK_TAGGED]).ToString();
                            AddToolTipDetailMethod.GetValue(new object[] { localText, tagAttackMod });
                        }
                        if (stealthAttackMod != 0) {
                            string localText = new Localize.Text(Mod.Config.LocalizedText[ModConfig.LT_ATTACK_STEALTH]).ToString();
                            AddToolTipDetailMethod.GetValue(new object[] { localText, stealthAttackMod });
                        }
                    }
                }
            }
        }
    }

}
