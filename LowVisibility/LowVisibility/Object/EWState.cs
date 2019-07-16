using BattleTech;
using HBS.Collections;
using LowVisibility.Helper;
using System;
using System.Collections.Generic;
using System.Globalization;
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

        private int CurrentRoundEWCheck = 0; // Raw value before any manipulation

        private string parentGUID = null;

        private float sensorsBaseRange = 0.0f;

        private int ECMShield = 0;
        private int ECMCarrier = 0;
        private int ECMJammed = 0;

        private int AdvancedSensors = 0;

        private int ProbeCarrier = 0;
        private int ProbeSweepTarget = 0;

        private int StaticSensorStealth = 0;
        private DecayingStealth DecayingSensorStealth = null;
        private float[] SensorStealthAttackMulti = new float[] { 1f, 1f, 1f, 1f, 1f };

        private int StaticVisionStealth = 0;
        private DecayingStealth DecayingVisionStealth = null;
        private float[] VisionStealthAttackMulti = new float[] { 1f, 1f, 1f, 1f, 1f };

        private int vismodeZoomMod = 0;
        private int vismodeZoomCap = 0;
        private int vismodeZoomStep = 0;

        private int vismodeHeatMod = 0;
        private int vismodeHeatDivisor = 0;

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
            if (actor.StatCollection.ContainsStatistic(ModStats.SensorStealthAttackMulti) &&
                actor.StatCollection.GetStatistic(ModStats.SensorStealthAttackMulti).Value<string>() != "") {
                string rawValue = actor.StatCollection.GetStatistic(ModStats.SensorStealthAttackMulti).Value<string>();
                string[] tokens = rawValue.Split('_');
                if (tokens.Length == 5) {
                    try {
                        SensorStealthAttackMulti = new float[] {
                            float.Parse(tokens[0], CultureInfo.InvariantCulture),
                            float.Parse(tokens[1], CultureInfo.InvariantCulture),
                            float.Parse(tokens[2], CultureInfo.InvariantCulture),
                            float.Parse(tokens[3], CultureInfo.InvariantCulture)
                        };
                    } catch (Exception) {
                        Mod.Log.Info($"Failed to tokenize SensorStealthAttackMulti value: ({rawValue}). Discarding!");
                        SensorStealthAttackMulti = new float[] { 1f, 1f, 1f, 1f, 1f };
                    }
                } else {
                    Mod.Log.Info($"WARNING: Invalid SensorStealthAttackMulti value: ({rawValue}) found. Discarding!");
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
            if (actor.StatCollection.ContainsStatistic(ModStats.VisionStealthAttackMulti) &&
                actor.StatCollection.GetStatistic(ModStats.VisionStealthAttackMulti).Value<string>() != "") {
                string rawValue = actor.StatCollection.GetStatistic(ModStats.VisionStealthAttackMulti).Value<string>();
                string[] tokens = rawValue.Split('_');
                if (tokens.Length == 5) {
                    try {
                        VisionStealthAttackMulti = new float[] {
                            float.Parse(tokens[0], CultureInfo.InvariantCulture),
                            float.Parse(tokens[1], CultureInfo.InvariantCulture),
                            float.Parse(tokens[2], CultureInfo.InvariantCulture),
                            float.Parse(tokens[3], CultureInfo.InvariantCulture)
                        };
                    } catch (Exception) {
                        Mod.Log.Info($"Failed to tokenize VisionStealthAttackMulti value: ({rawValue}). Discarding!");
                        VisionStealthAttackMulti = new float[] { 1f, 1f, 1f, 1f, 1f };
                    }
                } else {
                    Mod.Log.Info($"WARNING: Invalid VisionStealthAttackMulti value: ({rawValue}) found. Discarding!");
                }
            }

            SetSensorBaseRange(actor);
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
        public float GetSensorsBaseRange() { return sensorsBaseRange; }
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

        // Probes
        public int GetProbeSelfModifier() { return ProbeCarrier; }
        public int GetTargetOfProbeModifier() { return ProbeSweepTarget; }

        // Sensor Stealth
        public float GetSensorStealthSignatureModifier() { return CurrentSensorStealthPips() * 0.05f; }
        public int GetSensorStealthDetailsModifier() { return CurrentSensorStealthPips(); }
        public int GetSensorStealthAttackModifier(Weapon weapon, float distance, EWState attackerState) {
            int rangeIdx = WeaponHelper.GetRangeIndex(weapon, distance);
            float multi = SensorStealthAttackMulti[rangeIdx];
            int modifier = (int) Math.Ceiling(multi * CurrentSensorStealthPips());
            // TODO: CAP MODIFIER AT NO-SENSOR-LOCK-CAP

            Mod.Log.Debug($"Target:({CombatantUtils.Label(actor)}) has Sensor Stealth Mod:{modifier} - ProbeMod:{attackerState.GetProbeSelfModifier()} " +
                $"from source:{CombatantUtils.Label(attackerState.actor)}");
            return Math.Max(0, modifier - attackerState.GetProbeSelfModifier());
        }
        public int CurrentSensorStealthPips(float distance) { return CurrentStealthPips(StaticSensorStealth, DecayingSensorStealth, distance); }
        public int CurrentSensorStealthPips() {
            float distance = Vector3.Distance(actor.PreviousPosition, actor.CurrentPosition);
            return CurrentStealthPips(StaticSensorStealth, DecayingSensorStealth, distance);
        }
        public int MaxSensorStealthPips() { return StaticSensorStealth + (DecayingSensorStealth != null ? DecayingSensorStealth.InitialMod : 0); }
        public bool HasSensorStealth() { return StaticSensorStealth != 0 || DecayingSensorStealth != null; }

        // Vision Stealth
        public float GetVisionStealthVisibilityModifier() { return CurrentVisionStealthPips() * 0.05f; }
        public int GetVisionStealthDetailsModifier() { return CurrentVisionStealthPips(); }
        public int GetVisionStealthAttackModifier(Weapon weapon, float distance, EWState attackerState) {
            int rangeIdx = WeaponHelper.GetRangeIndex(weapon, distance);
            float multi = SensorStealthAttackMulti[rangeIdx];
            int modifier = (int)Math.Ceiling(multi * CurrentVisionStealthPips());
            // TODO: CAP MODIFIER AT NO-SENSOR-LOCK-CAP

            Mod.Log.Debug($"Target:({CombatantUtils.Label(actor)}) has Sensor Stealth Mod:{modifier} - ProbeMod:{attackerState.GetProbeSelfModifier()} " +
                $"from source:{CombatantUtils.Label(attackerState.actor)}");
            return Math.Max(0, modifier - attackerState.GetProbeSelfModifier());
        }

        public int CurrentVisionStealthPips(float distance) { return CurrentStealthPips(StaticVisionStealth, DecayingVisionStealth, distance); }
        public int CurrentVisionStealthPips() {
            float distance = Vector3.Distance(actor.PreviousPosition, actor.CurrentPosition);
            return CurrentStealthPips(StaticVisionStealth, DecayingVisionStealth, distance);
        }
        public int MaxVisionStealthPips() { return StaticVisionStealth + (DecayingVisionStealth != null ? DecayingVisionStealth.InitialMod : 0); }
        public bool HasVisionStealth() { return StaticVisionStealth != 0 || DecayingVisionStealth != null; }

        public bool HasStealth() {
            return StaticSensorStealth != 0 || DecayingSensorStealth != null || StaticVisionStealth != 0 || DecayingVisionStealth != null;
        }

        // Vision
        public float GetTacticsVisionBoost() { return tacticsModifier * actor.Combat.Constants.Visibility.SpotterTacticsMultiplier; }

        // Helper method
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

        public override string ToString() { return $"sensorsCheck:{CurrentRoundEWCheck}"; }

        public string Details() {
            return $"tacticsModifier:{tacticsModifier}" +
                $"ecmShieldSigMod:{GetECMShieldSignatureModifier()} ecmShieldDetailsMod:{GetECMShieldDetailsModifier()} " +
                $"ecmJammedDetailsMod:{GetECMJammedDetailsModifier()} " +
                $"probeCarrier:{GetProbeSelfModifier()} probeSweepTarget:{GetTargetOfProbeModifier()}" +
                $"staticSensorStealth:{StaticSensorStealth} decayingSensorStealth - {(DecayingSensorStealth != null ? DecayingSensorStealth.ToString() : "")} " +
                $"sensorStealthAttackMulti:{(SensorStealthAttackMulti != null ? SensorStealthAttackMulti.ToString() : "1")} " +
                $"staticVisionStealth:{StaticVisionStealth} decayingVisionStealth- {(DecayingVisionStealth != null ? DecayingVisionStealth.ToString() : "")} " +
                $"visionStealthAttackMulti:{(VisionStealthAttackMulti != null ? VisionStealthAttackMulti.ToString() : "1")} " +
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
                if (state.HasStealth() && state.ECMCarrier != 0) {
                    Mod.Log.Debug($"Actor:{actorLabel} has both STEALTH and JAMMER - disabling ECM bubble.");
                    state.ECMCarrier = 0;
                    state.ECMShield = 0;
                }

                // Determine the bonus from the pilots tactics
                if (actor.GetPilot() != null) {
                    state.tacticsModifier = 1.0f + (SkillUtils.GetTacticsModifier(actor.GetPilot()) / 10.0f);
                } else {
                    Mod.Log.Info($"Actor:{CombatantUtils.Label(actor)} HAS NO PILOT!");
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
