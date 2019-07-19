
namespace LowVisibility {

    public class ModStats {

        public const string TacticsMod = "LV_TacticsMod"; // Int_32

        public const string CurrentRoundEWCheck = "LV_Current_Round_EW_Check"; // Int_32

        // ECM 
        public const string ECMCarrier = "LV_ECM_Carrier"; // Int_32
        public const string ECMShield = "LV_ECM_Shield"; // Int_32
        public const string ECMJammed = "LV_ECM_Jammed"; // Int_32

        // Sensors
        public const string AdvancedSensors = "LV_Advanced_Sensors";

        // Probe    
        public const string ProbeCarrier = "LV_Probe_Carrier";
        public const string ProbeSweepTarget = "LV_Probe_Sweep_Target";

        public const string StealthEffect = "LV_Stealth"; // String
        public const string MimeticEffect = "LV_Mimetic"; // String
        public const string MimeticCurrentSteps = "LV_Mimetic_Current_Steps"; // Int_32

        // Sensor sharing
        public const string SharesSensors = "LV_Shares_Sensors";

        // Vision modes
        public const string HeatVision = "LV_Heat_Vision";
        public const string ZoomVision = "LV_Zoom_Vision";
        public const string NightVision = "LV_Night_Vision"; // TODO

        public static bool IsStealthStat(string statName) {
            return statName != null && statName != "" && (
                statName.Equals(ModStats.StealthEffect) ||
                statName.Equals(ModStats.MimeticEffect)
                );
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

        // Applied when the attacker has sensor but no visual lock to a target.
        public int VisionOnlyPenalty = 1;
        public float VisionOnlyCriticalPenalty = 0.0f;

        // Applied when the attacker has sensor but no visual lock to a target.
        public int SensorsOnlyPenalty = 2;
        public float SensorsOnlyCriticalPenalty = 0.0f;

        public int MultipleECMSourceModifier = 1;

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
            Mod.Log.Info($"SensorsOnlyPenalty:{SensorsOnlyPenalty}, SensorsOnlyCriticalPenalty:{SensorsOnlyCriticalPenalty}");

            Mod.Log.Info($"  == Vision ==");
            Mod.Log.Info($"VisionRangeBaseDaylight:{VisionRangeBaseDaylight} VisionRangeBaseDimlight:{VisionRangeBaseDimlight} VisionRangeBaseNight:{VisionRangeBaseNight}");
            Mod.Log.Info($"VisionRangeMultiRainSnow:{VisionRangeMultiRainSnow} VisionRangeMultiLightFog:{VisionRangeMultiLightFog} VisionRangeMultiHeavyFog:{VisionRangeMultiHeavyFog}");
            Mod.Log.Info($"VisionRangeMinimum:{VisionRangeMinimum} SensorRangeMinimum:{SensorRangeMinimum}, VisualIDRange:{VisualScanRange}");
            Mod.Log.Info($"VisionOnlyPenalty:{VisionOnlyPenalty} VisionOnlyCriticalPenalty:{VisionOnlyCriticalPenalty}");

            Mod.Log.Info("=== MOD CONFIG END ===");
        }
    }
}
