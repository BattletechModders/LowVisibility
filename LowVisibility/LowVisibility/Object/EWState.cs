using BattleTech;
using HBS.Collections;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using us.frostraptor.modUtils;
using us.frostraptor.modUtils.math;

namespace LowVisibility.Object {

    public class DecayingStealth {
        public int InitialMod = 0;
        public int StepsUntilDecay = 0;
        public override string ToString() { return $"initial: {InitialMod} stepsUntilDecay: {StepsUntilDecay}";  }
    }

    public class EWState {

        private readonly AbstractActor actor;

        public int sensorsCheck = 0;

        public string parentGUID = null;

        public float sensorsBaseRange = 0.0f;

        public int ECMShield = 0;
        public int ECMCarrier = 0;
        public int ECMJammed = 0;

        public int StaticSensorStealth = 0;
        public DecayingStealth DecayingSensorStealth = null;

        public int StaticVisionStealth = 0;
        public DecayingStealth DecayingVisionStealth = null;
        
        public int ecmMod = 0;
        public int probeMod = 0;
        public int stealthMod = 0;

        public int vismodeZoomMod = 0;
        public int vismodeZoomCap = 0;
        public int vismodeZoomStep = 0;

        public int vismodeHeatMod = 0;
        public int vismodeHeatDivisor = 0;

        // Modifier for stealth movement - base modifier (0), reduced by -1 for each (1) hexes moved
        public int[] stealthMoveMod = new int[] { 0, 0 };

        // The amount of tactics bonus to the sensor check
        public int tacticsBonus = 0;

        // Whether this actor will share sensor data with others
        public bool sharesSensors = false;

        // Necessary for serialization
        public EWState() {}

        // Normal Constructor
        public EWState(AbstractActor actor) {
            this.actor = actor;

            tacticsBonus = actor.StatCollection.ContainsStatistic(ModStats.TacticsMod) ?
                actor.StatCollection.GetStatistic(ModStats.TacticsMod).Value<int>() : 0;

            sensorsCheck = actor.StatCollection.ContainsStatistic(ModStats.SensorCheck) ? 
                actor.StatCollection.GetStatistic(ModStats.SensorCheck).Value<int>() : 0;

            ECMJammed = actor.StatCollection.ContainsStatistic(ModStats.ECMJammed) ?
                actor.StatCollection.GetStatistic(ModStats.ECMJammed).Value<int>() : 0;
            ECMShield = actor.StatCollection.ContainsStatistic(ModStats.ECMShield) ?
                actor.StatCollection.GetStatistic(ModStats.ECMShield).Value<int>() : 0;
            ECMCarrier = actor.StatCollection.ContainsStatistic(ModStats.ECMCarrier) ?
                actor.StatCollection.GetStatistic(ModStats.ECMCarrier).Value<int>() : 0;

            // Sensor stealth
            StaticSensorStealth = actor.StatCollection.ContainsStatistic(ModStats.StaticSensorStealth) ?
                actor.StatCollection.GetStatistic(ModStats.StaticSensorStealth).Value<int>() : 0;
            if (actor.StatCollection.ContainsStatistic(ModStats.DecayingSensorStealthValue) &&
                actor.StatCollection.GetStatistic(ModStats.DecayingSensorStealthValue).Value<string>() != "") {
                string rawValue = actor.StatCollection.GetStatistic(ModStats.DecayingSensorStealthValue).Value<string>();
                string[] tokens = rawValue.Split('_');
                if (tokens.Length == 2) {
                    try {
                        DecayingSensorStealth = new DecayingStealth {
                            InitialMod = Int32.Parse(tokens[0]),
                            StepsUntilDecay = Int32.Parse(tokens[1])
                        };
                    } catch (Exception) {
                        Mod.Log.Info($"Failed to tokenize StealthSensorCharge value: ({rawValue}). Discarding!");
                        DecayingSensorStealth = null;
                    }
                } else {
                    Mod.Log.Info($"WARNING: Invalid StealthSensorCharge value: ({rawValue}) found. Discarding!");
                }
            }

            // Vision stealth
            StaticVisionStealth = actor.StatCollection.ContainsStatistic(ModStats.StaticVisionStealth) ?
                actor.StatCollection.GetStatistic(ModStats.StaticVisionStealth).Value<int>() : 0;
            if (actor.StatCollection.ContainsStatistic(ModStats.DecayingVisionStealthValue) &&
                actor.StatCollection.GetStatistic(ModStats.DecayingVisionStealthValue).Value<string>() != "") {
                string rawValue = actor.StatCollection.GetStatistic(ModStats.DecayingVisionStealthValue).Value<string>();
                string[] tokens = rawValue.Split('_');
                if (tokens.Length == 2) {
                    try {
                        DecayingVisionStealth = new DecayingStealth {
                            InitialMod = Int32.Parse(tokens[0]),
                            StepsUntilDecay = Int32.Parse(tokens[1])
                        };
                    } catch (Exception) {
                        Mod.Log.Info($"Failed to tokenize VisionStealthCharge value: ({rawValue}). Discarding!");
                        DecayingVisionStealth = null;
                    }
                } else {
                    Mod.Log.Info($"WARNING: Invalid VisionStealthCharge value: ({rawValue}) found. Discarding!");
                }
            }

            ecmMod = actor.StatCollection.ContainsStatistic(ModStats.Jammer) ?
                actor.StatCollection.GetStatistic(ModStats.Jammer).Value<int>() : 0;
            probeMod = actor.StatCollection.ContainsStatistic(ModStats.Probe) ?
                actor.StatCollection.GetStatistic(ModStats.Probe).Value<int>() : 0;
            stealthMod = actor.StatCollection.ContainsStatistic(ModStats.Stealth) ?
                actor.StatCollection.GetStatistic(ModStats.Stealth).Value<int>() : 0;

            SetSensorBaseRange(actor);
            //EWStateHelper.UpdateStaticState(this, actor);
        }

        private void SetSensorBaseRange(AbstractActor actor) {
            if (actor.GetType() == typeof(Mech)) {
                sensorsBaseRange = Mod.Config.SensorRangeMechType * 30.0f;
            } else if (actor.GetType() == typeof(Vehicle)) {
                sensorsBaseRange = Mod.Config.SensorRangeVehicleType * 30.0f;
            } else if (actor.GetType() == typeof(Turret)) {
                sensorsBaseRange = Mod.Config.SensorRangeTurretType * 30.0f;
            } else {
                sensorsBaseRange = Mod.Config.SensorRangeUnknownType * 30.0f;
            }
        }

        public int GetECMJammedDetailsModifier() { return ECMJammed;  }

        public float GetECMShieldSignatureModifier() { return ECMShield > ECMCarrier ? ECMShield * 0.05f : ECMCarrier * 0.05f; }
        public int GetECMShieldDetailsModifier() { return ECMShield > ECMCarrier ? ECMShield : ECMCarrier;  }
        public int GetECMShieldAttackModifier() { return ECMShield > ECMCarrier ? ECMShield : ECMCarrier; }

        // Sensor Stealth
        public float GetSensorStealthSignatureModifier() { return CurrentSensorStealthPips() * 0.05f; }
        public int GetSensorStealthDetailsModifier() { return CurrentSensorStealthPips(); }

        public int CurrentSensorStealthPips(float distance) { return CurrentStealthPips(StaticSensorStealth, DecayingSensorStealth, distance); }
        public int CurrentSensorStealthPips() {
            float distance = Vector3.Distance(actor.PreviousPosition, actor.CurrentPosition);
            return CurrentStealthPips(StaticSensorStealth, DecayingSensorStealth, distance);
        }
        public int MaxSensorStealthPips() { return StaticSensorStealth + (DecayingSensorStealth != null ? DecayingSensorStealth.InitialMod : 0); }

        // Vision Stealth
        public float GetVisualStealthVisibilityModifier() { return CurrentVisionStealthPips() * 0.05f; }
        public int GetVisualStealthDetailsModifier() { return CurrentVisionStealthPips(); }

        public int CurrentVisionStealthPips(float distance) { return CurrentStealthPips(StaticVisionStealth, DecayingVisionStealth, distance); }
        public int CurrentVisionStealthPips() {
            float distance = Vector3.Distance(actor.PreviousPosition, actor.CurrentPosition);
            return CurrentStealthPips(StaticVisionStealth, DecayingVisionStealth, distance);
        }
        public int MaxVisionStealthPips() { return StaticVisionStealth + (DecayingVisionStealth != null ? DecayingVisionStealth.InitialMod : 0); }

        private int CurrentStealthPips(int staticStealth, DecayingStealth decay, float distance) {
            int stepsMoved = (int)Math.Ceiling(distance / 30f);
            Mod.Log.Debug($"  stepsMoved: {stepsMoved} = distanceMoved: {distance} / 30");

            int pips = staticStealth;
            Mod.Log.Debug($"  Actor:({CombatantUtils.Label(actor)}) has basePips: {pips}");
            if (decay != null) {
                Mod.Log.Debug($"  decaying sensor stealth");
                int numDecays = (int)Math.Floor(stepsMoved / (float)decay.StepsUntilDecay);
                Mod.Log.Debug($"  -- decays = {numDecays} from currentSteps: {stepsMoved} / decayPerStep: {decay.StepsUntilDecay}");
                int currentMod = Math.Max(decay.InitialMod - numDecays, 0);
                Mod.Log.Debug($"  -- current: {currentMod} = initial: {decay.InitialMod} - decays: {numDecays}");
                pips += currentMod;
            }

            return pips;
        }


        // TODO: Lash this into serialization
        public void RefreshAfterSave(CombatGameState Combat) {
            AbstractActor actor = Combat.FindActorByGUID(parentGUID);
            SetSensorBaseRange(actor);
            EWStateHelper.UpdateStaticState(this, actor);
        }

        public bool HasStealthMoveMod() {
            bool hasMod = stealthMoveMod != null && stealthMoveMod[0] != 0;
            return hasMod;
        }

        public bool HasStealth() {
            return StaticSensorStealth != 0 || DecayingSensorStealth != null || StaticVisionStealth != 0 || DecayingVisionStealth != null;
        }

        public int CalculateStealthMoveMod(AbstractActor owner) {
            int moveMod = 0;
            if (owner != null && this.stealthMoveMod[0] != 0) {
                moveMod = HexUtils.DecayingModifier(this.stealthMoveMod[0], 0, this.stealthMoveMod[1], owner.DistMovedThisRound);                
                //LowVisibility.Logger.Debug($"  StealthMoveMod - actor:{CombatantUtils.Label(owner)} has moveMod:{moveMod}");
            }
            return moveMod;
        }


        public VisionModeModifer CalculateVisionModeModifier(ICombatant target, float distance, Weapon weapon) {

            if (weapon.Type == WeaponType.Melee || weapon.Type == WeaponType.NotSet) { return new VisionModeModifer(); }
            Mod.Log.Debug($" Source has zoomMod:{vismodeZoomMod} heatMod:{vismodeHeatMod}");

            int zoomMod = 0;
            if (vismodeZoomMod != 0 || vismodeZoomCap != 0) {
                zoomMod = HexUtils.DecayingModifier(this.vismodeZoomMod, this.vismodeZoomCap, this.vismodeZoomStep, distance);
                Mod.Log.Debug($" Zoom mod calculated as:{zoomMod} for distance:{distance}");
            }

            Mech targetMech = target as Mech;
            double targetHeat = targetMech != null ? (double)targetMech.CurrentHeat : 0.0;
            //LowVisibility.Logger.Debug($" Target:{CombatantUtils.Label(target)} has currentHeat:{targetHeat}");
            int heatMod = 0;
            if (vismodeHeatMod != 0 && targetHeat > 0) {
                int heatModRaw = (int)Math.Floor(targetHeat / this.vismodeHeatDivisor) * this.vismodeHeatMod;
                //LowVisibility.Logger.Debug($" Heat steps are:{heatModRaw}");
                heatMod = heatModRaw <= Mod.Config.HeatVisionMaxBonus ? heatModRaw : Mod.Config.HeatVisionMaxBonus;
                // Bonuses are negatives, penalties are positives
                heatMod = -1 * heatMod;
                Mod.Log.Trace($" Heat mod calculated as:{heatMod}");
            }

            VisionModeModifer vmod = new VisionModeModifer();
            if (zoomMod != 0 && heatMod != 0) {
                vmod.modifier = zoomMod < heatMod ? zoomMod : heatMod;               
                //LowVisibility.Logger.Debug($" Source has both heat and vision mod, increasing {vmod.modifier} to {vmod.modifier - 1}");
                vmod.modifier = vmod.modifier - 1;

                vmod.label = zoomMod < heatMod ? "Zoom Vision" : "Heat Vision";
            } else if (zoomMod != 0 && heatMod == 0) {
                vmod.modifier = zoomMod;
                vmod.label = "Zoom Vision";
            } else if (zoomMod == 0 && heatMod != 0) {
                vmod.modifier = heatMod;
                vmod.label = "Heat Vision";
            }

            return vmod;
        }

        public float SensorCheckRangeMultiplier() { return sensorsCheck / 20.0f; }

        public override string ToString() { return $"sensorsCheck:{sensorsCheck} ecmMod:{ecmMod}"; }

        public string Details() {
            return $"tacticsBonus:{tacticsBonus} ecmMod:{ecmMod} probeMod:{probeMod} stealthMod:{stealthMod} " +
                $"ecmShieldSigMod:{GetECMShieldSignatureModifier()} ecmShieldDetailsMod:{GetECMShieldDetailsModifier()} ecmShieldAttackMod:{GetECMShieldAttackModifier()} " +
                $"ecmJammedDetailsMod:{GetECMJammedDetailsModifier()} " +
                $"stealthMoveMod:{stealthMoveMod[0]}/{stealthMoveMod[1]} " +
                $"staticSensorStealth:{StaticSensorStealth} decayingSensorStealth - {(DecayingSensorStealth != null ? DecayingSensorStealth.ToString() : "")}" +
                $"staticVisionStealth:{StaticVisionStealth} decayingVisionStealth- {(DecayingVisionStealth != null ? DecayingVisionStealth.ToString() : "")}" +
                $"vismodeZoomMod:{vismodeZoomMod} vismodeZoomCap:{vismodeZoomCap} vismodeZoomStep:{vismodeZoomStep} " +
                $"vismodeHeatMod:{vismodeHeatMod} vismodeHeatDiv:{vismodeHeatDivisor} " +
                $"sharesSensors:{sharesSensors}";
        }

        public void Update(AbstractActor actor) {
            EWStateHelper.UpdateStaticState(this, actor);
        }

        private static class EWStateHelper {

            public static void UpdateStaticState(EWState state, AbstractActor actor) {

                if (state == null || actor == null) { return; }

                string actorLabel = CombatantUtils.Label(actor);

                Dictionary<string, TagSet> cTags = actor?.allComponents?
                    .Where(c => c?.componentDef?.ComponentTags != null)
                    .GroupBy(c => c.componentDef)
                    .Select(c => c.First())
                    .ToDictionary(c => c.componentDef.Description.Id, v => v?.componentDef?.ComponentTags);
                foreach (KeyValuePair<string, TagSet> kv in cTags) {
                    foreach (string tag in kv.Value) {

                        if (tag == null || tag.Equals("") || tag.ToLower() == null) { continue; }

                        string tagLower = tag.ToLower();

                        // TODO: Move to a debug block that prints out on unit spawn
                        //if (tagLower.StartsWith(ModStats.TagPrefixJammer)) {
                        //    Mod.Log.Debug($"Actor:{actorLabel} has JAMMER component:{kv.Key} with tag:{tag}");
                        //    ParseJammer(ref state, tag);
                        //} else if (tagLower.StartsWith(ModStats.TagPrefixProbe)) {
                        //    Mod.Log.Debug($"Actor:{actorLabel} has PROBE component:{kv.Key} with tag:{tag}");
                        //    ParseProbe(ref state, tag);
                        //} else if (tagLower.StartsWith(ModStats.TagPrefixProbeBoost)) {
                        //    Mod.Log.Debug($"Actor:{actorLabel} has PROBE BOOST component:{kv.Key} with tag:{tag}");
                        //    ParseProbeBoost(ref state, tag);
                        //} else if (tagLower.StartsWith(ModStats.TagPrefixStealth)) {
                        //    Mod.Log.Debug($"Actor:{actorLabel} has STEALTH component:{kv.Key} with tag:{tag}");
                        //    ParseStealth(ref state, tag);
                        //} else if (tagLower.StartsWith(ModStats.TagPrefixScrambler)) {
                        //    Mod.Log.Debug($"Actor:{actorLabel} has SCRAMBLER component:{kv.Key} with tag:{tag}");
                        //    ParseScrambler(ref state, tag);
                        //} else if (tagLower.StartsWith(ModStats.TagPrefixStealthRangeMod)) {
                        //    Mod.Log.Debug($"Actor:{actorLabel} has STEALTH_RANGE_MOD component:{kv.Key} with tag:{tag}");
                        //    ParseStealthRangeMod(ref state, tag);
                        //} else if (tagLower.StartsWith(ModStats.TagPrefixStealthMoveMod)) {
                        //    Mod.Log.Debug($"Actor:{actorLabel} has STEALTH_MOVE_MOD component:{kv.Key} with tag:{tag}");
                        //    ParseStealthMoveMod(ref state, tag);
                        //} else if (tagLower.Equals(ModStats.TagSharesSensors)) {
                        //    Mod.Log.Debug($"Actor:{actorLabel} shares sensors due to component:{kv.Key} with tag:{tag}");
                        //    state.sharesSensors = true;
                        //} else if (tagLower.StartsWith(ModStats.TagPrefixVismodeZoom)) {
                        //    Mod.Log.Debug($"Actor:{actorLabel} has ZOOM VISION component:{kv.Key} with tag:{tag}");
                        //    ParseVismodeZoom(ref state, tag);
                        //} else if (tagLower.StartsWith(ModStats.TagPrefixVismodeHeat)) {
                        //    Mod.Log.Debug($"Actor:{actorLabel} has HEAT VISION component:{kv.Key} with tag:{tag}");
                        //    ParseVismodeHeat(ref state, tag);
                        //}
                    }
                }

                // If the unit has stealth, it disables the ECM system.
                if (state.stealthMod != 0 && state.ecmMod != 0) {
                    Mod.Log.Debug($"Actor:{actorLabel} has both STEALTH and JAMMER - disabling ECM bubble.");
                    state.ecmMod = 0;
                }

                // TODO: Should stealth move/range mods depend on an active stealth system?

                // Determine the bonus from the pilots tactics
                if (actor.GetPilot() != null) {
                    state.tacticsBonus = SkillUtils.GetTacticsModifier(actor.GetPilot());
                } else {
                    Mod.Log.Info($"Actor:{CombatantUtils.Label(actor)} HAS NO PILOT!");
                }

            }

            private static void ParseProbe(ref EWState state, string tag) {
                string[] split = tag.Split('_');
                if (split.Length == 2) {
                    int modifier = Int32.Parse(split[1].Substring(1));
                    if (modifier >= state.probeMod) {
                        state.probeMod = modifier;
                    }
                } else {
                    Mod.Log.Info($"MALFORMED TAG - ({tag})");
                }
            }

            private static void ParseStealth(ref EWState state, string tag) {
                string[] split = tag.Split('_');
                if (split.Length == 2) {
                    int modifier = Int32.Parse(split[1].Substring(1));
                    if (modifier >= state.stealthMod) {
                        state.stealthMod = modifier;
                    }
                } else {
                    Mod.Log.Info($"MALFORMED TAG - ({tag})");
                }
            }

            private static void ParseStealthMoveMod(ref EWState state, string tag) {
                string[] split = tag.Split('_');
                if (split.Length == 3) {
                    int modifier = Int32.Parse(split[1].Substring(1));
                    int moveStep = Int32.Parse(split[2].Substring(1));
                    if (state.stealthMoveMod == null || modifier > state.stealthMoveMod[0]) {
                        state.stealthMoveMod = new int[] { modifier, moveStep };
                    }
                } else {
                    Mod.Log.Info($"MALFORMED TAG - ({tag})");
                }
            }

            private static void ParseVismodeZoom(ref EWState state, string tag) {
                string[] split = tag.Split('_');
                if (split.Length == 4) {
                    int modifier = int.Parse(split[1].Substring(1));
                    int cap = int.Parse(split[2].Substring(1));
                    int step = int.Parse(split[3].Substring(1));
                    state.vismodeZoomMod = modifier;
                    state.vismodeZoomCap = cap;
                    state.vismodeZoomStep = step;
                } else {
                    Mod.Log.Info($"MALFORMED TAG - ({tag})");
                }
            }

            private static void ParseVismodeHeat(ref EWState state, string tag) {
                string[] split = tag.Split('_');
                if (split.Length == 3) {
                    int modifier = int.Parse(split[1].Substring(1));
                    int divisor = int.Parse(split[2].Substring(1));
                    if (modifier >= state.vismodeHeatMod) {
                        state.vismodeHeatMod = modifier;
                        state.vismodeHeatDivisor = divisor;
                    }
                } else {
                    Mod.Log.Info($"MALFORMED TAG -:{tag}");
                }
            }
        }

    };

    public class VisionModeModifer {
        public int modifier;
        public string label;

        public override String ToString() { return $"label:{label} - modifier:{modifier} "; }
    }
  
}
