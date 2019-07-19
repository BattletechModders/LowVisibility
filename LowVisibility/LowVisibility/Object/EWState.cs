using BattleTech;
using LowVisibility.Helper;
using System;
using UnityEngine;
using us.frostraptor.modUtils;
using us.frostraptor.modUtils.math;

namespace LowVisibility.Object {

    // <signature_modifier>_<details_modifier> _<mediumAttackMod>_<longAttackmod> _<extremeAttackMod>
    class Stealth {
        public float SignatureMulti = 0.0f;
        public int DetailsMod = 0;
        public int MediumRangeAttackMod = 0;
        public int LongRangeAttackMod = 0;
        public int ExtremeRangeAttackMod = 0;

        public override string ToString() { return $"SignatureMulti:{SignatureMulti} details:{DetailsMod} " +
                $"rangeMods:{MediumRangeAttackMod} / {LongRangeAttackMod} / {ExtremeRangeAttackMod}";  }
    }

    // <initialVisibility>_<initialModifier>_<stepsUntilDecay>
    class Mimetic {
        public float VisibilityMulti = 0.0f;
        public int AttackMod = 0;
        public int HexesUntilDecay = 0;

        public override string ToString() {
            return $"visibilityMulti:{VisibilityMulti} attackMod:{AttackMod} hexesUntilDecay:{HexesUntilDecay}";
        }
    }

    // <initialAttackModifier>_<attackModifierCap>_<hexesUntilDecay>
    class ZoomVision {
        public int AttackMod = 0;
        public int AttackCap = 0;
        public int HexesUntilDecay = 0;

        public override string ToString() {
            return $"attackMod:{AttackMod} attackCap:{AttackCap} hexesUntilDecay:{HexesUntilDecay}";
        }
    }

    // <initialAttackModifier>_<heatDivisorForStep>
    class HeatVision {
        public int AttackMod = 0;
        public float HeatDivisor = 1f;

        public override string ToString() {
            return $"attackMod:{AttackMod} heatDivisor:{HeatDivisor}";
        }
    }

    public class EWState {

        private readonly AbstractActor actor;

        private int CurrentRoundEWCheck = 0; // Raw value before any manipulation

        private string parentGUID = null;

        private int ECMShield = 0;
        private int ECMCarrier = 0;
        private int ECMJammed = 0;

        private int AdvancedSensors = 0;

        private int ProbeCarrier = 0;
        private int ProbeSweepTarget = 0;

        private Stealth Stealth = null;
        private Mimetic Mimetic = null;

        private ZoomVision ZoomVision = null;
        private HeatVision HeatVision = null;
        // TODO: Night vision here

        // The amount of tactics bonus to the sensor check
        private float tacticsModifier = 1.0f;

        // Whether this actor will share sensor data with others
        private bool sharesSensors = false;

        // Necessary for serialization
        public EWState() {}

        // Normal Constructor
        public EWState(AbstractActor actor) {
            this.actor = actor;

            tacticsModifier = actor.StatCollection.ContainsStatistic(ModStats.TacticsMod) ?
                actor.StatCollection.GetStatistic(ModStats.TacticsMod).Value<int>() : 0;

            CurrentRoundEWCheck = actor.StatCollection.ContainsStatistic(ModStats.CurrentRoundEWCheck) ? 
                actor.StatCollection.GetStatistic(ModStats.CurrentRoundEWCheck).Value<int>() : 0;

            // ECM
            ECMJammed = actor.StatCollection.ContainsStatistic(ModStats.ECMJammed) ?
                actor.StatCollection.GetStatistic(ModStats.ECMJammed).Value<int>() : 0;
            ECMShield = actor.StatCollection.ContainsStatistic(ModStats.ECMShield) ?
                actor.StatCollection.GetStatistic(ModStats.ECMShield).Value<int>() : 0;
            ECMCarrier = actor.StatCollection.ContainsStatistic(ModStats.ECMCarrier) ?
                actor.StatCollection.GetStatistic(ModStats.ECMCarrier).Value<int>() : 0;

            // Sensors
            AdvancedSensors = actor.StatCollection.ContainsStatistic(ModStats.AdvancedSensors) ?
                actor.StatCollection.GetStatistic(ModStats.AdvancedSensors).Value<int>() : 0;

            // Probes
            ProbeCarrier = actor.StatCollection.ContainsStatistic(ModStats.ProbeCarrier) ?
                actor.StatCollection.GetStatistic(ModStats.ProbeCarrier).Value<int>() : 0;
            ProbeSweepTarget = actor.StatCollection.ContainsStatistic(ModStats.ProbeSweepTarget) ?
                actor.StatCollection.GetStatistic(ModStats.ProbeSweepTarget).Value<int>() : 0;

            // Stealth - <signature_modifier>_<details_modifier>_<mediumAttackMod>_<longAttackmod>_<extremeAttackMod>
            if (actor.StatCollection.ContainsStatistic(ModStats.StealthEffect) &&
                actor.StatCollection.GetStatistic(ModStats.StealthEffect).Value<string>() != "") {
                string rawValue = actor.StatCollection.GetStatistic(ModStats.StealthEffect).Value<string>();
                string[] tokens = rawValue.Split('_');
                if (tokens.Length == 5) {
                    try {
                        Stealth = new Stealth {
                            SignatureMulti = float.Parse(tokens[0]),
                            DetailsMod = Int32.Parse(tokens[1]),
                            MediumRangeAttackMod = Int32.Parse(tokens[2]),
                            LongRangeAttackMod = Int32.Parse(tokens[3]),
                            ExtremeRangeAttackMod = Int32.Parse(tokens[4])
                        };
                    } catch (Exception) {
                        Mod.Log.Info($"Failed to tokenize StealthEffect value: ({rawValue}). Discarding!");
                        Stealth = null;
                    }
                } else {
                    Mod.Log.Info($"WARNING: Invalid StealthEffect value: ({rawValue}) found. Discarding!");
                }
            }
            
            // Mimetic - <initialVisibility>_<initialModifier>_<stepsUntilDecay>
            if (actor.StatCollection.ContainsStatistic(ModStats.MimeticEffect) &&
                actor.StatCollection.GetStatistic(ModStats.MimeticEffect).Value<string>() != "") {
                string rawValue = actor.StatCollection.GetStatistic(ModStats.MimeticEffect).Value<string>();
                string[] tokens = rawValue.Split('_');
                if (tokens.Length == 3) {
                    try {
                        Mimetic = new Mimetic {
                            VisibilityMulti = float.Parse(tokens[0]),
                            AttackMod = Int32.Parse(tokens[1]),
                            HexesUntilDecay = Int32.Parse(tokens[2]),
                        };
                    } catch (Exception) {
                        Mod.Log.Info($"Failed to tokenize Mimetic value: ({rawValue}). Discarding!");
                        Mimetic = null;
                    }
                } else {
                    Mod.Log.Info($"WARNING: Invalid Mimetic value: ({rawValue}) found. Discarding!");
                }
            }

            // ZoomVision - <initialAttackModifier>_<attackModifierCap>_<hexesUntilDecay>
            if (actor.StatCollection.ContainsStatistic(ModStats.ZoomVision) &&
                actor.StatCollection.GetStatistic(ModStats.ZoomVision).Value<string>() != "") {
                string rawValue = actor.StatCollection.GetStatistic(ModStats.ZoomVision).Value<string>();
                string[] tokens = rawValue.Split('_');
                if (tokens.Length == 3) {
                    try {
                        ZoomVision = new ZoomVision {
                            AttackMod = Int32.Parse(tokens[0]),
                            AttackCap = Int32.Parse(tokens[1]),
                            HexesUntilDecay = Int32.Parse(tokens[2])
                        };
                    } catch (Exception) {
                        Mod.Log.Info($"Failed to tokenize ZoomVision value: ({rawValue}). Discarding!");
                        Mimetic = null;
                    }
                } else {
                    Mod.Log.Info($"WARNING: Invalid ZoomVision value: ({rawValue}) found. Discarding!");
                }
            }

            // HeatVision - <initialAttackModifier>_<heatDivisorForStep>
            if (actor.StatCollection.ContainsStatistic(ModStats.HeatVision) &&
                actor.StatCollection.GetStatistic(ModStats.HeatVision).Value<string>() != "") {
                string rawValue = actor.StatCollection.GetStatistic(ModStats.HeatVision).Value<string>();
                string[] tokens = rawValue.Split('_');
                if (tokens.Length == 3) {
                    try {
                        HeatVision = new HeatVision {
                            AttackMod = Int32.Parse(tokens[0]),
                            HeatDivisor = float.Parse(tokens[1]),
                    };
                    } catch (Exception) {
                        Mod.Log.Info($"Failed to tokenize HeatVision  value: ({rawValue}). Discarding!");
                        Mimetic = null;
                    }
                } else {
                    Mod.Log.Info($"WARNING: Invalid HeatVision  value: ({rawValue}) found. Discarding!");
                }
            }

            //EWStateHelper.UpdateStaticState(this, actor);
        }

        public int GetCurrentEWCheck() { return CurrentRoundEWCheck; }

        // ECM
        public int GetECMJammedDetailsModifier() { return ECMJammed;  }
        public float GetECMShieldSignatureModifier() { return ECMShield > ECMCarrier ? ECMShield * 0.05f : ECMCarrier * 0.05f; }
        public int GetECMShieldDetailsModifier() { return ECMShield > ECMCarrier ? ECMShield : ECMCarrier;  }
        public int GetECMShieldAttackModifier(EWState attackerState) {
            int ECMMod = ECMShield > ECMCarrier ? ECMShield : ECMCarrier;
            Mod.Log.Debug($"Target:({CombatantUtils.Label(actor)}) has ECMAttackMod:{ECMMod} - ProbeMod:{attackerState.GetProbeSelfModifier()} " +
                $"from source:{CombatantUtils.Label(attackerState.actor)}");
            return Math.Max(0, ECMMod - attackerState.GetProbeSelfModifier());
        }

        // Sensors
        public int GetAdvancedSensorsMod() { return AdvancedSensors; }
        public float GetSensorsRangeMulti() { return CurrentRoundEWCheck / 20.0f + tacticsModifier / 10.0f; }
        public float GetSensorsBaseRange() {
            if (actor.GetType() == typeof(Mech)) {
                return Mod.Config.SensorRangeMechType * 30.0f;
            } else if (actor.GetType() == typeof(Vehicle)) {
                return Mod.Config.SensorRangeVehicleType * 30.0f;
            } else if (actor.GetType() == typeof(Turret)) {
                return Mod.Config.SensorRangeTurretType * 30.0f;
            } else {
                return Mod.Config.SensorRangeUnknownType * 30.0f;
            }
        }

        // Probes
        public int GetProbeSelfModifier() { return ProbeCarrier; }
        public int GetTargetOfProbeModifier() { return ProbeSweepTarget; }

        // Stealth - Defender
        public float StealthSignatureMod() { return HasStealth() ? Stealth.SignatureMulti : 0.0f; }
        public int StealthDetailsMod() { return HasStealth() ? Stealth.DetailsMod: 0; }
        public int StealthAttackMod(Weapon weapon, float distance) {
            if (Stealth == null) { return 0; }

            if (distance < weapon.MediumRange) {
                return Stealth.MediumRangeAttackMod;
            } else if (distance < weapon.LongRange) {
                return Stealth.LongRangeAttackMod;
            } else if (distance < weapon.MaxRange) {
                return Stealth.ExtremeRangeAttackMod;
            } else {
                return 0;
            }

        }
        public bool HasStealth() { return Stealth != null; }

        // Mimetic - Defender
        public float MimeticVisibilityMod() { return CurrentMimeticPips() * 0.05f; }
        public int MimeticAttackMod(EWState targetState, Weapon weapon, float distance) {
            int attackMod = targetState.CurrentMimeticPips();
            return attackMod;
        }
        //public int MimeticPips(float distance) {
        //    return CalculatePipCount(distance);
        //}       
        public int CurrentMimeticPips() {
            float distance = Vector3.Distance(actor.PreviousPosition, actor.CurrentPosition);
            return CalculatePipCount(distance);
        }
        public int MaxMimeticPips() { return Mimetic != null ? Mimetic.AttackMod : 0; }
        public bool HasMimetic() { return Mimetic != null; }
        private int CalculatePipCount(float distance) {
            if (Mimetic == null) { return 0;  }

            int hexesMoved = (int)Math.Ceiling(distance / 30f);
            Mod.Log.Debug($"  hexesMoved: {hexesMoved} = distanceMoved: {distance} / 30");

            int pips = Mimetic.AttackMod;
            int numDecays = (int)Math.Floor(hexesMoved / (float)Mimetic.HexesUntilDecay);
            Mod.Log.Debug($"  -- decays = {numDecays} from currentSteps: {hexesMoved} / decayPerStep: {Mimetic.HexesUntilDecay}");
            int currentMod = Math.Max(Mimetic.AttackMod - numDecays, 0);
            Mod.Log.Debug($"  -- current: {currentMod} = initial: {Mimetic.AttackMod} - decays: {numDecays}");

            return currentMod;
        }

        // ZoomVision - Attacker
        public int GetZoomVisionAttackMod(Weapon weapon, float distance) {
            if (ZoomVision == null || weapon.Type == WeaponType.Melee || weapon.Type == WeaponType.NotSet) { return 0; }

            int hexesBetween = (int)Math.Ceiling(distance / 30f);
            Mod.Log.Debug($"  hexesBetween: {hexesBetween} = distance: {distance} / 30");

            int pips = ZoomVision.AttackMod;
            int numDecays = (int)Math.Floor(hexesBetween / (float)ZoomVision.HexesUntilDecay);
            Mod.Log.Debug($"  -- decays = {numDecays} from currentSteps: {hexesBetween} / decayPerStep: {ZoomVision.HexesUntilDecay}");
            int currentMod = Math.Max(ZoomVision.AttackMod - numDecays, ZoomVision.AttackCap);
            Mod.Log.Debug($"  -- current: {currentMod} = initial: {ZoomVision.AttackMod} - decays: {numDecays}");

            return currentMod;
        }

        // HeatVision - Attacker
        public int GetHeatVisionAttackMod(ICombatant target, Weapon weapon, float distance) {
            if (HeatVision == null || weapon.Type == WeaponType.Melee || weapon.Type == WeaponType.NotSet) { return 0; }

            int currentMod = 0;
            Mech targetMech = target as Mech;
            if (targetMech != null) {
                double targetHeat = targetMech != null ? (double)targetMech.CurrentHeat : 0.0;
                int numDecays = (int)Math.Floor(targetHeat / HeatVision.HeatDivisor);
                Mod.Log.Debug($"  numDecays: {numDecays} = targetHeat: {targetHeat} / divisor: {HeatVision.HeatDivisor}");

                currentMod = Math.Max(HeatVision.AttackMod - numDecays, 0);
                Mod.Log.Debug($"  -- current: {currentMod} = initial: {HeatVision.AttackMod} - decays: {numDecays}");

            }

            return currentMod;
        }


        public override string ToString() { return $"sensorsCheck:{CurrentRoundEWCheck}"; }

        public string Details() {
            return $"tacticsModifier:{tacticsModifier}" +
                $"ecmShieldSigMod:{GetECMShieldSignatureModifier()} ecmShieldDetailsMod:{GetECMShieldDetailsModifier()} " +
                $"ecmJammedDetailsMod:{GetECMJammedDetailsModifier()} " +
                $"probeCarrier:{GetProbeSelfModifier()} probeSweepTarget:{GetTargetOfProbeModifier()}" +
                // TODO: FIXME!
                //$"staticSensorStealth:{StaticSensorStealth} decayingSensorStealth - {(DecayingSensorStealth != null ? DecayingSensorStealth.ToString() : "")} " +
                //$"sensorStealthAttackMulti:{(SensorStealthAttackMulti != null ? SensorStealthAttackMulti.ToString() : "1")} " +
                //$"staticVisionStealth:{StaticVisionStealth} decayingVisionStealth- {(DecayingVisionStealth != null ? DecayingVisionStealth.ToString() : "")} " +
                //$"visionStealthAttackMulti:{(VisionStealthAttackMulti != null ? VisionStealthAttackMulti.ToString() : "1")} " +
                //$"vismodeZoomMod:{vismodeZoomMod} vismodeZoomCap:{vismodeZoomCap} vismodeZoomStep:{vismodeZoomStep} " +
                //$"vismodeHeatMod:{vismodeHeatMod} vismodeHeatDiv:{vismodeHeatDivisor} " +
                $"sharesSensors:{sharesSensors}";
        }

        public void Update(AbstractActor actor) {
            EWStateHelper.UpdateStaticState(this, actor);
        }

        private static class EWStateHelper {

            public static void UpdateStaticState(EWState state, AbstractActor actor) {

                if (state == null || actor == null) { return; }

                string actorLabel = CombatantUtils.Label(actor);

                // Determine the bonus from the pilots tactics
                if (actor.GetPilot() != null) {
                    state.tacticsModifier = 1.0f + (SkillUtils.GetTacticsModifier(actor.GetPilot()) / 10.0f);
                } else {
                    Mod.Log.Info($"Actor:{CombatantUtils.Label(actor)} HAS NO PILOT!");
                }

            }
  
        }

    };
  
}
