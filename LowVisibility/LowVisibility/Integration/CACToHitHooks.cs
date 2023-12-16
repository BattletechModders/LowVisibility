using IRBTModUtils.Extension;
using LowVisibility.Helper;
using LowVisibility.Object;
using System.Collections.Generic;
using UnityEngine;

namespace LowVisibility.Integration
{
    public class CACToHitHooks
    {
        public const string CAC_MOD_NAME = "LOWVIS";

        public enum LowVisModifierType
        {
            FiringBlind, NoVisuals, zoomVisionMod, mimeticMod, heatAttackMod, NoSensors, ecmJammed, ecmShield,
            narcAttack, tagAttack, stealthAttack
        }

        public class LowVisToHitState
        {
            public Dictionary<LowVisModifierType, int> modifiers = new Dictionary<LowVisModifierType, int>();

            public float get(LowVisModifierType type)
            {
                if (modifiers.TryGetValue(type, out int result)) { return result; };
                return 0f;
            }

            public void AddModifiers(LowVisModifierType type, int value)
            {
                if (modifiers.ContainsKey(type)) { modifiers[type] = value; }
                else { modifiers.Add(type, value); };
            }
        }

        // CAC ToHit modifier methods below
        public static void RegisterToHitModifiers()
        {
            // Node will create the state before displaying any modifier
            CustAmmoCategories.ToHitModifiersHelper.registerNode(CAC_MOD_NAME, CACToHitHooks.Prepare);

            string firingBlindLabel = new Localize.Text(Mod.LocalizedText.AttackModifiers[ModText.LT_ATTACK_FIRING_BLIND]).ToString();
            CustAmmoCategories.ToHitModifiersHelper.registerNodeModifier(CAC_MOD_NAME, ModText.LT_ATTACK_FIRING_BLIND, firingBlindLabel, true, false, get_FiringBlindMod, null);

            // Visual modifiers
            string noVisualsLabel = new Localize.Text(Mod.LocalizedText.AttackModifiers[ModText.LT_ATTACK_NO_VISUALS]).ToString();
            CustAmmoCategories.ToHitModifiersHelper.registerNodeModifier(CAC_MOD_NAME, ModText.LT_ATTACK_NO_VISUALS, noVisualsLabel, true, false, get_NoVisualsMod, null);

            string zoomVisionLabel = new Localize.Text(Mod.LocalizedText.AttackModifiers[ModText.LT_ATTACK_ZOOM_VISION]).ToString();
            CustAmmoCategories.ToHitModifiersHelper.registerNodeModifier(CAC_MOD_NAME, ModText.LT_ATTACK_ZOOM_VISION, zoomVisionLabel, true, false, get_zoomVisionMod, null);

            string mimeticLabel = new Localize.Text(Mod.LocalizedText.AttackModifiers[ModText.LT_ATTACK_MIMETIC]).ToString();
            CustAmmoCategories.ToHitModifiersHelper.registerNodeModifier(CAC_MOD_NAME, ModText.LT_ATTACK_MIMETIC, mimeticLabel, true, false, get_mimeticMod, null);

            string heatVisionLabel = new Localize.Text(Mod.LocalizedText.AttackModifiers[ModText.LT_ATTACK_HEAT_VISION]).ToString();
            CustAmmoCategories.ToHitModifiersHelper.registerNodeModifier(CAC_MOD_NAME, ModText.LT_ATTACK_HEAT_VISION, heatVisionLabel, true, false, get_heatAttackMod, null);

            // Sensors modifiers
            string noSensorsLabel = new Localize.Text(Mod.LocalizedText.AttackModifiers[ModText.LT_ATTACK_NO_SENSORS]).ToString();
            CustAmmoCategories.ToHitModifiersHelper.registerNodeModifier(CAC_MOD_NAME, ModText.LT_ATTACK_NO_SENSORS, noSensorsLabel, true, false, get_NoSensors, null);

            string ecmJammedLabel = new Localize.Text(Mod.LocalizedText.AttackModifiers[ModText.LT_ATTACK_ECM_JAMMED]).ToString();
            CustAmmoCategories.ToHitModifiersHelper.registerNodeModifier(CAC_MOD_NAME, ModText.LT_ATTACK_ECM_JAMMED, ecmJammedLabel, true, false, get_ecmJammedMod, null);

            string ecmShieldLabel = new Localize.Text(Mod.LocalizedText.AttackModifiers[ModText.LT_ATTACK_ECM_SHIELD]).ToString();
            CustAmmoCategories.ToHitModifiersHelper.registerNodeModifier(CAC_MOD_NAME, ModText.LT_ATTACK_ECM_SHIELD, ecmShieldLabel, true, false, get_ecmShieldMod, null);

            string narcedLabel = new Localize.Text(Mod.LocalizedText.AttackModifiers[ModText.LT_ATTACK_NARCED]).ToString();
            CustAmmoCategories.ToHitModifiersHelper.registerNodeModifier(CAC_MOD_NAME, ModText.LT_ATTACK_NARCED, narcedLabel, true, false, get_narcAttackMod, null);

            string taggedLabel = new Localize.Text(Mod.LocalizedText.AttackModifiers[ModText.LT_ATTACK_TAGGED]).ToString();
            CustAmmoCategories.ToHitModifiersHelper.registerNodeModifier(CAC_MOD_NAME, ModText.LT_ATTACK_TAGGED, taggedLabel, true, false, get_tagAttackMod, null);

            string stealthedLabel = new Localize.Text(Mod.LocalizedText.AttackModifiers[ModText.LT_ATTACK_STEALTH]).ToString();
            CustAmmoCategories.ToHitModifiersHelper.registerNodeModifier(CAC_MOD_NAME, ModText.LT_ATTACK_STEALTH, stealthedLabel, true, false, get_stealthAttackMod, null);
        }


        public static LowVisToHitState Prepare(ToHit instance, AbstractActor attacker, Weapon weapon, ICombatant target, Vector3 attackPosition, Vector3 targetPosition, LineOfFireLevel lofLevel, MeleeAttackType meleeAttackType, bool isCalledShot)
        {
            LowVisToHitState state = new LowVisToHitState();
            if (target is AbstractActor targetActor && weapon != null)
            {
                float attackMagnitude = (attacker.CurrentPosition - target.CurrentPosition).magnitude;
                EWState attackerState = new EWState(attacker);
                EWState targetState = new EWState(targetActor);
                Mod.ToHitLog.Debug?.Write($"Preparing toHit from: {attacker.DistinctId()} to: {target.DistinctId()} with range: {attackMagnitude} for weapon: {weapon?.UIName}");


                // Determine if we can visually detect the target, either from the attack position or our position
                bool hasLOFWithinVisionRange = VisualLockHelper.CanSpotTarget(attacker, attacker.CurrentPosition, target, target.CurrentPosition, target.CurrentRotation, attacker.Combat.LOS);
                if (Vector3.Distance(attacker.CurrentPosition, attackPosition) > 0.1f)
                {
                    lofLevel = attacker.Combat.LOS.GetLineOfFire(attacker, attackPosition, target, target.CurrentPosition, target.CurrentRotation, out Vector3 vector);
                }
                else
                {
                    lofLevel = attacker.VisibilityCache.VisibilityToTarget(target).LineOfFireLevel;
                }
                bool hasZoomVision = attackerState.HasZoomVisionToTarget(weapon, attackMagnitude, lofLevel);
                bool canAttackWithVision = hasLOFWithinVisionRange || attackerState.HasZoomVisionToTarget(weapon, attackMagnitude, lofLevel);
                Mod.ToHitLog.Debug?.Write($"Attacker hasLOFInRange: {hasLOFWithinVisionRange} hasZoomVision: {hasZoomVision}");

                // Calculate an eyeball + mimetic visual attack result
                int mimeticMod = targetState.MimeticAttackMod(attackerState);
                int zoomVisionMod = attackerState.GetZoomAttackMod(weapon, attackMagnitude);

                // Sensor attack bucket.  Sensors always fallback, so roll everything up and cap
                int narcAttackMod = targetState.NarcAttackMod(attackerState);
                int tagAttackMod = targetState.TagAttackMod(attackerState);

                int ecmJammedAttackMod = attackerState.ECMJammedAttackMod();
                int ecmShieldAttackMod = targetState.ECMAttackMod(attackerState);
                int stealthAttackMod = targetState.StealthAttackMod(attackerState, weapon, attackMagnitude);
                Mod.ToHitLog.Debug?.Write($"  Sensor attack penalties == narc: {narcAttackMod}  tag: {tagAttackMod}  ecmShield: {ecmShieldAttackMod}  stealth: {stealthAttackMod}");

                bool hasSensorAttack = SensorLockHelper.CalculateSharedLock(targetActor, attacker) > SensorScanType.NoInfo;
                int sensorsAttackMod = Mod.Config.Attack.NoSensorsPenalty;
                if (hasSensorAttack)
                {
                    sensorsAttackMod = 0;
                    sensorsAttackMod += narcAttackMod;
                    sensorsAttackMod += tagAttackMod;

                    sensorsAttackMod += ecmJammedAttackMod;
                    sensorsAttackMod += ecmShieldAttackMod;
                    sensorsAttackMod += stealthAttackMod;
                }
                if (sensorsAttackMod > Mod.Config.Attack.NoSensorsPenalty)
                {
                    Mod.ToHitLog.Debug?.Write($"  Rollup of penalties {sensorsAttackMod} is > than NoSensors, defaulting to {Mod.Config.Attack.NoSensorsPenalty} ");
                    hasSensorAttack = false;
                }

                // Check firing blind
                if (!canAttackWithVision && !hasSensorAttack)
                {
                    Mod.ToHitLog.Debug?.Write("  Has neither visual or sensor attack, applying firing blind penalty");
                    state.AddModifiers(LowVisModifierType.FiringBlind, Mod.Config.Attack.FiringBlindPenalty);
                }
                else
                {
                    // Visual attacks
                    if (!canAttackWithVision)
                    {
                        Mod.ToHitLog.Debug?.Write("  Neither vision or zoom has range to target, apply NoVisuals penalty");
                        state.AddModifiers(LowVisModifierType.NoVisuals, Mod.Config.Attack.NoVisualsPenalty);
                    }
                    else
                    {
                        if (zoomVisionMod != 0)
                        {
                            state.AddModifiers(LowVisModifierType.zoomVisionMod, zoomVisionMod);
                        }

                        if (mimeticMod != 0)
                        {
                            state.AddModifiers(LowVisModifierType.mimeticMod, mimeticMod);
                        }
                    }

                    if (attackerState.HasHeatVisionToTarget(weapon, attackMagnitude))
                    {
                        int heatAttackMod = attackerState.GetHeatVisionAttackMod(targetActor, attackMagnitude, weapon);
                        if (heatAttackMod != 0)
                        {
                            state.AddModifiers(LowVisModifierType.heatAttackMod, heatAttackMod);
                        }
                    }

                    if (!hasSensorAttack)
                    {
                        Mod.ToHitLog.Debug?.Write("  Applying noSensors penalty");
                        state.AddModifiers(LowVisModifierType.NoSensors, Mod.Config.Attack.NoSensorsPenalty);
                    }
                    else
                    {

                        if (ecmJammedAttackMod != 0)
                        {
                            state.AddModifiers(LowVisModifierType.ecmJammed, ecmJammedAttackMod);
                        }
                        if (ecmShieldAttackMod != 0)
                        {
                            state.AddModifiers(LowVisModifierType.ecmShield, ecmShieldAttackMod);
                        }
                        if (narcAttackMod != 0)
                        {
                            state.AddModifiers(LowVisModifierType.narcAttack, narcAttackMod);
                        }
                        if (tagAttackMod != 0)
                        {
                            state.AddModifiers(LowVisModifierType.tagAttack, tagAttackMod);
                        }
                        if (stealthAttackMod != 0)
                        {
                            state.AddModifiers(LowVisModifierType.stealthAttack, stealthAttackMod);
                        }
                    }
                }
            }
            return state;
        }

        public static float get_FiringBlindMod(object state, ToHit instance, AbstractActor attacker, Weapon weapon, ICombatant target, Vector3 attackPosition, Vector3 targetPosition, LineOfFireLevel lofLevel, MeleeAttackType meleeAttackType, bool isCalledShot)
        {
            return ((LowVisToHitState)state).get(LowVisModifierType.FiringBlind);
        }

        public static float get_NoVisualsMod(object state, ToHit instance, AbstractActor attacker, Weapon weapon, ICombatant target, Vector3 attackPosition, Vector3 targetPosition, LineOfFireLevel lofLevel, MeleeAttackType meleeAttackType, bool isCalledShot)
        {
            return ((LowVisToHitState)state).get(LowVisModifierType.NoVisuals);
        }

        public static float get_zoomVisionMod(object state, ToHit instance, AbstractActor attacker, Weapon weapon, ICombatant target, Vector3 attackPosition, Vector3 targetPosition, LineOfFireLevel lofLevel, MeleeAttackType meleeAttackType, bool isCalledShot)
        {
            return ((LowVisToHitState)state).get(LowVisModifierType.zoomVisionMod);
        }

        public static float get_mimeticMod(object state, ToHit instance, AbstractActor attacker, Weapon weapon, ICombatant target, Vector3 attackPosition, Vector3 targetPosition, LineOfFireLevel lofLevel, MeleeAttackType meleeAttackType, bool isCalledShot)
        {
            return ((LowVisToHitState)state).get(LowVisModifierType.mimeticMod);
        }

        public static float get_heatAttackMod(object state, ToHit instance, AbstractActor attacker, Weapon weapon, ICombatant target, Vector3 attackPosition, Vector3 targetPosition, LineOfFireLevel lofLevel, MeleeAttackType meleeAttackType, bool isCalledShot)
        {
            return ((LowVisToHitState)state).get(LowVisModifierType.heatAttackMod);
        }

        public static float get_NoSensors(object state, ToHit instance, AbstractActor attacker, Weapon weapon, ICombatant target, Vector3 attackPosition, Vector3 targetPosition, LineOfFireLevel lofLevel, MeleeAttackType meleeAttackType, bool isCalledShot)
        {
            return ((LowVisToHitState)state).get(LowVisModifierType.NoSensors);
        }

        public static float get_ecmJammedMod(object state, ToHit instance, AbstractActor attacker, Weapon weapon, ICombatant target, Vector3 attackPosition, Vector3 targetPosition, LineOfFireLevel lofLevel, MeleeAttackType meleeAttackType, bool isCalledShot)
        {
            return ((LowVisToHitState)state).get(LowVisModifierType.ecmJammed);
        }

        public static float get_ecmShieldMod(object state, ToHit instance, AbstractActor attacker, Weapon weapon, ICombatant target, Vector3 attackPosition, Vector3 targetPosition, LineOfFireLevel lofLevel, MeleeAttackType meleeAttackType, bool isCalledShot)
        {
            return ((LowVisToHitState)state).get(LowVisModifierType.ecmShield);
        }

        public static float get_narcAttackMod(object state, ToHit instance, AbstractActor attacker, Weapon weapon, ICombatant target, Vector3 attackPosition, Vector3 targetPosition, LineOfFireLevel lofLevel, MeleeAttackType meleeAttackType, bool isCalledShot)
        {
            return ((LowVisToHitState)state).get(LowVisModifierType.narcAttack);
        }

        public static float get_tagAttackMod(object state, ToHit instance, AbstractActor attacker, Weapon weapon, ICombatant target, Vector3 attackPosition, Vector3 targetPosition, LineOfFireLevel lofLevel, MeleeAttackType meleeAttackType, bool isCalledShot)
        {
            return ((LowVisToHitState)state).get(LowVisModifierType.tagAttack);
        }

        public static float get_stealthAttackMod(object state, ToHit instance, AbstractActor attacker, Weapon weapon, ICombatant target, Vector3 attackPosition, Vector3 targetPosition, LineOfFireLevel lofLevel, MeleeAttackType meleeAttackType, bool isCalledShot)
        {
            return ((LowVisToHitState)state).get(LowVisModifierType.stealthAttack);
        }
    }

}
