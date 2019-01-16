using BattleTech.Rendering.Mood;
using HBS.Collections;
using System;

namespace LowVisibility.Helper {
    public static class MapHelper {

        public static float CalculateMapVisionRange() {
            MoodController moodController = UnityEngine.Object.FindObjectOfType<MoodController>();
            MoodSettings moodSettings = moodController.CurrentMood;
            TagSet moodTags = moodSettings.moodTags;

            if (moodTags.IsEmpty) { return 0.0f; }

            String allTags = String.Join(", ", moodTags.ToArray());
            LowVisibility.Logger.LogIfDebug($"  - All mood tags are: {allTags}");

            float baseVision = LowVisibility.Config.VisionRangeBaseDaylight;
            float visionMulti = 1.0f;
            foreach (string tag in moodTags) {
                switch (tag) {
                    case "mood_timeMorning":
                    case "mood_timeNoon":
                    case "mood_timeAfternoon":
                    case "mood_timeDay":
                        LowVisibility.Logger.LogIfDebug($"  - {tag}");
                        break;
                    case "mood_timeSunrise":
                    case "mood_timeSunset":
                    case "mood_timeTwilight":
                        LowVisibility.Logger.LogIfDebug($"  - {tag}");
                        if (baseVision > LowVisibility.Config.VisionRangeBaseDimlight) { baseVision = LowVisibility.Config.VisionRangeBaseDimlight; }
                        break;
                    case "mood_timeNight":
                        LowVisibility.Logger.LogIfDebug($"  - {tag}");
                        if (baseVision > LowVisibility.Config.VisionRangeBaseNight) { baseVision = LowVisibility.Config.VisionRangeBaseNight; }
                        break;
                    case "mood_weatherRain":
                    case "mood_weatherSnow":
                        LowVisibility.Logger.LogIfDebug($"  - {tag}");
                        if (visionMulti > LowVisibility.Config.VisionRangeMultiRainSnow) { visionMulti = LowVisibility.Config.VisionRangeMultiRainSnow; }
                        break;
                    case "mood_fogLight":
                        LowVisibility.Logger.LogIfDebug($"  - {tag}");
                        if (visionMulti > LowVisibility.Config.VisionRangeMultiLightFog) { visionMulti = LowVisibility.Config.VisionRangeMultiLightFog; }
                        break;
                    case "mood_fogHeavy":
                        LowVisibility.Logger.LogIfDebug($"  - {tag}");
                        if (visionMulti > LowVisibility.Config.VisionRangeMultiHeavyFog) { visionMulti = LowVisibility.Config.VisionRangeMultiHeavyFog; }
                        break;
                    default:
                        break;
                }
            }            
            float visRange = (float)Math.Ceiling(baseVision * 30.0f * visionMulti);
            LowVisibility.Logger.Log($"  Calculating vision range as Math.Ceil(baseVision:{baseVision} * 30.0 * visionMulti:{visionMulti}) = visRange:{visRange}.");

            if (visRange < LowVisibility.Config.VisionRangeMinimum) { visRange = LowVisibility.Config.VisionRangeMinimum; }
            LowVisibility.Logger.Log($"MapHelper: Vision range for map will be ==> {visRange}m.");
            return visRange;
        }
    }
}
