using BattleTech;
using HBS.Collections;
using LowVisibility.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using static LowVisibility.Helper.ActorHelper;

namespace LowVisibility.Object {

    class DynamicEWState {
        public DetectionLevel visualDetectLevel;
        public DetectionLevel sensorDetectLevel;
        public int currentCheck;

        public DynamicEWState() {
            this.visualDetectLevel = DetectionLevel.NoInfo;
            this.sensorDetectLevel = DetectionLevel.NoInfo;
            this.currentCheck = 0;
        }

        public DynamicEWState(int roundCheck, AbstractActor actor) {
            StaticEWState staticState = State.GetStaticState(actor);
            int modifiedCheck = roundCheck + staticState.tacticsBonus;
            LowVisibility.Logger.LogIfTrace($"RoundCheck::Actor{ActorLabel(actor)} has {roundCheck} modified by tactics:{staticState.tacticsBonus} to {modifiedCheck}");
            // TODO: For now, return a single check for both values. In the future, split those
            DetectionLevel checkLevel = VisibilityHelper.DetectionLevelForCheck(modifiedCheck);

            this.visualDetectLevel = checkLevel;
            this.sensorDetectLevel = checkLevel;
            this.currentCheck = modifiedCheck;
        }

        public override string ToString() {
            return $"currentCheck:{currentCheck} / visualDetectLevel:{visualDetectLevel} / sensorDetectlevel:{sensorDetectLevel}";
        }
    }

    public class StaticEWState {

        public const string TagPrefixJammer = "lv-jammer_m";
        public const string TagPrefixProbe = "lv-probe_m";
        public const string TagPrefixSensorBoost = "lv-sensor-boost_m";
        public const string TagSharesSensors = "lv-shares-sensors";
        public const string TagPrefixStealth = "lv-stealth_m";
        public const string TagPrefixStealthRangeMod = "lv-stealth-range-mod_s";
        public const string TagPrefixStealthMoveMod = "lv-stealth-move-mod_m";

        public int ecmMod = 0;
        public float ecmRange = 0;
        public int probeMod = 0;              
        public int stealthMod = 0;
        public int sensorMod = 0;

        // Modifier for stealth range modification - min-short, short-medium, medium-long, long-max
        public int[] stealthRangeMod = new int[] { 0, 0, 0, 0 };

        // Modifier for stealth movement - base modifier (0), reduced by -1 for each (1) hexes moved
        public int[] stealthMoveMod = new int[] { 0, 0 };

        // The amount of tactics bonus to the sensor check
        public int tacticsBonus = 0;

        // Whether this actor will share sensor data with others
        public bool sharesSensors = false;

        public StaticEWState(AbstractActor actor) {
            // Check tags for any ecm/sensors
            int actorEcmMod = 0;
            float actorEcmRange = 0;
            int actorProbeMod = 0;
            int actorStealthMod = 0;
            int actorSensorMod = 0;

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
                        LowVisibility.Logger.LogIfDebug($"Actor:{ActorLabel(actor)} has JAMMER component:{kv.Key} with tag:{tag}");
                        string[] split = tag.Split('_');
                        if (split.Length == 3) {
                            int modifier = Int32.Parse(split[1].Substring(1));
                            int range = Int32.Parse(split[2].Substring(1));
                            if (modifier >= actorEcmMod) {
                                actorEcmMod = modifier;
                                actorEcmRange = range * 30.0f;                                
                            } else {
                                LowVisibility.Logger.LogIfDebug($"Actor:{ActorLabel(actor)} - MALFORMED TAG -:{tag}");
                            }
                        }
                    } else if (tagLower.StartsWith(TagPrefixProbe)) {
                        LowVisibility.Logger.LogIfDebug($"Actor:{ActorLabel(actor)} has PROBE component:{kv.Key} with tag:{tag}");
                        string[] split = tag.Split('_');
                        if (split.Length == 2) {
                            int modifier = Int32.Parse(split[1].Substring(1));
                            if (modifier >= actorProbeMod) {
                                actorProbeMod = modifier;
                            }
                        } else {
                            LowVisibility.Logger.LogIfDebug($"Actor:{ActorLabel(actor)} - MALFORMED TAG -:{tag}");
                        }
                    } else if (tagLower.StartsWith(TagPrefixSensorBoost)) {
                        LowVisibility.Logger.LogIfDebug($"Actor:{ActorLabel(actor)} has SENSOR_BOOST component:{kv.Key} with tag:{tag}");
                        string[] split = tag.Split('_');
                        if (split.Length == 2) {
                            int modifier = Int32.Parse(split[1].Substring(1));
                            if (modifier >= actorSensorMod) {
                                actorSensorMod = modifier;
                            }
                        } else {
                            LowVisibility.Logger.LogIfDebug($"Actor:{ActorLabel(actor)} - MALFORMED TAG -:{tag}");
                        }
                    } else if (tagLower.StartsWith(TagPrefixStealth)) {
                        LowVisibility.Logger.LogIfDebug($"Actor:{ActorLabel(actor)} has STEALTH component:{kv.Key} with tag:{tag}");
                        string[] split = tag.Split('_');
                        if (split.Length == 2) {
                            int modifier = Int32.Parse(split[1].Substring(1));
                            if (modifier >= actorStealthMod) {
                                actorStealthMod = modifier;
                            }
                        } else {
                            LowVisibility.Logger.LogIfDebug($"Actor:{ActorLabel(actor)} - MALFORMED TAG -:{tag}");
                        }
                    } else if (tagLower.StartsWith(TagPrefixStealthRangeMod)) {
                        LowVisibility.Logger.LogIfDebug($"Actor:{ActorLabel(actor)} has STEALTH_RANGE_MOD component:{kv.Key} with tag:{tag}");
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
                        LowVisibility.Logger.LogIfDebug($"Actor:{ActorLabel(actor)} has STEALTH_MOVE_MOD component:{kv.Key} with tag:{tag}");
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
                    } else if (tagLower.Equals(TagSharesSensors)) {
                        LowVisibility.Logger.LogIfDebug($"Actor:{ActorLabel(actor)} shares sensors due to component:{kv.Key} with tag:{tag}");
                        actorSharesSensors = true;
                    }
                }
            }

            // If the unit has stealth, it disables the ECM system.
            if (actorStealthMod != 0 && actorEcmMod != 0) {
                LowVisibility.Logger.Log($"Actor:{ActorLabel(actor)} has both STEALTH and JAMMER - disabling ECM bubble.");
                actorEcmRange = 0;
                actorEcmMod = 0;
            }

            // TODO: Check that stealth-move-mod/stealth-range-mod has an active stealth system

            // Determine pilot bonus
            int unitTacticsBonus = 0;
            if (actor.GetPilot() != null) {
                int pilotTactics = actor.GetPilot().Tactics;                
                int normedTactics = SkillHelper.NormalizeSkill(pilotTactics);
                unitTacticsBonus = SkillHelper.ModifierBySkill[normedTactics];
                // TODO: Add bonus for SensorLock
            } else {
                LowVisibility.Logger.Log($"Actor:{ActorLabel(actor)} HAS NO PILOT!");
            }

            this.ecmMod = actorEcmMod;
            this.ecmRange = actorEcmRange;
            this.probeMod = actorProbeMod;
            this.tacticsBonus = unitTacticsBonus;
            this.sharesSensors = actorSharesSensors;
            this.sensorMod = actorSensorMod;
            this.stealthMod = actorStealthMod;
            this.stealthRangeMod = actorStealthRangeMod ?? (new int[] { 0, 0, 0, 0 });
            this.stealthMoveMod = actorStealthMoveMod ?? (new int[] { 0, 0 });            
        }

        public override string ToString() {
            return $"tacticsBonus:+{tacticsBonus} ecmMod:{ecmMod} ecmRange:{ecmRange} " +
                $"probeMod:{probeMod} stealthMod:{stealthMod} " +
                $"stealthRangeMod:{stealthRangeMod[0]}/{stealthRangeMod[1]}/{stealthRangeMod[2]}/{stealthRangeMod[3]} " +
                $"stealthMoveMod:{stealthMoveMod[0]}/{stealthMoveMod[1]} " +
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

        public int StealthRangeModAtDistance(Weapon weapon, float distance) {
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

        public int StealthMoveModForActor(AbstractActor owner) {
            int moveMod = 0;
            if (owner != null && this.stealthMoveMod[0] != 0) {
                int hexesMoved = (int)Math.Floor(owner.DistMovedThisRound / 30.0);
                //LowVisibility.Logger.LogIfDebug($"  StealthMoveMod - actor:{ActorLabel(owner)} " +
                //    $"hasMovedThisRound:{owner.HasMovedThisRound} distMovedThisRound:{owner.DistMovedThisRound} which is hexesMoved:{hexesMoved}");
                moveMod = this.stealthMoveMod[0];
                while (hexesMoved > 0) {
                    moveMod--;
                    hexesMoved -= this.stealthMoveMod[1];
                }
                //LowVisibility.Logger.LogIfDebug($"  StealthMoveMod - actor:{ActorLabel(owner)} has moveMod:{moveMod}");
            }

            return moveMod;
        }

    };
}
