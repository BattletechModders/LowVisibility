using UnityEngine;

namespace LowVisibility {

    public class ModConfig {
        // If true, extra logging will be used
        public bool Debug = false;
        public bool Trace = false;

        public class IconOpts {
            public string ElectronicWarfare = "lv_eye-shield"; // 00aad4ff
            public string SensorsDisabled = "lv_sight-disabled"; // ff0000ff
            public string VisionAndSensors = "lv_cyber-eye";

            public string TargetSensorsMark = "lv_radar-sweep";
            public string TargetVisualsMark = "lv_brass-eye";
            public string TargetTaggedMark = "lv_target-laser";
            public string TargetNarcedMark = "lv_radio-tower";
            public string TargetStealthMark = "lv_robber-mask";
            public string TargetMimeticMark = "lv_static";
            public string TargetECMShieldedMark = "lv_eye-shield";
            public string TargetActiveProbePingedMark = "lv_eye-target";

            public float[] MarkColorPlayerPositive = new float[] { 1f, 0f, 0.062f, 1f };
            public Color PlayerPositiveMarkColor;
            public float[] MarkColorPlayerNegative = new float[] { 0f, 0.901f, 0.098f, 1f };
            public Color PlayerNegativeMarkColor;
        }
        public IconOpts Icons = new IconOpts();

        public class ToggleOpts {
            public bool LogEffectsOnMove = false;
            public bool ShowNightVision = true;
            public bool MimeticUsesGhost = true;
        }
        public ToggleOpts Toggles = new ToggleOpts();

        public class SensorRangeOpts {
            public float MechRange = 360f;
            public float TrooperRange = 180f;
            public float VehicleRange = 270f;
            public float TurretRange = 450f;
            public float UnknownRange = 180f;

            // The minimum range for sensors, no matter the circumstances
            public float MinimumRange = 180f;

            // If true, sensor checks always fail on the first turn of the game
            public bool SensorsOfflineAtSpawn = true;

            // The maximum penalty that ECM Shield + ECM Jamming should apply TO SENSOR DETAILS ONLY
            public int MaxECMDetailsPenalty = -8;

            // The minimum signature that we'll allow after all modifiers are applied
            public float MinSignature = 0.05f;

        }
        public SensorRangeOpts Sensors = new SensorRangeOpts();

        public class VisionRangeOpts {

            public float RangeBright = 450f;
            public float RangeDim = 330f;
            public float RangeDark = 210f;

            // The multiplier used for weather effects
            public float RangeMultiRainSnow = 0.8f;
            public float RangeMultiLightFog = 0.66f;
            public float RangeMultiHeavyFog = 0.33f;

            // The minium range for vision, no matter the circumstances
            public float MinimumRange = 90f;

            // The range from which you can identify some elements of a unit
            public float ScanRange = 210f;

        }
        public VisionRangeOpts Vision = new VisionRangeOpts();

        public class FogOfWarOpts
        {
            // If true, fog of war will be redrawn on each unit's activation
            public bool RedrawFogOfWarOnActivation = false;

            // If true, terrain is visible outside of the immediate fog of war boundaires
            public bool ShowTerrainThroughFogOfWar = true;
        }
        public FogOfWarOpts FogOfWar = new FogOfWarOpts();

        public class AttackOpts {
            public int NoVisualsPenalty = 5;
            public int NoSensorsPenalty = 5;
            public int FiringBlindPenalty = 13;

            public float ShieldedMulti = 1.0f;
            public float JammedMulti = 1.0f;
        }
        public AttackOpts Attack = new AttackOpts();

        public class ProbabilityOpts {
            // The inflection point of the probability distribution function.
            public int Sigma = 4;
            // The inflection point of the probability distribution function.
            public int Mu = -1;
        }
        public ProbabilityOpts Probability = new ProbabilityOpts();

        public void LogConfig() {
            Mod.Log.Info?.Write("=== MOD CONFIG BEGIN ===");
            Mod.Log.Info?.Write($"  DEBUG:{this.Debug} Trace:{this.Trace}");

            Mod.Log.Info?.Write($"  == Probability ==");
            Mod.Log.Info?.Write($"ProbabilitySigma:{Probability.Sigma}, ProbabilityMu:{Probability.Mu}");
            
            Mod.Log.Info?.Write($"  == Sensors ==");
            Mod.Log.Info?.Write($"Type Ranges - Mech: {Sensors.MechRange} Trooper: {Sensors.TrooperRange} Vehicle: {Sensors.VehicleRange} Turret: {Sensors.TurretRange} UnknownType: {Sensors.UnknownRange}");
            Mod.Log.Info?.Write($"MinimumRange: {Sensors.MinimumRange}  FirstTurnForceFailedChecks: {Sensors.SensorsOfflineAtSpawn}  MaxECMDetailsPenalty: {Sensors.MaxECMDetailsPenalty}");

            Mod.Log.Info?.Write($"  == Vision ==");
            Mod.Log.Info?.Write($"Vision Ranges - Bright: {Vision.RangeBright} Dim:{Vision.RangeDim} Dark:{Vision.RangeDark}");
            Mod.Log.Info?.Write($"Range Multis - Rain/Snow: x{Vision.RangeMultiRainSnow} Light Fog: x{Vision.RangeMultiLightFog} HeavyFog: x{Vision.RangeMultiHeavyFog}");
            Mod.Log.Info?.Write($"Minimum range: {Vision.MinimumRange} ScanRange: {Vision.ScanRange}");

            Mod.Log.Info?.Write($"  == FogOfWar ==");
            Mod.Log.Info?.Write($"RedrawFogOfWarOnActivation: {FogOfWar.RedrawFogOfWarOnActivation}  ShowTerrainThroughFogOfWar: {FogOfWar.ShowTerrainThroughFogOfWar}");

            Mod.Log.Info?.Write($"  == Attacking ==");
            //Mod.Log.Info?.Write($"Penalties - NoSensors:{Attack.NoSensorInfoPenalty} NoVisuals:{Attack.NoVisualsPenalty} BlindFire:{Attack.BlindFirePenalty}");
            //Mod.Log.Info?.Write($"Criticals Penalty - NoSensors:{Attack.NoSensorsCriticalPenalty} NoVisuals:{Attack.NoVisualsCriticalPenalty}");
            //Mod.Log.Info?.Write($"HeatVisionMaxBonus: {Attack.MaxHeatVisionBonus}");

            Mod.Log.Info?.Write("=== MOD CONFIG END ===");
        }

        public void Init()
        {
            this.Icons.PlayerPositiveMarkColor = new Color(this.Icons.MarkColorPlayerPositive[0], this.Icons.MarkColorPlayerPositive[1], this.Icons.MarkColorPlayerPositive[2], this.Icons.MarkColorPlayerPositive[3]);
            this.Icons.PlayerNegativeMarkColor = new Color(this.Icons.MarkColorPlayerNegative[0], this.Icons.MarkColorPlayerNegative[1], this.Icons.MarkColorPlayerNegative[2], this.Icons.MarkColorPlayerNegative[3]);
        }
    }
}
