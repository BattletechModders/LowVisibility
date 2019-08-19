using BattleTech;
using System;
using System.Collections.Generic;
using UnityEngine;
using us.frostraptor.modUtils;

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

    // <signatureMod>_<detailsMod>_<attackMod>
    class NarcEffect {
        public int AttackMod = 0;
        public float SignatureMod = 0.0f;
        public int DetailsMod = 0;
    }

    // <signatureMod>_<detailsMod>_<attackMod>
    class TagEffect {
        public int AttackMod = 0;
        public float SignatureMod = 0.0f;
        public int DetailsMod = 0;
    }

    public class EWState {

        private readonly AbstractActor actor;

        private int ewCheck = 0; // Raw value before any manipulation

        private int ecmCarrierMod = 0;
        private int shieldedByECMMod = 0;
        private int jammedByECMMod = 0;

        private int advSensorsCarrierMod = 0;

        private int probeCarrierMod = 0;
        private int pingedByProbeMod = 0;

        private Stealth stealth = null;
        private Mimetic mimetic = null;

        private ZoomVision zoomVision = null;
        private HeatVision heatVision = null;
        // TODO: Night vision here

        private NarcEffect narcEffect = null;
        private TagEffect tagEffect = null;

        private int tacticsMod = 0;

        private bool sharesVision = false;

        // Necessary for serialization
        public EWState() {}

        // Normal Constructor
        public EWState(AbstractActor actor) {
            this.actor = actor;

            // Pilot effects; cache and only read once
            if (actor.StatCollection.ContainsStatistic(ModStats.TacticsMod)) {
                tacticsMod = actor.StatCollection.GetStatistic(ModStats.TacticsMod).Value<int>();
                if (tacticsMod == 0 && actor.GetPilot() != null) {
                    tacticsMod = SkillUtils.GetTacticsModifier(actor.GetPilot());
                    actor.StatCollection.Set<int>(ModStats.TacticsMod, tacticsMod);
                }
            }

            // Ephemeral round check
            ewCheck = actor.StatCollection.ContainsStatistic(ModStats.CurrentRoundEWCheck) ?
                actor.StatCollection.GetStatistic(ModStats.CurrentRoundEWCheck).Value<int>() : 0;

            // ECM
            ecmCarrierMod = actor.StatCollection.ContainsStatistic(ModStats.ECMCarrier) ?
                actor.StatCollection.GetStatistic(ModStats.ECMCarrier).Value<int>() : 0;

            // Possible overlapping values
            jammedByECMMod = actor.StatCollection.ContainsStatistic(ModStats.JammedByECM) ?
                actor.StatCollection.GetStatistic(ModStats.JammedByECM).Value<int>() : 0;
            string ecmJammedValuesStat = ModStats.JammedByECM + "_AH_VALUES";
            if (actor.StatCollection.ContainsStatistic(ecmJammedValuesStat) && 
                actor.StatCollection.GetStatistic(ecmJammedValuesStat).Value<string>() != "") {
                string valuesStat = actor.StatCollection.GetStatistic(ecmJammedValuesStat).Value<string>();
                string[] values = valuesStat.Split(',');

                int highCount = 0;
                int highest = 0;
                int lowCount = 0;
                int lowest = 0;
                foreach (string value in values) {
                    // value format should be like LV_ECM_SHIELD:Cicada_Kraken_42004E3F:Weapon_PPC_PPC_0-STOCK:4
                    string[] tokens = value.Split(':');
                    string valS = tokens[3];
                    int val = Int32.Parse(valS);
                    if (val < 0) {
                        if (val < lowest) { lowest = val; }
                        lowCount++;
                    } else {
                        if (val > highest) { highest = val; }
                        highCount++;
                    }
                }
                //Mod.Log.Debug($"Setting ECM_JAMMED to highest:{highest} + count:{count} - 1");
                jammedByECMMod = (highest + (highCount - 1)) + (lowest - (lowCount - 1));
                if (jammedByECMMod < 0) { jammedByECMMod = 0; }
            }

            shieldedByECMMod = actor.StatCollection.ContainsStatistic(ModStats.ShieldedByECM) ?
                actor.StatCollection.GetStatistic(ModStats.ShieldedByECM).Value<int>() : 0;
            string ecmShieldValuesStat = ModStats.ShieldedByECM + "_AH_VALUES";
            if (actor.StatCollection.ContainsStatistic(ecmShieldValuesStat)) {
                string valuesStat = actor.StatCollection.GetStatistic(ecmShieldValuesStat).Value<string>();
                //Mod.Log.Debug($"Multiple values found for ECM_SHIELD: ({valuesStat})");
                string[] values = valuesStat.Split(',');

                int highCount = 0;
                int highest = 0;
                int lowCount = 0;
                int lowest = 0;
                foreach (string value in values) {
                    // value format should be like LV_ECM_SHIELD:Cicada_Kraken_42004E3F:Weapon_PPC_PPC_0-STOCK:4
                    string[] tokens = value.Split(':');
                    string valS = tokens[3];
                    int val = Int32.Parse(valS);
                    if (val < 0) {
                        if (val < lowest) { lowest = val; }
                        lowCount++;
                    } else {
                        if (val > highest) { highest = val; }
                        highCount++;
                    }

                    highCount++;
                }
                //Mod.Log.Debug($"Setting ECM_SHIELD to highest:{highest} + count:{count} - 1");
                shieldedByECMMod = (highest + (highCount - 1)) + (lowest - (lowCount - 1));
                if (shieldedByECMMod < 0) { shieldedByECMMod = 0; }
            }

            // Sensors
            advSensorsCarrierMod = actor.StatCollection.ContainsStatistic(ModStats.AdvancedSensors) ?
                actor.StatCollection.GetStatistic(ModStats.AdvancedSensors).Value<int>() : 0;

            // Probes
            probeCarrierMod = actor.StatCollection.ContainsStatistic(ModStats.ProbeCarrier) ?
                actor.StatCollection.GetStatistic(ModStats.ProbeCarrier).Value<int>() : 0;
            pingedByProbeMod = actor.StatCollection.ContainsStatistic(ModStats.PingedByProbe) ?
                actor.StatCollection.GetStatistic(ModStats.PingedByProbe).Value<int>() : 0;

            // Stealth - <signature_modifier>_<details_modifier>_<mediumAttackMod>_<longAttackmod>_<extremeAttackMod>
            if (actor.StatCollection.ContainsStatistic(ModStats.StealthEffect) &&
                actor.StatCollection.GetStatistic(ModStats.StealthEffect).Value<string>() != "") {
                string rawValue = actor.StatCollection.GetStatistic(ModStats.StealthEffect).Value<string>();
                string[] tokens = rawValue.Split('_');
                if (tokens.Length == 5) {
                    try {
                        stealth = new Stealth {
                            SignatureMulti = float.Parse(tokens[0]),
                            DetailsMod = Int32.Parse(tokens[1]),
                            MediumRangeAttackMod = Int32.Parse(tokens[2]),
                            LongRangeAttackMod = Int32.Parse(tokens[3]),
                            ExtremeRangeAttackMod = Int32.Parse(tokens[4])
                        };
                    } catch (Exception) {
                        Mod.Log.Info($"Failed to tokenize StealthEffect value: ({rawValue}). Discarding!");
                        stealth = null;
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
                        mimetic = new Mimetic {
                            VisibilityMulti = float.Parse(tokens[0]),
                            AttackMod = Int32.Parse(tokens[1]),
                            HexesUntilDecay = Int32.Parse(tokens[2]),
                        };
                    } catch (Exception) {
                        Mod.Log.Info($"Failed to tokenize Mimetic value: ({rawValue}). Discarding!");
                        mimetic = null;
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
                        zoomVision = new ZoomVision {
                            AttackMod = Int32.Parse(tokens[0]),
                            AttackCap = Int32.Parse(tokens[1]),
                            HexesUntilDecay = Int32.Parse(tokens[2])
                        };
                    } catch (Exception) {
                        Mod.Log.Info($"Failed to tokenize ZoomVision value: ({rawValue}). Discarding!");
                        mimetic = null;
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
                        heatVision = new HeatVision {
                            AttackMod = Int32.Parse(tokens[0]),
                            HeatDivisor = float.Parse(tokens[1]),
                    };
                    } catch (Exception) {
                        Mod.Log.Info($"Failed to tokenize HeatVision value: ({rawValue}). Discarding!");
                        mimetic = null;
                    }
                } else {
                    Mod.Log.Info($"WARNING: Invalid HeatVision value: ({rawValue}) found. Discarding!");
                }
            }

            // Narc effect - <signatureMod>_<detailsMod>_<attackMod>
            if (actor.StatCollection.ContainsStatistic(ModStats.NarcEffect) &&
                actor.StatCollection.GetStatistic(ModStats.NarcEffect).Value<string>() != "") {
                string rawValue = actor.StatCollection.GetStatistic(ModStats.NarcEffect).Value<string>();
                string[] tokens = rawValue.Split('_');
                if (tokens.Length == 3) {
                    try {
                        narcEffect = new NarcEffect {
                            SignatureMod = float.Parse(tokens[0]),
                            DetailsMod = Int32.Parse(tokens[1]),
                            AttackMod = Int32.Parse(tokens[2]), 
                        };
                    } catch (Exception) {
                        Mod.Log.Info($"Failed to tokenize NarcEffect value: ({rawValue}). Discarding!");
                        stealth = null;
                    }
                } else {
                    Mod.Log.Info($"WARNING: Invalid NarcEffect value: ({rawValue}) found. Discarding!");
                }
            }

            // Tag effect - <signatureMod>_<detailsMod>_<attackMod>
            if (actor.StatCollection.ContainsStatistic(ModStats.TagEffect) &&
                actor.StatCollection.GetStatistic(ModStats.TagEffect).Value<string>() != "") {
                string rawValue = actor.StatCollection.GetStatistic(ModStats.TagEffect).Value<string>();
                string[] tokens = rawValue.Split('_');
                if (tokens.Length == 3) {
                    try {
                        tagEffect = new TagEffect {
                            SignatureMod = float.Parse(tokens[0]),
                            DetailsMod = Int32.Parse(tokens[1]),
                            AttackMod = Int32.Parse(tokens[2]),
                        };
                    } catch (Exception) {
                        Mod.Log.Info($"Failed to tokenize TagEffect value: ({rawValue}). Discarding!");
                        stealth = null;
                    }
                } else {
                    Mod.Log.Info($"WARNING: Invalid TagEffect value: ({rawValue}) found. Discarding!");
                }
            }

            if (actor.StatCollection.ContainsStatistic(ModStats.SharesVision)) {
                sharesVision = actor.StatCollection.GetValue<bool>(ModStats.SharesVision);
            }

        }

        public int GetCurrentEWCheck() { return ewCheck + tacticsMod; }

        // ECM
        public int ECMJammedMod() { return jammedByECMMod;  }
        public float ECMSignatureMod(EWState attackerState) {
            int strength = 0;
            if (ecmCarrierMod > 0) {
                // We are a carrier - apply as a positive signature change (easier to locate)
                strength = ecmCarrierMod + attackerState.ProbeCarrierMod();
                Mod.Log.Trace($"Target:({CombatantUtils.Label(actor)}) has ECMCarrier:{ecmCarrierMod} - ProbeMod:{attackerState.ProbeCarrierMod()} " +
    $"from source:{CombatantUtils.Label(attackerState.actor)}");
            } else if (shieldedByECMMod > 0) {
                // We are shielded - apply as a negative signature change (harder to locate)
                strength = ecmCarrierMod - attackerState.ProbeCarrierMod();
                Mod.Log.Trace($"Target:({CombatantUtils.Label(actor)}) has ECMShield:{shieldedByECMMod} - ProbeMod:{attackerState.ProbeCarrierMod()} " +
    $"from source:{CombatantUtils.Label(attackerState.actor)}");
            } else { return 0f; }

            if (attackerState.PingedByProbeMod() > 0) { strength -= attackerState.PingedByProbeMod(); }
            if (attackerState.ProbeCarrierMod() > 0) { strength -= attackerState.ProbeCarrierMod(); }

            strength = Math.Max(0, strength);

            float sigMod = strength * 0.1f;
            if (sigMod != 0) { Mod.Log.Trace($"Target:({CombatantUtils.Label(actor)}) has ECMSignatureMod:{sigMod}"); }

            return sigMod;
        }
        public int ECMDetailsMod(EWState attackerState) {
            int strength = shieldedByECMMod > ecmCarrierMod ? shieldedByECMMod : ecmCarrierMod;

            if (attackerState.PingedByProbeMod() > 0) { strength -= attackerState.PingedByProbeMod(); }
            if (attackerState.ProbeCarrierMod() > 0) { strength -= attackerState.ProbeCarrierMod(); }

            strength = Math.Max(0, strength);

            return strength;
        }
        public bool IsECMCarrier() { return ecmCarrierMod > 0; }
        // Defender modifier
        public int ECMAttackMod(EWState attackerState) {
            int strength = shieldedByECMMod > ecmCarrierMod ? shieldedByECMMod : ecmCarrierMod;

            if (attackerState.PingedByProbeMod() > 0) { strength -= attackerState.PingedByProbeMod(); }
            if (attackerState.ProbeCarrierMod() > 0) { strength -= attackerState.ProbeCarrierMod(); }

            strength = Math.Max(0, strength);
            
            return strength;
        }

        // Sensors
        public int AdvancedSensorsMod() { return advSensorsCarrierMod; }
        public float GetSensorsRangeMulti() { return ewCheck / 20.0f + tacticsMod / 10.0f; }
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
        public int ProbeCarrierMod() { return probeCarrierMod; }
        public int PingedByProbeMod() { return pingedByProbeMod; }

        // Stealth
        public float StealthSignatureMod(EWState attackerState) {
            float strength = this.stealth != null ? this.stealth.SignatureMulti : 0.0f;

            if (attackerState.PingedByProbeMod() > 0) { strength -= (attackerState.PingedByProbeMod() * 0.05f); }
            if (attackerState.ProbeCarrierMod() > 0) { strength -= (attackerState.ProbeCarrierMod() * 0.05f); }

            strength = Math.Max(0, strength);
            
            return strength;
        }
        public int StealthDetailsMod() { return HasStealth() ? stealth.DetailsMod: 0; }
        // Defender modifier
        public int StealthAttackMod(EWState attackerState, Weapon weapon, float distance) {
            if (stealth == null) { return 0; }

            int strength = 0;
            if (distance < weapon.MediumRange) {
                strength = stealth.MediumRangeAttackMod;
            } else if (distance < weapon.LongRange) {
                strength = stealth.LongRangeAttackMod;
            } else if (distance < weapon.MaxRange) {
                strength = stealth.ExtremeRangeAttackMod;
            }

            if (attackerState.PingedByProbeMod() > 0) { strength -= attackerState.PingedByProbeMod(); }
            if (attackerState.ProbeCarrierMod() > 0) { strength -= attackerState.ProbeCarrierMod(); }

            strength = Math.Max(0, strength);

            return strength;
        }
        public bool HasStealth() { return stealth != null; }

        // Mimetic
        public float MimeticVisibilityMod(EWState attackerState) {
            float strength = CurrentMimeticPips() * 0.05f;

            if (attackerState.PingedByProbeMod() > 0) { strength -= (attackerState.PingedByProbeMod() * 0.05f); }
            if (attackerState.ProbeCarrierMod() > 0) { strength -= (attackerState.ProbeCarrierMod() * 0.05f); }

            strength = Math.Max(0, strength);

            return strength;
        }
        // Defender modifier
        public int MimeticAttackMod(EWState attackerState) {
            if (mimetic == null) { return 0;  }

            int strength = CurrentMimeticPips();

            if (attackerState.PingedByProbeMod() > 0) { strength -= attackerState.PingedByProbeMod(); }
            if (attackerState.ProbeCarrierMod() > 0) { strength -= attackerState.ProbeCarrierMod(); }

            strength = Math.Max(0, strength);

            return strength;
        }

        public int CurrentMimeticPips(float distance) {
            return CalculatePipCount(distance);
        }
        public int CurrentMimeticPips() {
            float distance = Vector3.Distance(actor.PreviousPosition, actor.CurrentPosition);
            return CalculatePipCount(distance);
        }
        public int MaxMimeticPips() { return mimetic != null ? mimetic.AttackMod : 0; }
        public bool HasMimetic() { return mimetic != null; }
        private int CalculatePipCount(float distance) {
            if (mimetic == null) { return 0;  }

            int hexesMoved = (int)Math.Ceiling(distance / 30f);
//            Mod.Log.Debug($"  hexesMoved: {hexesMoved} = distanceMoved: {distance} / 30");

            int pips = mimetic.AttackMod;
            int numDecays = (int)Math.Floor(hexesMoved / (float)mimetic.HexesUntilDecay);
            Mod.Log.Trace($"  -- decays = {numDecays} from currentSteps: {hexesMoved} / decayPerStep: {mimetic.HexesUntilDecay}");
            int currentMod = Math.Max(mimetic.AttackMod - numDecays, 0);
            Mod.Log.Trace($"  -- current: {currentMod} = initial: {mimetic.AttackMod} - decays: {numDecays}");

            return currentMod;
        }

        // ZoomVision - Attacker
        public int GetZoomVisionAttackMod(Weapon weapon, float distance) {
            if (zoomVision == null || weapon.Type == WeaponType.Melee || weapon.Type == WeaponType.NotSet) { return 0; }

            int hexesBetween = (int)Math.Ceiling(distance / 30f);
            Mod.Log.Trace($"  hexesBetween: {hexesBetween} = distance: {distance} / 30");

            int pips = zoomVision.AttackMod;
            int numDecays = (int)Math.Floor(hexesBetween / (float)zoomVision.HexesUntilDecay);
            Mod.Log.Trace($"  -- decays = {numDecays} from currentSteps: {hexesBetween} / decayPerStep: {zoomVision.HexesUntilDecay}");
            int currentMod = Math.Max(zoomVision.AttackMod - numDecays, zoomVision.AttackCap);
            Mod.Log.Trace($"  -- current: {currentMod} = initial: {zoomVision.AttackMod} - decays: {numDecays}");

            return currentMod;
        }

        // HeatVision - Attacker
        public int GetHeatVisionAttackMod(AbstractActor target, Weapon weapon) {
            if (heatVision == null || weapon.Type == WeaponType.Melee || weapon.Type == WeaponType.NotSet) { return 0; }

            int currentMod = 0;
            if (target is Mech targetMech) {
                double targetHeat = targetMech != null ? (double)targetMech.CurrentHeat : 0.0;
                int numDecays = (int)Math.Floor(targetHeat / heatVision.HeatDivisor);
                Mod.Log.Debug($"  numDecays: {numDecays} = targetHeat: {targetHeat} / divisor: {heatVision.HeatDivisor}");

                currentMod = Math.Max(heatVision.AttackMod - numDecays, 0);
                Mod.Log.Debug($"  -- current: {currentMod} = initial: {heatVision.AttackMod} - decays: {numDecays}");

            }

            return currentMod;
        }

        // NARC effects
        public bool IsNarced(EWState attackerState) {
            return narcEffect != null;
        }
        public int NarcAttackMod(EWState attackerState) {
            int val = 0;
            if (narcEffect != null) {
                val = Math.Max(0, narcEffect.AttackMod - ECMAttackMod(attackerState));
            }
            return val;
        }
        public int NarcDetailsMod(EWState attackerState) {
            int val = 0;
            if (narcEffect != null) {
                val = Math.Max(0, narcEffect.DetailsMod - ECMDetailsMod(attackerState));
            }
            return val;
        }
        public float NarcSignatureMod(EWState attackerState) {
            float val = 0;
            if (narcEffect != null) {
                val = (float)Math.Max(0.0f, narcEffect.SignatureMod - ECMDetailsMod(attackerState) * 0.1f);
            }
            return val;
        }

        // TAG effects
        public bool IsTagged(EWState attackerState) {
            return tagEffect != null && Math.Max(0, tagEffect.AttackMod - MimeticAttackMod(attackerState)) > 0;
        }
        public int TagAttackMod(EWState attackerState) {
            int val = 0;
            if (tagEffect != null) {
                val = Math.Max(0, tagEffect.AttackMod - MimeticAttackMod(attackerState));
            }
            return val;
        }
        public int TagDetailsMod(EWState attackerState) {
            int val = 0;
            if (tagEffect != null) {
                val = Math.Max(0, tagEffect.DetailsMod - MimeticAttackMod(attackerState));
            }
            return val;
        }
        public float TagSignatureMod(EWState attackerState) {
            float val = 0;
            if (tagEffect != null) {
                val = (float)Math.Max(0.0f, tagEffect.SignatureMod - MimeticVisibilityMod(attackerState));
            }
            return val;
        }

        public bool SharesVision() { return sharesVision; }

        // Misc
        public override string ToString() { return $"sensorsCheck:{ewCheck}"; }

        public void BuildCheckTooltip(List<string> details) {
            
            details.Add("Details Check:");

            List<string> toBuild = new List<string>();
            if (ewCheck >= 0) {
                toBuild.Add($"<color=#00FF00>{ewCheck:+0}</color>");
            } else {
                toBuild.Add($"<color=#FF0000>{ewCheck:0}</color>");
            }

            toBuild.Add($" + (Tactics: <color=#00FF00>{tacticsMod:+0}</color>)");

            if (AdvancedSensorsMod() != 0) {
                if (AdvancedSensorsMod() > 0) {
                    toBuild.Add($" + (Sensors: <color=#00FF00>{AdvancedSensorsMod():+0}</color>)");
                } else {
                    toBuild.Add($" + (Sensors: <color=#FF0000>{AdvancedSensorsMod():0}</color>)");
                }
            }

            details.Add(String.Join("", toBuild.ToArray()));
        }

        public string Details() {
            return $"tacticsBonus:{tacticsMod}" +
                //$"ecmShieldSigMod:{GetECMShieldSignatureModifier()} ecmShieldDetailsMod:{GetECMShieldDetailsModifier()} " +
                $"ecmJammedDetailsMod:{ECMJammedMod()} " +
                $"probeCarrier:{ProbeCarrierMod()} probeSweepTarget:{PingedByProbeMod()}";
                // TODO: FIXME!
                //$"staticSensorStealth:{StaticSensorStealth} decayingSensorStealth - {(DecayingSensorStealth != null ? DecayingSensorStealth.ToString() : "")} " +
                //$"sensorStealthAttackMulti:{(SensorStealthAttackMulti != null ? SensorStealthAttackMulti.ToString() : "1")} " +
                //$"staticVisionStealth:{StaticVisionStealth} decayingVisionStealth- {(DecayingVisionStealth != null ? DecayingVisionStealth.ToString() : "")} " +
                //$"visionStealthAttackMulti:{(VisionStealthAttackMulti != null ? VisionStealthAttackMulti.ToString() : "1")} " +
                //$"vismodeZoomMod:{vismodeZoomMod} vismodeZoomCap:{vismodeZoomCap} vismodeZoomStep:{vismodeZoomStep} " +
                //$"vismodeHeatMod:{vismodeHeatMod} vismodeHeatDiv:{vismodeHeatDivisor} " +
        }

    };
  
}
