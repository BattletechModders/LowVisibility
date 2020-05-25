
using System.Collections.Generic;

namespace LowVisibility {

    public class ModStats {
        // WARNING: HBS Code upper-cases all stat names; if you try to comparison match in a case-sensitive fashion
        //  it will fail. Better to uppercase everthing.

        public const string TacticsMod = "LV_TACTICS_MOD"; // Int_32

        public const string CurrentRoundEWCheck = "LV_CURRENT_ROUND_EW_CHECK"; // Int_32

        // ECM 
        public const string ECMShield = "LV_ECM_SHIELD"; // Int_32
        public const string ECMJamming = "LV_ECM_JAMMED"; // Int_32

        // Sensors
        public const string AdvancedSensors = "LV_ADVANCED_SENSORS";

        // Probe    
        public const string ProbeCarrier = "LV_PROBE_CARRIER";
        public const string PingedByProbe = "LV_PROBE_PING";

        // Stealth
        public const string StealthEffect = "LV_STEALTH"; // String
        public const string StealthVFXEnabled = "LV_STEALTH_VFX_ENABLED"; // bool

        // Mimetic
        public const string MimeticEffect = "LV_MIMETIC"; // String
        public const string MimeticVFXEnabled = "LV_MIMETIC_VFX_ENABLED"; // bool
        public const string MimeticCurrentSteps = "LV_MIMETIC_CURRENT_STEPS"; // Int_32

        // Vision modes
        public const string HeatVision = "LV_HEAT_VISION"; // String
        public const string ZoomVision = "LV_ZOOM_VISION"; // String

        // Narc
        public const string NarcEffect = "LV_NARC"; // String
        public const string TagEffect = "LV_TAG"; // String

        // Vision Modes - impacts player team only
        public const string NightVision = "LV_NIGHT_VISION"; // bool
        public const string SharesVision = "LV_SHARES_VISION"; // bool

        public const string DisableSensors = "LV_DISABLE_SENSORS"; // bool

        // CAE Stats - write carefully!
        public const string CAESensorsRange = "CAE_SENSORS_RANGE";

        public static bool IsStealthStat(string statName) {
            return statName != null && statName != "" &&
                (statName.Equals(ModStats.StealthEffect) || statName.Equals(ModStats.MimeticEffect));
        }
    }

    public class ModConfig {
        // If true, extra logging will be used
        public bool Debug = false;
        public bool Trace = false;

        public class IconOpts {
            public string VisionAndSensors = "lv_cyber-eye";
            public string SensorsDisabled = "lv_sight-disabled";
            public string ElectronicWarfare = "lv_eye-shield";
            public string TargetSensorsMark = "lv_radar-sweep"; 
            public string TargetVisualsMark = "lv_eye-target";
        }
        public IconOpts Icons = new IconOpts();

        public class ToggleOpts {
            public bool LogEffectsOnMove = false;
            public bool ShowNightVision = true;
            public bool MimeticUsesGhost = true;
        }
        public ToggleOpts Toggles = new ToggleOpts();

        // The base range (in hexes) for a unit's sensors
        public class SensorRangeOpts {
            public int MechTypeRange = 12;
            public int VehicleTypeRange = 9;
            public int TurretTypeRange = 15;
            public int UnknownTypeRange = 6;

            // The minimum range for sensors, no matter the circumstances
            public int MinimumRangeHexes = 8;

            // If true, sensor checks always fail on the first turn of the game
            public bool SensorsOfflineAtSpawn = true;

            // The maximum penalty that ECM Shield + ECM Jamming should apply TO SENSOR DETAILS ONLY
            public int MaxECMDetailsPenalty = -8;

            public float MinimumSensorRange() { return this.MinimumRangeHexes * 30.0f; }
        }
        public SensorRangeOpts Sensors = new SensorRangeOpts();

        // The base range (in hexes) for a unit's vision
        public class VisionRangeOpts {
            public int BaseRangeBright = 15;
            public int BaseRangeDim = 11;
            public int BaseRangeDark = 7;

            // The multiplier used for weather effects
            public float RangeMultiRainSnow = 0.8f;
            public float RangeMultiLightFog = 0.66f;
            public float RangeMultiHeavyFog = 0.33f;

            // The minium range for vision, no matter the circumstances
            public int MinimumRangeHexes = 3;

            // The range (in hexes) from which you can identify some elements of a unit
            public int ScanRangeHexes = 7;

            // If true, terrain is visible outside of the immediate fog of war boundaires
            public bool ShowTerrainThroughFogOfWar = true;

            public float MinimumVisionRange() { return this.MinimumRangeHexes * 30.0f; }
        }
        public VisionRangeOpts Vision = new VisionRangeOpts();

        public class AttackOpts {
            public int NoVisualsPenalty = 5;
            public int NoSensorsPenalty = 5;
            public int FiringBlindPenalty = 13;
        }
        public AttackOpts Attack = new AttackOpts();

        public class ProbabilityOpts {
            // The inflection point of the probability distribution function.
            public int Sigma = 4;
            // The inflection point of the probability distribution function.
            public int Mu = -1;
        }
        public ProbabilityOpts Probability = new ProbabilityOpts();

        // === Localization ===

        // Map effect keys
        public const string LT_MAP_LIGHT_BRIGHT = "MAP_LIGHT_BRIGHT";
        public const string LT_MAP_LIGHT_DIM = "MAP_LIGHT_DIM";
        public const string LT_MAP_LIGHT_DARK = "MAP_LIGHT_DARK";
        public const string LT_MAP_FOG_LIGHT = "MAP_FOG_LIGHT";
        public const string LT_MAP_FOG_HEAVY = "MAP_FOG_HEAVY";
        public const string LT_MAP_SNOW = "MAP_SNOW";
        public const string LT_MAP_RAIN = "MAP_RAIN";

        // Status Panel
        public const string LT_PANEL_SENSORS = "PANEL_SENSORS";
        public const string LT_PANEL_VISUALS = "PANEL_VISUALS";
        public const string LT_PANEL_DETAILS = "PANEL_DETAILS";
        public const string LT_PANEL_HEAT = "PANEL_HEAT_VISION";
        public const string LT_PANEL_ZOOM = "PANEL_ZOOM_VISION";

        // Sensor details keys
        public const string LT_DETAILS_NONE = "DETAILS_NONE";
        public const string LT_DETAILS_LOCATION_AND_TYPE = "LOCATION_AND_TYPE";
        public const string LT_DETAILS_ARMOR_AND_WEAPON_TYPE = "ARMOR_AND_WEAPON_TYPE";
        public const string LT_DETAILS_STRUCT_AND_WEAPON_ID = "STRUCT_AND_WEAPON_ID";
        public const string LT_DETAILS_ALL_INFO = "ALL_INFO";
        public const string LT_DETAILS_UNKNOWN = "DETAILS_UNKNOWN";

        public const string LT_UNIT_TYPE_MECH = "UNIT_TYPE_MECH";
        public const string LT_UNIT_TYPE_VEHICLE = "UNIT_TYPE_VEHICLE";
        public const string LT_UNIT_TYPE_TURRET = "UNIT_TYPE_TURRET";

        // Attack Tooltip
        public const string LT_ATTACK_FIRING_BLIND = "ATTACK_FIRE_BLIND";
        public const string LT_ATTACK_NO_VISUALS = "ATTACK_NO_VISUALS";
        public const string LT_ATTACK_ZOOM_VISION = "ATTACK_ZOOM_VISION";
        public const string LT_ATTACK_HEAT_VISION = "ATTACK_HEAT_VISION";
        public const string LT_ATTACK_MIMETIC = "ATTACK_MIMETIC_ARMOR";
        public const string LT_ATTACK_NO_SENSORS = "ATTACK_NO_SENSORS";
        public const string LT_ATTACK_ECM_SHEILD = "ATTACK_ECM_SHIELD";
        public const string LT_ATTACK_STEALTH = "ATTACK_STEALTH";
        public const string LT_ATTACK_NARCED = "ATTACK_NARCED";
        public const string LT_ATTACK_TAGGED = "ATTACK_TAGGED";

        // Floatie texts
        public const string LT_FLOATIE_ECM_SHIELD = "FLOATIE_ECM_SHIELDED";
        public const string LT_FLOATIE_ECM_JAMMED = "FLOATIE_ECM_JAMMED";

        // Targeting computer 
        public const string LT_TARG_COMP_BALLISTIC = "TARG_COMP_BALLISTIC";
        public const string LT_TARG_COMP_ENERGY = "TARG_COMP_ENERGY";
        public const string LT_TARG_COMP_MISSILE = "TARG_COMP_MISSILE";
        public const string LT_TARG_COMP_PHYSICAL = "TARG_COMP_PHYSICAL";
        public const string LT_TARG_COMP_UNIDENTIFIED = "TARG_COMP_UNIDENTIFIED";

        // HUD ToolTips
        public const string LT_TT_TITLE_VISION_AND_SENSORS = "TOOLTIP_TITLE_VISION_AND_SENSORS";
        public const string LT_TT_TITLE_SENSORS_DISABLED = "TOOLTIP_TITLE_SENSORS_DISABLED";
        public const string LT_TT_TEXT_SENSORS_DISABLED = "TOOLTIP_TEXT_SENSORS_DISABLED";
        public const string LT_TT_TITLE_EW = "TOOLTIP_TITLE_EW";
        public const string LT_TT_TEXT_EW_ECM_SHIELD = "TOOLTIP_LABEL_EW_ECM_SHIELD";
        public const string LT_TT_TEXT_EW_ECM_JAMMING = "TOOLTIP_LABEL_EW_ECM_JAMMING";
        public const string LT_TT_TEXT_EW_PROBE_CARRIER = "TOOLTIP_LABEL_EW_PROBE_CARRIER";
        public const string LT_TT_TEXT_EW_MIMETIC = "TOOLTIP_LABEL_EW_MIMETIC";
        public const string LT_TT_TEXT_EW_STEALTH = "TOOLTIP_LABEL_EW_STEALTH";
        public const string LT_TT_TEXT_EW_NARC_EFFECT = "TOOLTIP_LABEL_EW_NARC_EFFECT";
        public const string LT_TT_TEXT_EW_PROBE_EFFECT = "TOOLTIP_LABEL_EW_PROBE_EFFECT";
        public const string LT_TT_TEXT_EW_TAG_EFFECT = "TOOLTIP_LABEL_EW_TAG_EFFECT";

        // CAC TargetInfo strings
        public const string LT_CAC_SIDEPANEL_TITLE = "SIDEPANEL_TITLE";
        public const string LT_CAC_SIDEPANEL_MOVE_MECH = "SIDEPANEL_MOVE_MECH";
        public const string LT_CAC_SIDEPANEL_MOVE_VEHICLE = "SIDEPANEL_MOVE_VEHICLE";
        public const string LT_CAC_SIDEPANEL_DIST = "SIDEPANEL_DISTANCE";
        public const string LT_CAC_SIDEPANEL_HEAT = "SIDEPANEL_HEAT";
        public const string LT_CAC_SIDEPANEL_STAB = "SIDEPANEL_DAMAGE";
        public const string LT_CAC_SIDEPANEL_WEIGHT = "SIDEPANEL_WEIGHT";

        public Dictionary<string, string> LocalizedText = new Dictionary<string, string>() {


            // Map effects
            { LT_MAP_LIGHT_BRIGHT, "Bright" },
            { LT_MAP_LIGHT_DIM, "Dim" },
            { LT_MAP_LIGHT_DARK, "Dark" },
            { LT_MAP_FOG_LIGHT, "Fog" },
            { LT_MAP_FOG_HEAVY, "Dense Fog" },
            { LT_MAP_SNOW, "Snow" },
            { LT_MAP_RAIN, "Rain" },

            // Status Panel
            { LT_PANEL_SENSORS, "<b>Sensors</b><size=90%> <color=#{0}>{1:#.00}m</color> Multi:<color=#{2}> x{3}</color> [{4}]\n" },
            { LT_PANEL_VISUALS, "<b>Visuals</b><size=90%> <color=#00FF00>{0:0}m</color> Scan:{1}m [{2}]\n" },
            { LT_PANEL_DETAILS, "  Total: <color=#{0}>{1:+0;-#}</color><size=90%> Roll: <color=#{2}>{3:+0;-#}</color> Tactics: <color=#00FF00>{4:+0;-#}</color> AdvSen: <color=#{5}>{6:+0;-#}</color>\n" },
            { LT_PANEL_HEAT, "<b>Thermals</b><size=90%> Mod: <color=#{0}>{1:+0;-#}</color> / {2} heat Range: {3}m\n" },
            { LT_PANEL_ZOOM, "<b>Zoom</b><size=90%> Mod: <color=#{0}>{1:+0;-#}</color> Cap: <color=#{2}>{3:+0;-#}</color> Range: {4}m\n" },

            // Sensor Details Level
            { LT_DETAILS_NONE, "No Info" },
            { LT_DETAILS_LOCATION_AND_TYPE, "Type" },
            { LT_DETAILS_ARMOR_AND_WEAPON_TYPE, "Armor + Weapon Type" },
            { LT_DETAILS_STRUCT_AND_WEAPON_ID, "Struct + Weapon" },
            { LT_DETAILS_ALL_INFO, "All Info" },
            { LT_DETAILS_UNKNOWN, "Unknown" },

            { LT_UNIT_TYPE_MECH, "MECH" },
            { LT_UNIT_TYPE_VEHICLE, "VEHICLE" },
            { LT_UNIT_TYPE_TURRET, "TURRET" },

            // Attack Tooltip
            { LT_ATTACK_FIRING_BLIND, "FIRING BLIND" },
            { LT_ATTACK_NO_SENSORS, "NO SENSORS" },
            { LT_ATTACK_NO_VISUALS, "NO VISUALS" },
            { LT_ATTACK_ZOOM_VISION, "ZOOM VISION" },
            { LT_ATTACK_HEAT_VISION, "HEAT VISION" },
            { LT_ATTACK_MIMETIC, "MIMETIC ARMOR" },
            { LT_ATTACK_STEALTH, "STEALTH" },
            { LT_ATTACK_ECM_SHEILD, "ECM SHIELD" },
            { LT_ATTACK_NARCED, "NARCED" },
            { LT_ATTACK_TAGGED, "TAGGED" },

            // Floatie text
            { LT_FLOATIE_ECM_JAMMED, "ECM JAMMING" },
            { LT_FLOATIE_ECM_SHIELD, "ECM SHIELDING" },

            // Targeting computer text
            { LT_TARG_COMP_BALLISTIC, "Ballistic" },
            { LT_TARG_COMP_ENERGY, "Energy" },
            { LT_TARG_COMP_MISSILE, "Missile" },
            { LT_TARG_COMP_PHYSICAL, "Physical" },
            { LT_TARG_COMP_UNIDENTIFIED, "Unidentified" },

            // HUD Tooltips
            { LT_TT_TITLE_VISION_AND_SENSORS, "VISION AND SENSORS" },
            { LT_TT_TITLE_SENSORS_DISABLED, "SENSORS OFFLINE" },
            { LT_TT_TEXT_SENSORS_DISABLED, "Sensors offline during the first round of the battle." },
            
            // Electronic Warfare options
            { LT_TT_TITLE_EW, "ELECTRONIC WARFARE" },
            { LT_TT_TEXT_EW_ECM_SHIELD, "<size=90%><b>ECM SHIELD</b>: <color=#{0}>{1:+0;-#}</color> to detect or attack.\n" },
            { LT_TT_TEXT_EW_ECM_JAMMING, "<size=90%><b>ECM JAMMING</b>: <color=#{0}>{1:+0;-#} sensor details.</color>\n" },
            { LT_TT_TEXT_EW_PROBE_CARRIER, "<size=90%><b>ACTIVE PROBE</b>: <color=#{0}>{1:+0;-#}</color> to enemy ECM, Stealth, Mimetic\n" },
            { LT_TT_TEXT_EW_NARC_EFFECT, "<size=90%><b>NARCED</b>: <color=#{0}>{1:+0;-#}</color> to attack and detect.\n" },
            { LT_TT_TEXT_EW_PROBE_EFFECT, "<size=90%><b>PROBE PINGED</b>: <color=#{0}>{1:+0;-#}</color> to detect.\n" },
            { LT_TT_TEXT_EW_TAG_EFFECT, "<size=90%><b>TAGGED</b>: <color=#{0}>{1:+0;-#}</color> attack and detection modifier.\n" },
            { LT_TT_TEXT_EW_MIMETIC, "<size=90%><b>MIMETIC</b>: <color=#{0}>{1:+0;-#}</color> attack modifier.\n" },
            { LT_TT_TEXT_EW_STEALTH, "<size=90%><b>STEALTH</b>: med. <color=#{0}>{1:+0;-#}</color> / long <color=#{0}>{2:+0;-#}</color> / ext. <color=#{0}>{3:+0;-#}</color> attack modifier\n" },

            // CAC Sidepanel strings
            { LT_CAC_SIDEPANEL_TITLE, "  {0} {1}\n" },
            { LT_CAC_SIDEPANEL_MOVE_MECH, "  Walk: {0}m  Run: {1}m  Jump: {2}m\n" },
            { LT_CAC_SIDEPANEL_MOVE_VEHICLE, "  Cruise: {0}m  Flank: {1}m\n" },
            { LT_CAC_SIDEPANEL_DIST, "  Distance: {0}m\n" },
            { LT_CAC_SIDEPANEL_HEAT, "  Heat: {0} of {1}\n" },
            { LT_CAC_SIDEPANEL_STAB, "  Instability: {0} of {1}\n" },
            { LT_CAC_SIDEPANEL_WEIGHT, "{0} tons" }
    };

        public void LogConfig() {
            Mod.Log.Info("=== MOD CONFIG BEGIN ===");
            Mod.Log.Info($"  DEBUG:{this.Debug} Trace:{this.Trace}");

            Mod.Log.Info($"  == Probability ==");
            Mod.Log.Info($"ProbabilitySigma:{Probability.Sigma}, ProbabilityMu:{Probability.Mu}");
            
            Mod.Log.Info($"  == Sensors ==");
            Mod.Log.Info($"Type Ranges - Mech: {Sensors.MechTypeRange} Vehicle: {Sensors.VehicleTypeRange} Turret: {Sensors.TurretTypeRange} UnknownType: {Sensors.UnknownTypeRange}");
            Mod.Log.Info($"MinimumRange: {Sensors.MinimumRangeHexes}  FirstTurnForceFailedChecks: {Sensors.SensorsOfflineAtSpawn}  MaxECMDetailsPenalty: {Sensors.MaxECMDetailsPenalty}");

            Mod.Log.Info($"  == Vision ==");
            Mod.Log.Info($"Vision Ranges - Bright: {Vision.BaseRangeBright} Dim:{Vision.BaseRangeDim} Dark:{Vision.BaseRangeDark}");
            Mod.Log.Info($"Range Multis - Rain/Snow: x{Vision.RangeMultiRainSnow} Light Fog: x{Vision.RangeMultiLightFog} HeavyFog: x{Vision.RangeMultiHeavyFog}");
            Mod.Log.Info($"Minimum range: {Vision.MinimumRangeHexes} ScanRange: {Vision.ScanRangeHexes}");
            Mod.Log.Info($"ShowTerrainThroughFogOfWar: {Vision.ShowTerrainThroughFogOfWar}");

            Mod.Log.Info($"  == Attacking ==");
            //Mod.Log.Info($"Penalties - NoSensors:{Attack.NoSensorInfoPenalty} NoVisuals:{Attack.NoVisualsPenalty} BlindFire:{Attack.BlindFirePenalty}");
            //Mod.Log.Info($"Criticals Penalty - NoSensors:{Attack.NoSensorsCriticalPenalty} NoVisuals:{Attack.NoVisualsCriticalPenalty}");
            //Mod.Log.Info($"HeatVisionMaxBonus: {Attack.MaxHeatVisionBonus}");

            Mod.Log.Info("=== MOD CONFIG END ===");
        }
    }
}
