using BattleTech;
using HBS.Collections;
using System;
using System.Collections.Generic;
using System.Linq;
using static LowVisibility.Helper.ActorHelper;

namespace LowVisibility.Helper {
    public class ActorEWConfig {

        public const string TagPrefixJammer = "lv-jammer_t";
        public const string TagPrefixProbe = "lv-probe_t";
        public const string TagSharesSensors = "lv-shares-sensors";
        public const string TagPrefixStealth = "lv-stealth_t";
        public const string TagPrefixStealthRangeMod = "lv-stealth-range-mod_s";
        public const string TagPrefixStealthMoveMod = "lv-stealth-move-mod_m";

        // ECM Equipment = ecm_t0, Guardian ECM = ecm_t1, Angel ECM = ecm_t2, CEWS = ecm_t3. -1 means none.
        public int ecmTier = -1;
        public float ecmRange = 0;
        public int ecmModifier = 0; // Any additional modifier to opposed ECM modifier for the sensor check

        // Pirate = activeprobe_t0, Beagle = activeprobe_t1, Bloodhound = activeprobe_t2, CEWS = activeprobe_t3. -1 means none.
        public int probeTier = -1;
        public float probeRange = 0;
        public int probeModifier = 0; // The sensor check modifier used in opposed cases (see MaxTech 55)

        // Stealth armor
        public int stealthTier = -1;
        // Modifier for stealth range modification - min-short, short-medium, medium-long, long-max
        public int[] stealthRangeMod = new int[] { 0, 0, 0, 0 };

        // Modifier for stealth movement - base modifier (0), reduced by -1 for each (1) hexes moved
        public int[] stealthMoveMod = new int[] { 0, 0 };

        // The amount of tactics bonus to the sensor check
        public int tacticsBonus = 0;

        // Whether this actor will share sensor data with others
        public bool sharesSensors = false;

        public ActorEWConfig(AbstractActor actor) {
            // Check tags for any ecm/sensors
            // TODO: Check for stealth
            int actorEcmTier = -1;
            float actorEcmRange = 0;
            int actorEcmModifier = 0;

            int actorProbeTier = -1;
            float actorProbeRange = 0;
            int actorProbeModifier = 0;

            // TODO: Add pilot skill check / tag check for same effect
            bool actorSharesSensors = false;

            int actorStealthTier = -1;
            int[] actorStealthRangeMod = null;
            int[] actorStealthMoveMod = null;

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
                        LowVisibility.Logger.LogIfDebug($"Actor:{ActorLabel(actor)} has ECM component:{kv.Key} with tag:{tag}");
                        string[] split = tag.Split('_');
                        if (split.Length == 4) {
                            int tier = Int32.Parse(split[1].Substring(1));
                            int range = Int32.Parse(split[2].Substring(1));
                            int modifier = Int32.Parse(split[3].Substring(1));
                            if (tier >= actorEcmTier) {
                                actorEcmTier = tier;
                                actorEcmRange = range * 30.0f;
                                actorEcmModifier = modifier;
                            } else {
                                LowVisibility.Logger.LogIfDebug($"Actor:{ActorLabel(actor)} - MALFORMED TAG -:{tag}");
                            }
                        }
                    } else if (tagLower.StartsWith(TagPrefixProbe)) {
                        LowVisibility.Logger.LogIfDebug($"Actor:{ActorLabel(actor)} has Probe component:{kv.Key} with tag:{tag}");
                        string[] split = tag.Split('_');
                        if (split.Length == 4) {
                            int tier = Int32.Parse(split[1].Substring(1));
                            int range = Int32.Parse(split[2].Substring(1));
                            int modifier = Int32.Parse(split[3].Substring(1));
                            if (tier >= actorProbeTier) {
                                actorProbeTier = tier;
                                actorProbeRange = range * 30.0f;
                                actorProbeModifier = modifier;
                            }
                        } else {
                            LowVisibility.Logger.LogIfDebug($"Actor:{ActorLabel(actor)} - MALFORMED TAG -:{tag}");
                        }
                    } else if (tagLower.Equals(TagSharesSensors)) {
                        LowVisibility.Logger.LogIfDebug($"Actor:{ActorLabel(actor)} shares sensors due to component:{kv.Key} with tag:{tag}");
                        actorSharesSensors = true;
                    } else if (tagLower.StartsWith(TagPrefixStealth)) {
                        LowVisibility.Logger.LogIfDebug($"Actor:{ActorLabel(actor)} has Stealth component:{kv.Key} with tag:{tag}");
                        string[] split = tag.Split('_');
                        if (split.Length == 2) {
                            int tier = Int32.Parse(split[1].Substring(1));
                            if (tier >= actorStealthTier) {
                                actorStealthTier = tier;
                            }
                        } else {
                            LowVisibility.Logger.LogIfDebug($"Actor:{ActorLabel(actor)} - MALFORMED TAG -:{tag}");
                        }
                    } else if (tagLower.StartsWith(TagPrefixStealthRangeMod)) {
                        LowVisibility.Logger.LogIfDebug($"Actor:{ActorLabel(actor)} has StealthRangeMod component:{kv.Key} with tag:{tag}");
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
                            LowVisibility.Logger.LogIfDebug($"Actor:{ActorLabel(actor)} - MALFORMED TAG -:{tag}");
                        }
                    } else if (tagLower.StartsWith(TagPrefixStealthMoveMod)) {
                        LowVisibility.Logger.LogIfDebug($"Actor:{ActorLabel(actor)} has StealthMoveMod component:{kv.Key} with tag:{tag}");
                        string[] split = tag.Split('_');
                        if (split.Length == 3) {
                            int modifier = Int32.Parse(split[1].Substring(1));
                            int moveStep = Int32.Parse(split[2].Substring(1));
                            if (actorStealthMoveMod == null || modifier > actorStealthMoveMod[0]) {
                                actorStealthMoveMod = new int[] { modifier, moveStep };
                            }
                        } else {
                            LowVisibility.Logger.LogIfDebug($"Actor:{ActorLabel(actor)} - MALFORMED TAG -:{tag}");
                        }
                    }
                }
            }

            //foreach (MechComponent component in actor?.allComponents) {
            //    MechComponentRef componentRef = component?.mechComponentRef;
            //    MechComponentDef componentDef = componentRef?.Def;
            //    TagSet componentTags = componentDef?.ComponentTags;
                
            //}

            // If the unit has stealth, it disables the ECM system.
            if (actorStealthTier >= 0) {
                LowVisibility.Logger.Log($"Actor:{ActorLabel(actor)} has multiple stealth and ECM - disabling ECM bubble.");
                actorEcmTier = -1;
                actorEcmRange = 0;
                actorEcmModifier = 0;
            }

            // Determine pilot bonus
            int unitTacticsBonus = 0;
            if (actor.GetPilot() != null) {
                int pilotTactics = actor.GetPilot().Tactics;                
                int normedTactics = NormalizeSkill(pilotTactics);
                unitTacticsBonus = ModifierBySkill[normedTactics];
            } else {
                LowVisibility.Logger.Log($"Actor:{ActorLabel(actor)} HAS NO PILOT!");
            }
            
            this.ecmTier = actorEcmTier;
            this.ecmRange = actorEcmRange;
            this.ecmModifier = actorEcmModifier;
            this.probeTier = actorProbeTier;
            this.probeRange = actorProbeRange;
            this.probeModifier = actorProbeModifier;
            this.tacticsBonus = unitTacticsBonus;
            this.sharesSensors = actorSharesSensors;
            this.stealthTier = actorStealthTier;
            this.stealthRangeMod = actorStealthRangeMod ?? (new int[] { 0, 0, 0, 0 });
            this.stealthMoveMod = actorStealthMoveMod ?? (new int[] { 0, 0 });            
        }

        public override string ToString() {
            return $"tacticsBonus:+{tacticsBonus} ecmTier:{ecmTier} ecmRange:{ecmRange} " +
                $"probeTier:{probeTier} probeRange:{probeRange} sharesSensors:{sharesSensors} " +
                $"stealthTier:{stealthTier} stealthRangeMod:{stealthRangeMod[0]}/{stealthRangeMod[1]}/{stealthRangeMod[2]}/{stealthRangeMod[3]} " +
                $"stealthMoveMod:{stealthMoveMod[0]}/{stealthMoveMod[1]}";
        }

        public bool HasStealthRangeMod() {
            bool hasMod = stealthRangeMod != null && (stealthRangeMod[0] != 0 || stealthRangeMod[1] != 0 || stealthRangeMod[2] != 0 || stealthRangeMod[3] != 0);
            return hasMod;
        }

        public bool HasStealthMoveMod() {
            bool hasMod = stealthMoveMod != null && stealthMoveMod[0] != 0;
            return hasMod;
        }

        public int StealthRangeModAtDistance(Weapon weapon, float distance) {
            int rangeMod = 0;
            if (this.stealthRangeMod[0] != 0 && distance < weapon.ShortRange) {
                rangeMod = this.stealthRangeMod[0];
                LowVisibility.Logger.LogIfDebug($"  StealthRangeMod - modifier {this.stealthRangeMod[0]} due to short range shot.");
            } else if (this.stealthRangeMod[1] != 0 && distance < weapon.MediumRange && distance >= weapon.ShortRange) {
                rangeMod = this.stealthRangeMod[1];
                LowVisibility.Logger.LogIfDebug($"  StealthRangeMod - modifier {this.stealthRangeMod[1]} due to medium range shot.");
            } else if (this.stealthRangeMod[2] != 0 && distance < weapon.LongRange && distance >= weapon.MediumRange) {
                rangeMod = this.stealthRangeMod[2];
                LowVisibility.Logger.LogIfDebug($"  StealthRangeMod - modifier  {this.stealthRangeMod[2]} due to long range shot.");
            } else if (this.stealthRangeMod[3] != 0 && distance < weapon.MaxRange && distance >= weapon.LongRange) {
                rangeMod = this.stealthRangeMod[3];
                LowVisibility.Logger.LogIfDebug($"  StealthRangeMod - modifier  {this.stealthRangeMod[3]} due to max range shot.");
            }
            return rangeMod;
        }

        public int StealthMoveModForActor(AbstractActor owner) {
            int moveMod = 0;
            if (owner != null && this.stealthMoveMod[0] != 0) {
                int hexesMoved = (int)Math.Floor(owner.DistMovedThisRound / 30.0);
                LowVisibility.Logger.LogIfDebug($"  StealthMoveMod - actor:{ActorLabel(owner)} " +
                    $"hasMovedThisRound:{owner.HasMovedThisRound} distMovedThisRound:{owner.DistMovedThisRound} which is hexesMoved:{hexesMoved}");
                moveMod = this.stealthMoveMod[0];
                while (hexesMoved > 0) {
                    moveMod--;
                    hexesMoved -= this.stealthMoveMod[1];
                }
                LowVisibility.Logger.LogIfDebug($"  StealthMoveMod - actor:{ActorLabel(owner)} has moveMod:{moveMod}");
            }

            return moveMod;
        }


        // A mapping of skill level to modifier
        private static readonly Dictionary<int, int> ModifierBySkill = new Dictionary<int, int> {
            { 1, 0 },
            { 2, 1 },
            { 3, 1 },
            { 4, 2 },
            { 5, 2 },
            { 6, 3 },
            { 7, 3 },
            { 8, 4 },
            { 9, 4 },
            { 10, 5 },
            { 11, 6 },
            { 12, 7 },
            { 13, 8 }
        };

        private static int NormalizeSkill(int rawValue) {
            int normalizedVal = rawValue;
            if (rawValue >= 11 && rawValue <= 14) {
                // 11, 12, 13, 14 normalizes to 11
                normalizedVal = 11;
            } else if (rawValue >= 15 && rawValue <= 18) {
                // 15, 16, 17, 18 normalizes to 14
                normalizedVal = 12;
            } else if (rawValue == 19 || rawValue == 20) {
                // 19, 20 normalizes to 13
                normalizedVal = 13;
            } else if (rawValue <= 0) {
                normalizedVal = 1;
            } else if (rawValue > 20) {
                normalizedVal = 13;
            }
            return normalizedVal;
        }
    };
}
