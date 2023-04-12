namespace LowVisibility
{

    public class ModStats
    {
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

        public const string DisableSensors = "LV_DISABLE_SENSORS"; // int

        public const string HBS_SensorSignatureModifier = "SensorSignatureModifier";

        // CAE Stats - write carefully!
        public const string CAESensorsRange = "CAE_SENSORS_RANGE";

        public static bool IsStealthStat(string statName)
        {
            return statName != null && statName != "" &&
                (statName.Equals(ModStats.StealthEffect) || statName.Equals(ModStats.MimeticEffect));
        }
    }

}
