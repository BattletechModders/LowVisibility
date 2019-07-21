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
        
        private int TacticsBonus = 1;

        // If true, actor shares sensor information with all allies
        public bool SharesSensors = false;

        // Necessary for serialization
        public EWState() {}

        // Normal Constructor
        public EWState(AbstractActor actor) {
            this.actor = actor;

            // Pilot effects; cache and only read once
            if (actor.StatCollection.ContainsStatistic(ModStats.TacticsMod)) {
                TacticsBonus = actor.StatCollection.GetStatistic(ModStats.TacticsMod).Value<int>();
                if (TacticsBonus == 0 && actor.GetPilot() != null) {
                    TacticsBonus = SkillUtils.GetTacticsModifier(actor.GetPilot());
                }
            }

            // Emphereal round check
            CurrentRoundEWCheck = actor.StatCollection.ContainsStatistic(ModStats.CurrentRoundEWCheck) ?
                actor.StatCollection.GetStatistic(ModStats.CurrentRoundEWCheck).Value<int>() : 0;

            // ECM
            ECMCarrier = actor.StatCollection.ContainsStatistic(ModStats.ECMCarrier) ?
                actor.StatCollection.GetStatistic(ModStats.ECMCarrier).Value<int>() : 0;

            // Possible overlapping values
            ECMJammed = actor.StatCollection.ContainsStatistic(ModStats.ECMJammed) ?
                actor.StatCollection.GetStatistic(ModStats.ECMJammed).Value<int>() : 0;
            string ecmJammedValuesStat = ModStats.ECMJammed + "_AH_VALUES";
            if (actor.StatCollection.ContainsStatistic(ecmJammedValuesStat) && 
                actor.StatCollection.GetStatistic(ecmJammedValuesStat).Value<string>() != "") {
                string valuesStat = actor.StatCollection.GetStatistic(ecmJammedValuesStat).Value<string>();
                //Mod.Log.Debug($"Multiple values found for ECM_JAMMED: ({valuesStat})");
                string[] values = valuesStat.Split(',');
                int count = 0;
                int highest = 0;
                foreach (string value in values) {
                    // value format should be like LV_ECM_SHIELD:Cicada_Kraken_42004E3F:Weapon_PPC_PPC_0-STOCK:4
                    string[] tokens = value.Split(':');
                    string valS = tokens[3];
                    int val = Int32.Parse(valS);
                    if (val > highest) { highest = val;  }
                    count++;
                }
                //Mod.Log.Debug($"Setting ECM_JAMMED to highest:{highest} + count:{count} - 1");
                ECMJammed = highest + (count - 1);
            }

            ECMShield = actor.StatCollection.ContainsStatistic(ModStats.ECMShield) ?
                actor.StatCollection.GetStatistic(ModStats.ECMShield).Value<int>() : 0;
            string ecmShieldValuesStat = ModStats.ECMShield + "_AH_VALUES";
            if (actor.StatCollection.ContainsStatistic(ecmShieldValuesStat)) {
                string valuesStat = actor.StatCollection.GetStatistic(ecmShieldValuesStat).Value<string>();
                //Mod.Log.Debug($"Multiple values found for ECM_SHIELD: ({valuesStat})");
                string[] values = valuesStat.Split(',');
                int count = 0;
                int highest = 0;
                foreach (string value in values) {
                    // value format should be like LV_ECM_SHIELD:Cicada_Kraken_42004E3F:Weapon_PPC_PPC_0-STOCK:4
                    string[] tokens = value.Split(':');
                    string valS = tokens[3];
                    int val = Int32.Parse(valS);
                    if (val > highest) { highest = val; }
                    count++;
                }
                //Mod.Log.Debug($"Setting ECM_SHIELD to highest:{highest} + count:{count} - 1");
                ECMShield = highest + (count - 1);
            }

            // Sensors
            AdvancedSensors = actor.StatCollection.ContainsStatistic(ModStats.AdvancedSensors) ?
                actor.StatCollection.GetStatistic(ModStats.AdvancedSensors).Value<int>() : 0;
            SharesSensors = actor.StatCollection.ContainsStatistic(ModStats.SharesSensors) ?
                actor.StatCollection.GetStatistic(ModStats.SharesSensors).Value<bool>() : false;

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
                if (tokens.Length == 2) {
                    try {
                        HeatVision = new HeatVision {
                            AttackMod = Int32.Parse(tokens[0]),
                            HeatDivisor = float.Parse(tokens[1]),
                    };
                    } catch (Exception) {
                        Mod.Log.Info($"Failed to tokenize HeatVision value: ({rawValue}). Discarding!");
                        Mimetic = null;
                    }
                } else {
                    Mod.Log.Info($"WARNING: Invalid HeatVision value: ({rawValue}) found. Discarding!");
                }
            }

            //EWStateHelper.UpdateStaticState(this, actor);
        }

        public int GetCurrentEWCheck() { return CurrentRoundEWCheck + TacticsBonus; }

        // ECM
        public int GetECMJammedDetailsModifier() { return ECMJammed;  }
        public float GetECMShieldSignatureModifier() { return ECMShield > ECMCarrier ? ECMShield * 0.05f : ECMCarrier * 0.05f; }
        public int GetECMShieldDetailsModifier() { return ECMShield > ECMCarrier ? ECMShield : ECMCarrier;  }
        // Defender modifier
        public int GetECMShieldAttackModifier(EWState attackerState) {
            int ECMMod = ECMShield > ECMCarrier ? ECMShield : ECMCarrier;
            Mod.Log.Debug($"Target:({CombatantUtils.Label(actor)}) has ECMAttackMod:{ECMMod} - ProbeMod:{attackerState.GetProbeSelfModifier()} " +
                $"from source:{CombatantUtils.Label(attackerState.actor)}");
            return Math.Max(0, ECMMod - attackerState.GetProbeSelfModifier());
        }

        // Sensors
        public int GetAdvancedSensorsMod() { return AdvancedSensors; }
        public float GetSensorsRangeMulti() { return CurrentRoundEWCheck / 20.0f + TacticsBonus / 10.0f; }
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

        // Stealth
        public float StealthSignatureMod() { return HasStealth() ? Stealth.SignatureMulti : 0.0f; }
        public int StealthDetailsMod() { return HasStealth() ? Stealth.DetailsMod: 0; }
        // Defender modifier
        public int StealthAttackMod(EWState attackerState, Weapon weapon, float distance) {
            if (Stealth == null) { return 0; }

            int stealthMod = 0;
            if (distance < weapon.MediumRange) {
                stealthMod = Stealth.MediumRangeAttackMod;
            } else if (distance < weapon.LongRange) {
                stealthMod = Stealth.LongRangeAttackMod;
            } else if (distance < weapon.MaxRange) {
                stealthMod = Stealth.ExtremeRangeAttackMod;
            }

            return Math.Max(0, stealthMod - attackerState.GetProbeSelfModifier());
        }
        public bool HasStealth() { return Stealth != null; }

        // Mimetic
        public float MimeticVisibilityMod() { return CurrentMimeticPips() * 0.05f; }
        // Defender modifier
        public int MimeticAttackMod(EWState attackerState, Weapon weapon, float distance) {
            if (Mimetic == null) { return 0;  }

            int mimeticMod = CurrentMimeticPips();

            return Math.Max(0, mimeticMod - attackerState.GetProbeSelfModifier());
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
        public int GetHeatVisionAttackMod(AbstractActor target, Weapon weapon) {
            if (HeatVision == null || weapon.Type == WeaponType.Melee || weapon.Type == WeaponType.NotSet) { return 0; }

            int currentMod = 0;
            if (target is Mech targetMech) {
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
            return $"tacticsBonus:{TacticsBonus}" +
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
                $"sharesSensors:{SharesSensors}";
        }

        public void Update(AbstractActor actor) {
            EWStateHelper.UpdateStaticState(this, actor);
        }

        private static class EWStateHelper {

            public static void UpdateStaticState(EWState state, AbstractActor actor) {

                if (state == null || actor == null) { return; }

                string actorLabel = CombatantUtils.Label(actor);

                // Determine the bonus from the pilots tactics


            }
  
        }

    };
  
}
