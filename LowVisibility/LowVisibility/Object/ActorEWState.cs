using BattleTech;
using HBS.Collections;
using LowVisibility.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using static LowVisibility.Helper.ActorHelper;

namespace LowVisibility.Object {

    //[JsonConverter(typeof(DynamicEWStateConverter))]
    class DynamicEWState {

        public int rangeCheck;
        public int detailCheck;

        public DynamicEWState() {
            rangeCheck = 0;
            detailCheck = 0;
        }

        public DynamicEWState(int rangeCheck, int detailCheck) {
            this.rangeCheck = rangeCheck;
            this.detailCheck = detailCheck;
        }

        public override string ToString() {
            return $"rangeCheck:{rangeCheck} detailCheck:{detailCheck}";
        }
    }

    public class StaticEWState {

        public const string TagPrefixJammer = "lv-jammer_m";

        public const string TagPrefixProbe = "lv-probe_m";
        public const string TagPrefixProbeBoost = "lv-probe-boost_m";

        public const string TagSharesSensors = "lv-shares-sensors";

        public const string TagPrefixStealth = "lv-stealth_m";
        public const string TagPrefixScrambler = "lv-scrambler_m";

        public const string TagPrefixStealthRangeMod = "lv-stealth-range-mod_s";
        public const string TagPrefixStealthMoveMod = "lv-stealth-move-mod_m";

        public const string TagPrefixNarcEffect = "lv-narc-effect_m";
        public const string TagPrefixTagEffect = "lv-tag-effect";

        public const string TagPrefixVismodeZoom = "lv-vismode-zoom_m";
        public const string TagPrefixVismodeHeat = "lv-vismode-heat_m";

        public int ecmMod = 0;
        public float ecmRange = 0;

        public int probeMod = 0;
        public int probeBoostMod = 0;

        public int stealthMod = 0;
        public int scramblerMod = 0;

        public int vismodeZoomMod = 0;
        public int vismodeZoomStep = 0;

        public int vismodeHeatMod = 0;
        public int vismodeHeatDivisor = 0;

        // Modifier for stealth range modification - min-short, short-medium, medium-long, long-max
        public int[] stealthRangeMod = new int[] { 0, 0, 0, 0 };

        // Modifier for stealth movement - base modifier (0), reduced by -1 for each (1) hexes moved
        public int[] stealthMoveMod = new int[] { 0, 0 };

        // The amount of tactics bonus to the sensor check
        public int tacticsBonus = 0;

        // Whether this actor will share sensor data with others
        public bool sharesSensors = false;

        public StaticEWState() {

        }

        public StaticEWState(AbstractActor actor) {
            // Check tags for any ecm/sensors
            int actorEcmMod = 0;
            float actorEcmRange = 0;

            int actorProbeMod = 0;

            int actorStealthMod = 0;
            int actorScramblerMod = 0;

            int actorVismodeZoomMod = 0;
            int actorVismodeZoomStep = 0;

            int actorVismodeHeatMod = 0;
            int actorVismodeHeatDivisor = 0;

            int actorProbeBoostMod = 0;

            int[] actorStealthRangeMod = null;
            int[] actorStealthMoveMod = null;

            bool actorSharesSensors = false;

            Dictionary<string, TagSet> cTags = actor?.allComponents?
                .Where(c => c?.componentDef?.ComponentTags != null)                
                .GroupBy(c => c.componentDef)
                .Select(c => c.First())
                .ToDictionary(c => c.componentDef.Description.Id, v => v?.componentDef?.ComponentTags);
            foreach (KeyValuePair<string, TagSet> kv in cTags) {
                foreach (string tag in kv.Value) {
                    if (tag == null || tag.Equals("") || tag.ToLower() == null) { continue; }
                    string tagLower = tag.ToLower();

                    if (tagLower.StartsWith(TagPrefixJammer)) {
                        LowVisibility.Logger.LogIfDebug($"Actor:{CombatantHelper.Label(actor)} has JAMMER component:{kv.Key} with tag:{tag}");
                        string[] split = tag.Split('_');
                        if (split.Length == 3) {
                            int modifier = Int32.Parse(split[1].Substring(1));
                            int range = Int32.Parse(split[2].Substring(1));
                            if (modifier >= actorEcmMod) {
                                actorEcmMod = modifier;
                                actorEcmRange = range * 30.0f;
                            } else {
                                LowVisibility.Logger.Log($"Actor:{CombatantHelper.Label(actor)} - MALFORMED TAG -:{tag}");
                            }
                        }
                    } else if (tagLower.StartsWith(TagPrefixProbe)) {
                        LowVisibility.Logger.LogIfDebug($"Actor:{CombatantHelper.Label(actor)} has PROBE component:{kv.Key} with tag:{tag}");
                        string[] split = tag.Split('_');
                        if (split.Length == 2) {
                            int modifier = Int32.Parse(split[1].Substring(1));
                            if (modifier >= actorProbeMod) {
                                actorProbeMod = modifier;
                            }
                        } else {
                            LowVisibility.Logger.Log($"Actor:{CombatantHelper.Label(actor)} - MALFORMED TAG -:{tag}");
                        }
                    } else if (tagLower.StartsWith(TagPrefixStealth)) {
                        LowVisibility.Logger.LogIfDebug($"Actor:{CombatantHelper.Label(actor)} has STEALTH component:{kv.Key} with tag:{tag}");
                        string[] split = tag.Split('_');
                        if (split.Length == 2) {
                            int modifier = Int32.Parse(split[1].Substring(1));
                            if (modifier >= actorStealthMod) {
                                actorStealthMod = modifier;
                            }
                        } else {
                            LowVisibility.Logger.Log($"Actor:{CombatantHelper.Label(actor)} - MALFORMED TAG -:{tag}");
                        }
                    } else if (tagLower.StartsWith(TagPrefixScrambler)) {
                        LowVisibility.Logger.LogIfDebug($"Actor:{CombatantHelper.Label(actor)} has SCRAMBLER component:{kv.Key} with tag:{tag}");
                        string[] split = tag.Split('_');
                        if (split.Length == 2) {
                            int modifier = Int32.Parse(split[1].Substring(1));
                            actorScramblerMod += modifier;
                        } else {
                            LowVisibility.Logger.Log($"Actor:{CombatantHelper.Label(actor)} - MALFORMED TAG -:{tag}");
                        }
                    } else if (tagLower.StartsWith(TagPrefixStealthRangeMod)) {
                        LowVisibility.Logger.LogIfDebug($"Actor:{CombatantHelper.Label(actor)} has STEALTH_RANGE_MOD component:{kv.Key} with tag:{tag}");
                        string[] split = tag.Split('_');
                        if (split.Length == 5) {
                            int shortRange = Int32.Parse(split[1].Substring(1));
                            int mediumRange = Int32.Parse(split[2].Substring(1));
                            int longRange = Int32.Parse(split[3].Substring(1));
                            int extremeRange = Int32.Parse(split[4].Substring(1));
                            if (actorStealthRangeMod == null || shortRange > actorStealthRangeMod[0]) {
                                actorStealthRangeMod = new int[] { shortRange, mediumRange, longRange, extremeRange };
                            }
                        } else {
                            LowVisibility.Logger.Log($"Actor:{CombatantHelper.Label(actor)} - MALFORMED TAG -:{tag}");
                        }
                    } else if (tagLower.StartsWith(TagPrefixStealthMoveMod)) {
                        LowVisibility.Logger.LogIfDebug($"Actor:{CombatantHelper.Label(actor)} has STEALTH_MOVE_MOD component:{kv.Key} with tag:{tag}");
                        string[] split = tag.Split('_');
                        if (split.Length == 3) {
                            int modifier = Int32.Parse(split[1].Substring(1));
                            int moveStep = Int32.Parse(split[2].Substring(1));
                            if (actorStealthMoveMod == null || modifier > actorStealthMoveMod[0]) {
                                actorStealthMoveMod = new int[] { modifier, moveStep };
                            }
                        } else {
                            LowVisibility.Logger.Log($"Actor:{CombatantHelper.Label(actor)} - MALFORMED TAG -:{tag}");
                        }
                    } else if (tagLower.Equals(TagSharesSensors)) {
                        LowVisibility.Logger.LogIfDebug($"Actor:{CombatantHelper.Label(actor)} shares sensors due to component:{kv.Key} with tag:{tag}");
                        actorSharesSensors = true;
                    } else if (tagLower.StartsWith(TagPrefixVismodeZoom)) {
                        LowVisibility.Logger.LogIfDebug($"Actor:{CombatantHelper.Label(actor)} has ZOOM VISION component:{kv.Key} with tag:{tag}");
                        string[] split = tag.Split('_');
                        if (split.Length == 3) {
                            int modifier = int.Parse(split[1].Substring(1));
                            int step = int.Parse(split[2].Substring(1));
                            if (modifier >= actorVismodeZoomMod) {
                                actorVismodeZoomMod = modifier;
                                actorVismodeZoomStep = step;
                            }
                        } else {
                            LowVisibility.Logger.Log($"Actor:{CombatantHelper.Label(actor)} - MALFORMED TAG -:{tag}");
                        }
                    } else if (tagLower.StartsWith(TagPrefixVismodeHeat)) {
                        LowVisibility.Logger.LogIfDebug($"Actor:{CombatantHelper.Label(actor)} has HEAT VISION component:{kv.Key} with tag:{tag}");
                        string[] split = tag.Split('_');
                        if (split.Length == 3) {
                            int modifier = int.Parse(split[1].Substring(1));
                            int divisor = int.Parse(split[2].Substring(1));
                            if (modifier >= actorVismodeHeatMod) {
                                actorVismodeHeatMod = modifier;
                                actorVismodeHeatDivisor = divisor;
                            }
                        } else {
                            LowVisibility.Logger.Log($"Actor:{CombatantHelper.Label(actor)} - MALFORMED TAG -:{tag}");
                        }
                    } else if (tagLower.StartsWith(TagPrefixProbeBoost)) {
                        LowVisibility.Logger.LogIfDebug($"Actor:{CombatantHelper.Label(actor)} has PROBE BOOST component:{kv.Key} with tag:{tag}");
                        string[] split = tag.Split('_');
                        if (split.Length == 2) {
                            int modifier = Int32.Parse(split[1].Substring(1));
                            actorProbeBoostMod += modifier;
                        } else {
                            LowVisibility.Logger.Log($"Actor:{CombatantHelper.Label(actor)} - MALFORMED TAG -:{tag}");
                        }
                    }
                }
            }

            // If the unit has stealth, it disables the ECM system.
            if (actorStealthMod != 0 && actorEcmMod != 0) {
                LowVisibility.Logger.Log($"Actor:{CombatantHelper.Label(actor)} has both STEALTH and JAMMER - disabling ECM bubble.");
                actorEcmRange = 0;
                actorEcmMod = 0;
            }

            // Determine pilot bonus
            int unitTacticsBonus = 0;
            if (actor.GetPilot() != null) {
                unitTacticsBonus = SkillHelper.GetTacticsModifier(actor.GetPilot());
            } else {
                LowVisibility.Logger.Log($"Actor:{CombatantHelper.Label(actor)} HAS NO PILOT!");
            }

            this.tacticsBonus = unitTacticsBonus;
            
            this.ecmMod = actorEcmMod;
            this.ecmRange = actorEcmRange;

            this.probeMod = actorProbeMod;
            this.probeBoostMod = actorProbeBoostMod;

            this.stealthMod = actorStealthMod;
            this.scramblerMod = actorScramblerMod;

            this.stealthRangeMod = actorStealthRangeMod ?? (new int[] { 0, 0, 0, 0 });
            this.stealthMoveMod = actorStealthMoveMod ?? (new int[] { 0, 0 });
            
            this.vismodeZoomMod = actorVismodeZoomMod;
            this.vismodeZoomStep = actorVismodeZoomStep;

            this.vismodeHeatMod = actorVismodeHeatMod;
            this.vismodeHeatDivisor = actorVismodeHeatDivisor;


            this.sharesSensors = actorSharesSensors;
        }

        public override string ToString() {
            return $"tacticsBonus:{tacticsBonus}ecmMod:{ecmMod} ecmRange:{ecmRange} " +
                $"probeMod:{probeMod} probeBoostMod:{probeBoostMod} stealthMod:{stealthMod} scramblerMod:{scramblerMod} " +
                $"stealthRangeMod:{stealthRangeMod[0]}/{stealthRangeMod[1]}/{stealthRangeMod[2]}/{stealthRangeMod[3]} " +
                $"stealthMoveMod:{stealthMoveMod[0]}/{stealthMoveMod[1]} " +
                $"viszmoeZoomMod:{vismodeZoomMod} vismodeZoomStep:{vismodeZoomStep} " +
                $"vismodeHeatMod:{vismodeHeatMod} vismodeHeatDiv:{vismodeHeatDivisor}" +
                $"sharesSensors:{sharesSensors}";
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
                //LowVisibility.Logger.LogIfDebug($"  StealthRangeMod - modifier {this.stealthRangeMod[0]} due to short range shot.");
            } else if (this.stealthRangeMod[1] != 0 && distance < weapon.MediumRange && distance >= weapon.ShortRange) {
                rangeMod = this.stealthRangeMod[1];
                //LowVisibility.Logger.LogIfDebug($"  StealthRangeMod - modifier {this.stealthRangeMod[1]} due to medium range shot.");
            } else if (this.stealthRangeMod[2] != 0 && distance < weapon.LongRange && distance >= weapon.MediumRange) {
                rangeMod = this.stealthRangeMod[2];
                //LowVisibility.Logger.LogIfDebug($"  StealthRangeMod - modifier  {this.stealthRangeMod[2]} due to long range shot.");
            } else if (this.stealthRangeMod[3] != 0 && distance < weapon.MaxRange && distance >= weapon.LongRange) {
                rangeMod = this.stealthRangeMod[3];
                //LowVisibility.Logger.LogIfDebug($"  StealthRangeMod - modifier  {this.stealthRangeMod[3]} due to max range shot.");
            }
            return rangeMod;
        }

        public int CalculateStealthMoveMod(AbstractActor owner) {
            int moveMod = 0;
            if (owner != null && this.stealthMoveMod[0] != 0) {
                moveMod = GetModifierForRangeDecay(owner.DistMovedThisRound, this.stealthMoveMod[0], this.stealthMoveMod[1]);
                //LowVisibility.Logger.LogIfDebug($"  StealthMoveMod - actor:{CombatantHelper.Label(owner)} has moveMod:{moveMod}");
            }
            return moveMod;
        }

        public VisionModeModifer CalculateVisionModeModifier(ICombatant target, float distance) {

            int zoomMod = 0;
            if (vismodeZoomMod != 0) {
                zoomMod = GetModifierForRangeDecay(distance, this.vismodeZoomMod, this.vismodeZoomStep);
            }

            Mech targetMech = target as Mech;
            double targetHeat = targetMech != null ? (double)targetMech.CurrentHeat : 0.0;
            int heatMod = 0;
            if (vismodeHeatMod != 0 && targetHeat > 0) {
                int heatModRaw = (int)Math.Floor(targetHeat / vismodeHeatDivisor);
                heatMod = heatModRaw <= LowVisibility.Config.HeatVisionMaxBonus ? heatModRaw : LowVisibility.Config.HeatVisionMaxBonus;
            }

            // Attack bonuses are negatives, penalties are positives
            VisionModeModifer vmod = new VisionModeModifer {
                modifier = zoomMod > heatMod ? -1 * zoomMod : -1 * heatMod,
                label = zoomMod > heatMod ? "ZOOM VISION" : "HEAT VISION"
            };
            if (zoomMod > 0 && heatMod > 0) { vmod.modifier++; }

            return zoomMod == 0 && heatMod == 0 ? new VisionModeModifer() : vmod; 
        }

        public class VisionModeModifer {
            public int modifier;
            public string label;
        }

        public int CalculateProbeModifier() {
            return tacticsBonus + probeMod + probeBoostMod;
        }

        private int GetModifierForRangeDecay(float distance, int initial, int step) {
            int decayingMod = initial;
            int distInHexes = (int)Math.Floor(distance / 30.0f);            
            while (distInHexes > 0 && decayingMod > 0) {
                decayingMod--;
                distInHexes -= step;
            }
            return decayingMod;
        }

    };
}
