
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

        public bool FirstTurnForceFailedChecks = true;

        // The base range (in hexes) for a unit's sensors
        public int SensorRangeMechType = 12;
        public int SensorRangeVehicleType  = 9;
        public int SensorRangeTurretType = 15;
        public int SensorRangeUnknownType = 6;

        // The base range (in hexes) for a unit's vision
        public int VisionRangeBaseDaylight = 15;
        public int VisionRangeBaseDimlight = 11;
        public int VisionRangeBaseNight = 7;

        // The multiplier used for weather effects
        public float VisionRangeMultiRainSnow = 0.8f;
        public float VisionRangeMultiLightFog = 0.66f;
        public float VisionRangeMultiHeavyFog = 0.33f;

        // The minium range for vision, no matter the circumstances
        public int VisionRangeMinimum = 3;
        public float MinimumVisionRange() { return VisionRangeMinimum * 30.0f; }

        // The minium range for sensors, no matter the circumstances
        public int SensorRangeMinimum = 8;
        public float MinimumSensorRange() { return SensorRangeMinimum * 30.0f; }

        // The range (in hexes) from which you can identify some elements of a unit
        public int VisualScanRange = 7;
        
        public int NoSensorInfoPenalty = 5; // Applied when the attacker cannot detect the target
        public int NoVisualsPenalty = 5; // Applied when the attacker cannot spot the target
        public int BlindFirePenalty = 13; // Applied if both of the above are true

        public float VisionOnlyCriticalPenalty = 0.0f;
        public float SensorsOnlyCriticalPenalty = 0.0f;

        public int MultipleECMSourceModifier = 1;

        public bool ShowTerrainThroughFogOfWar = true;

        // The maximum attack bonus for heat vision
        public int HeatVisionMaxBonus = 5;

        // The inflection point of the probability distribution function.
        public int ProbabilitySigma = 4;
        // The inflection point of the probability distribution function.
        public int ProbabilityMu = -1;

        public void LogConfig() {
            Mod.Log.Info("=== MOD CONFIG BEGIN ===");
            Mod.Log.Info($"  DEBUG:{this.Debug} Trace:{this.Trace}");
            Mod.Log.Info($"FirstTurnForceFailedChecks:{FirstTurnForceFailedChecks}, MultipleJammerPenalty:{MultipleECMSourceModifier}");

            Mod.Log.Info($"  == Probability ==");
            Mod.Log.Info($"ProbabilitySigma:{ProbabilitySigma}, ProbabilityMu:{ProbabilityMu}");
            
            Mod.Log.Info($"  == Sensors ==");
            Mod.Log.Info($"Mech:{SensorRangeMechType} Vehicle:{SensorRangeVehicleType} Turret:{SensorRangeTurretType} UnknownType:{SensorRangeUnknownType}");
            Mod.Log.Info($"SensorsOnlyCriticalPenalty:{SensorsOnlyCriticalPenalty}");

            Mod.Log.Info($"  == Vision ==");
            Mod.Log.Info($"VisionRangeBaseDaylight:{VisionRangeBaseDaylight} VisionRangeBaseDimlight:{VisionRangeBaseDimlight} VisionRangeBaseNight:{VisionRangeBaseNight}");
            Mod.Log.Info($"VisionRangeMultiRainSnow:{VisionRangeMultiRainSnow} VisionRangeMultiLightFog:{VisionRangeMultiLightFog} VisionRangeMultiHeavyFog:{VisionRangeMultiHeavyFog}");
            Mod.Log.Info($"VisionRangeMinimum:{VisionRangeMinimum} SensorRangeMinimum:{SensorRangeMinimum}, VisualIDRange:{VisualScanRange}");
            Mod.Log.Info($"VisionOnlyCriticalPenalty:{VisionOnlyCriticalPenalty}");

            Mod.Log.Info($"  == Attacking ==");
            Mod.Log.Info($"NoSensorInfoPenalty:{NoSensorInfoPenalty} NoVisualsPenalty:{NoVisualsPenalty} BlindFirePenalty:{BlindFirePenalty}");

            Mod.Log.Info("=== MOD CONFIG END ===");
        }
    }
}
