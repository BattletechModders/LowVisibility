using BattleTech;
using HBS.Collections;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using us.frostraptor.modUtils;
using us.frostraptor.modUtils.math;

namespace LowVisibility.Object {

    public class EWState {

        public int rangeCheck = 0;
        public int detailCheck = 0;
        public string parentGUID = null;

        [JsonIgnore]
        public float sensorsBaseRange = 0.0f;

        [JsonIgnore]
        public int ecmMod = 0;
        [JsonIgnore]
        public float ecmRange = 0;

        [JsonIgnore]
        public int probeMod = 0;
        [JsonIgnore]
        public int probeBoostMod = 0;

        [JsonIgnore]
        public int stealthMod = 0;
        [JsonIgnore]
        public int scramblerMod = 0;

        [JsonIgnore]
        public int vismodeZoomMod = 0;
        [JsonIgnore]
        public int vismodeZoomCap = 0;
        [JsonIgnore]
        public int vismodeZoomStep = 0;

        [JsonIgnore]
        public int vismodeHeatMod = 0;
        [JsonIgnore]
        public int vismodeHeatDivisor = 0;

        // Modifier for stealth range modification - min-short, short-medium, medium-long, long-max
        [JsonIgnore]
        public int[] stealthRangeMod = new int[] { 0, 0, 0, 0 };

        // Modifier for stealth movement - base modifier (0), reduced by -1 for each (1) hexes moved
        [JsonIgnore]
        public int[] stealthMoveMod = new int[] { 0, 0 };

        // The amount of tactics bonus to the sensor check
        [JsonIgnore]
        public int tacticsBonus = 0;

        // Whether this actor will share sensor data with others
        [JsonIgnore]
        public bool sharesSensors = false;

        // Necessary for serialization
        public EWState() {}

        // Normal Constructor
        public EWState(AbstractActor actor) {
            parentGUID = actor.GUID;

            SetSensorBaseRange(actor);
            EWStateHelper.UpdateStaticState(this, actor);
            UpdateChecks();
        }

        public void UpdateChecks() {
            rangeCheck = State.GetCheckResult();
            detailCheck = State.GetCheckResult();
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

        // TODO: Lash this into serialization
        public void RefreshAfterSave(CombatGameState Combat) {
            AbstractActor actor = Combat.FindActorByGUID(parentGUID);
            SetSensorBaseRange(actor);
            EWStateHelper.UpdateStaticState(this, actor);
        }

        public bool HasStealthRangeMod() {
            bool hasMod = stealthRangeMod != null && (stealthRangeMod[0] != 0 || stealthRangeMod[1] != 0 || stealthRangeMod[2] != 0 || stealthRangeMod[3] != 0);
            return hasMod;
        }

        public bool HasStealthMoveMod() {
            bool hasMod = stealthMoveMod != null && stealthMoveMod[0] != 0;
            return hasMod;
        }

        public int CalculateStealthRangeMod(Weapon weapon, float distance) {
            int rangeMod = 0;
            if (this.stealthRangeMod[0] != 0 && distance < weapon.ShortRange) {
                rangeMod = this.stealthRangeMod[0];
                //LowVisibility.Logger.Debug($"  StealthRangeMod - modifier {this.stealthRangeMod[0]} due to short range shot.");
            } else if (this.stealthRangeMod[1] != 0 && distance < weapon.MediumRange && distance >= weapon.ShortRange) {
                rangeMod = this.stealthRangeMod[1];
                //LowVisibility.Logger.Debug($"  StealthRangeMod - modifier {this.stealthRangeMod[1]} due to medium range shot.");
            } else if (this.stealthRangeMod[2] != 0 && distance < weapon.LongRange && distance >= weapon.MediumRange) {
                rangeMod = this.stealthRangeMod[2];
                //LowVisibility.Logger.Debug($"  StealthRangeMod - modifier  {this.stealthRangeMod[2]} due to long range shot.");
            } else if (this.stealthRangeMod[3] != 0 && distance < weapon.MaxRange && distance >= weapon.LongRange) {
                rangeMod = this.stealthRangeMod[3];
                //LowVisibility.Logger.Debug($"  StealthRangeMod - modifier  {this.stealthRangeMod[3]} due to max range shot.");
            }
            return rangeMod;
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

        public int SensorCheckModifier() {
            return tacticsBonus + probeMod + probeBoostMod;
        }

        public float SensorCheckMultiplier() {
            return 1.0f + ((rangeCheck + SensorCheckModifier()) / 20.0f);
        }

        public override string ToString() {
            return $"rangeCheck:{rangeCheck} detailCheck:{detailCheck} ecmMod:{ecmMod} sensorCheckMod:{SensorCheckModifier()}";
        }

        public string Details() {
            return $"tacticsBonus:{tacticsBonus} ecmMod:{ecmMod} ecmRange:{ecmRange} " +
                $"probeMod:{probeMod} probeBoostMod:{probeBoostMod} stealthMod:{stealthMod} scramblerMod:{scramblerMod} " +
                $"stealthRangeMod:{stealthRangeMod[0]}/{stealthRangeMod[1]}/{stealthRangeMod[2]}/{stealthRangeMod[3]} " +
                $"stealthMoveMod:{stealthMoveMod[0]}/{stealthMoveMod[1]} " +
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
                    state.ecmRange = 0;
                }

                // TODO: Should stealth move/range mods depend on an active stealth system?

                // Determine the bonus from the pilots tactics
                if (actor.GetPilot() != null) {
                    state.tacticsBonus = SkillUtils.GetTacticsModifier(actor.GetPilot());
                } else {
                    Mod.Log.Log($"Actor:{CombatantUtils.Label(actor)} HAS NO PILOT!");
                }

            }

            private static void ParseJammer(ref EWState state, string tag) {
                string[] split = tag.Split('_');
                if (split.Length == 3) {
                    int modifier = Int32.Parse(split[1].Substring(1));
                    int range = Int32.Parse(split[2].Substring(1));
                    if (modifier >= state.ecmMod) {
                        state.ecmMod = modifier;
                        state.ecmRange = range * 30.0f;
                    } else {
                        Mod.Log.Log($"MALFORMED TAG - ({tag})");
                    }
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
                    Mod.Log.Log($"MALFORMED TAG - ({tag})");
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
                    Mod.Log.Log($"MALFORMED TAG - ({tag})");
                }
            }

            private static void ParseScrambler(ref EWState state, string tag) {
                string[] split = tag.Split('_');
                if (split.Length == 2) {
                    int modifier = Int32.Parse(split[1].Substring(1));
                    state.scramblerMod += modifier;
                } else {
                    Mod.Log.Log($"MALFORMED TAG - ({tag})");
                }
            }

            private static void ParseStealthRangeMod(ref EWState state, string tag) {
                string[] split = tag.Split('_');
                if (split.Length == 5) {
                    int shortRange = Int32.Parse(split[1].Substring(1));
                    int mediumRange = Int32.Parse(split[2].Substring(1));
                    int longRange = Int32.Parse(split[3].Substring(1));
                    int extremeRange = Int32.Parse(split[4].Substring(1));
                    if (state.stealthRangeMod == null || shortRange > state.stealthRangeMod[0]) {
                        state.stealthRangeMod = new int[] { shortRange, mediumRange, longRange, extremeRange };
                    }
                } else {
                    Mod.Log.Log($"MALFORMED TAG - ({tag})");
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
                    Mod.Log.Log($"MALFORMED TAG - ({tag})");
                }
            }

            private static void ParseProbeBoost(ref EWState state, string tag) {
                string[] split = tag.Split('_');
                if (split.Length == 2) {
                    int modifier = Int32.Parse(split[1].Substring(1));
                    state.probeBoostMod += modifier;
                } else {
                    Mod.Log.Log($"MALFORMED TAG - ({tag})");
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
                    Mod.Log.Log($"MALFORMED TAG - ({tag})");
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
                    Mod.Log.Log($"MALFORMED TAG -:{tag}");
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
