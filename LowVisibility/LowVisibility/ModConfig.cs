
using System.Collections.Generic;

namespace LowVisibility {

    public class ModStats {
        // WARNING: HBS Code upper-cases all stat names; if you try to comparison match in a case-sensitive fashion
        //  it will fail. Better to uppercase everthing.

        public const string TacticsMod = "LV_TACTICS_MOD"; // Int_32

        public const string CurrentRoundEWCheck = "LV_CURRENT_ROUND_EW_CHECK"; // Int_32

        // ECM 
        public const string ECMCarrier = "LV_ECM_CARRIER"; // Int_32
        public const string ECMShield = "LV_ECM_SHIELD"; // Int_32
        public const string ECMShieldEmitterCount = "LV_ECM_SHIELD_EMITTER_COUNT"; // Int_32
        public const string ECMJamming = "LV_ECM_JAMMED"; // Int_32
        public const string ECMJammingEmitterCount = "LV_ECM_JAM_EMITTER_COUNT"; // Int_32
        public const string ECMVFXEnabled = "LV_ECM_VFX_ENABLED"; // Boolean

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
        
        public static bool IsStealthStat(string statName) {
            return statName != null && statName != "" && 
                (statName.Equals(ModStats.StealthEffect) || statName.Equals(ModStats.MimeticEffect));
        }
    }

    public class ModConfig {
        // If true, extra logging will be used
        public bool Debug = false;
        public bool Trace = false;

        // The base range (in hexes) for a unit's sensors
        public class SensorRangeOpts {
            public int MechTypeRange = 12;
            public int VehicleTypeRange = 9;
            public int TurretTypeRange = 15;
            public int UnknownTypeRange = 6;

            // The minium range for sensors, no matter the circumstances
            public int MinimumRangeHexes = 8;

            // If true, sensor checks always fail on the first turn of the game
            public bool FirstTurnForceFailedChecks = true;

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
            public int NoSensorInfoPenalty = 5; // Applied when the attacker cannot detect the target
            public int NoVisualsPenalty = 5; // Applied when the attacker cannot spot the target
            public int BlindFirePenalty = 13; // Applied if both of the above are true

            public float NoSensorsCriticalPenalty = 0.0f;
            public float NoVisualsCriticalPenalty = 0.0f;

            // The maximum attack bonus for heat vision
            public int MaxHeatVisionBonus = 5;
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
        public const string LT_PANEL_SENSOR_RANGE = "PANEL_SENSOR_RANGE";
        public const string LT_PANEL_VISUAL_RANGE = "PANEL_VISUAL_RANGE";

        // Sensor details keys
        public const string LT_DETAILS_NONE = "DETAILS_NONE";
        public const string LT_DETAILS_LOCATION = "DETAILS_LOCATION";
        public const string LT_DETAILS_TYPE = "DETAILS_TYPE";
        public const string LT_DETAILS_SILHOUETTE = "DETAILS_SILHOUETTE";
        public const string LT_DETAILS_VECTOR = "DETAILS_VECTOR";
        public const string LT_DETAILS_SURFACE_SCAN = "DETAILS_SURFACE_SCAN";
        public const string LT_DETAILS_SURFACE_ANALYZE = "DETAILS_SURFACE_ANALYZE";
        public const string LT_DETAILS_WEAPON_ANALYZE = "DETAILS_WEAPON_ANALYZE";
        public const string LT_DETAILS_STRUCTURE_ANALYZE = "DETAILS_STRUCTURE_ANALYZE";
        public const string LT_DETAILS_DEEP_SCAN = "DETAILS_DEEP_SCAN";
        public const string LT_DETAILS_PILOT = "DETAILS_PILOT";
        public const string LT_DETAILS_UNKNOWN = "DETAILS_UNKNOWN";

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
            { LT_PANEL_SENSOR_RANGE, "Sensors - Detect:<color=#{0}>{1:0}m</> Multi:<color=#{2}> x{3}</color> [{4}]\n" },
            { LT_PANEL_VISUAL_RANGE, "Visuals - Lock:{0:0}m Scan :{1}m [{2}]\n" },

            // Sensor Details Level
            { LT_DETAILS_NONE, "No Info" },
            { LT_DETAILS_LOCATION, "Location" },
            { LT_DETAILS_TYPE, "Unit Type" },
            { LT_DETAILS_SILHOUETTE, "Silhouettte" },
            { LT_DETAILS_VECTOR, "Vector" },
            { LT_DETAILS_SURFACE_SCAN, "Surface Only" },
            { LT_DETAILS_SURFACE_ANALYZE, "SurfaceAnalysis" },
            { LT_DETAILS_WEAPON_ANALYZE, "Weapons" },
            { LT_DETAILS_STRUCTURE_ANALYZE, "Structural" },
            { LT_DETAILS_DEEP_SCAN, "Complete" },
            { LT_DETAILS_PILOT, "Pilot" },
            { LT_DETAILS_UNKNOWN, "Unknown" },

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
        };

        public void LogConfig() {
            Mod.Log.Info("=== MOD CONFIG BEGIN ===");
            Mod.Log.Info($"  DEBUG:{this.Debug} Trace:{this.Trace}");

            Mod.Log.Info($"  == Probability ==");
            Mod.Log.Info($"ProbabilitySigma:{Probability.Sigma}, ProbabilityMu:{Probability.Mu}");
            
            Mod.Log.Info($"  == Sensors ==");
            Mod.Log.Info($"Type Ranges - Mech: {Sensors.MechTypeRange} Vehicle: {Sensors.VehicleTypeRange} Turret: {Sensors.TurretTypeRange} UnknownType: {Sensors.UnknownTypeRange}");
            Mod.Log.Info($"MinimumRange: {Sensors.MinimumRangeHexes}  FirstTurnForceFailedChecks: {Sensors.FirstTurnForceFailedChecks} ");

            Mod.Log.Info($"  == Vision ==");
            Mod.Log.Info($"Vision Ranges - Bright: {Vision.BaseRangeBright} Dim:{Vision.BaseRangeDim} Dark:{Vision.BaseRangeDark}");
            Mod.Log.Info($"Range Multis - Rain/Snow: x{Vision.RangeMultiRainSnow} Light Fog: x{Vision.RangeMultiLightFog} HeavyFog: x{Vision.RangeMultiHeavyFog}");
            Mod.Log.Info($"Minimum range: {Vision.MinimumRangeHexes} ScanRange: {Vision.ScanRangeHexes}");
            Mod.Log.Info($"ShowTerrainThroughFogOfWar: {Vision.ShowTerrainThroughFogOfWar}");

            Mod.Log.Info($"  == Attacking ==");
            Mod.Log.Info($"Penalties - NoSensors:{Attack.NoSensorInfoPenalty} NoVisuals:{Attack.NoVisualsPenalty} BlindFire:{Attack.BlindFirePenalty}");
            Mod.Log.Info($"Criticals Penalty - NoSensors:{Attack.NoSensorsCriticalPenalty} NoVisuals:{Attack.NoVisualsCriticalPenalty}");
            Mod.Log.Info($"HeatVisionMaxBonus: {Attack.MaxHeatVisionBonus}");

            Mod.Log.Info("=== MOD CONFIG END ===");
        }
    }
}
