using BattleTech.Rendering.Mood;
using HBS.Collections;
using System;
using System.Collections.Generic;

namespace LowVisibility.Helper {
    public static class MapHelper {

        private static readonly Dictionary<string, float[]> VisionStates = new Dictionary<string, float[]> {
            { "day", new float[] { 60.0f * 30, 15.0f * 30, 5.0f * 30} }, // 1800, 450, 150
            { "dim", new float[] { 16.0f * 30, 5.0f * 30, 3.0f * 30 } }, // 480, 150, 90
            { "night", new float[] { 6.0f * 30, 3.0f * 30, 2.0f * 30 } }, // 180, 90, 30
        };

        public static float CalculateMapVisionRange() {
            MoodController moodController = UnityEngine.Object.FindObjectOfType<MoodController>();
            MoodSettings moodSettings = moodController.CurrentMood;
            TagSet moodTags = moodSettings.moodTags;

            if (moodTags.IsEmpty) { return 0.0f; }

            String allTags = String.Join(", ", moodTags.ToArray());
            LowVisibility.Logger.LogIfDebug($"All mood tags are: {allTags}");

            string light = "day";
            int effect = 0;
            foreach (string tag in moodTags) {
                switch (tag) {
                    case "mood_timeMorning":
                    case "mood_timeNoon":
                    case "mood_timeAfternoon":
                    case "mood_timeDay":
                        LowVisibility.Logger.LogIfDebug($"Found daylight tag: {tag}");
                        light = "day";
                        break;
                    case "mood_timeSunrise":
                    case "mood_timeSunset":
                    case "mood_timeTwilight":
                        LowVisibility.Logger.LogIfDebug($"Found dimlight tag: {tag}");
                        light = "dim";
                        break;
                    case "mood_timeNight":
                        LowVisibility.Logger.LogIfDebug($"Found night tag: {tag}");
                        light = "night";
                        break;
                    case "mood_fogLight":
                    case "mood_weatherRain":
                    case "mood_weatherSnow":
                        LowVisibility.Logger.LogIfDebug($"Found rain/snow/lightFog tag: {tag}");
                        effect = 1;
                        break;
                    case "mood_fogHeavy":
                        LowVisibility.Logger.LogIfDebug($"Found heavyFog tag: {tag}");
                        effect = 2;
                        break;
                    default:
                        break;
                }
            }
            LowVisibility.Logger.LogIfDebug($"Light is:{light}, effect is:{effect}");

            float visRange = VisionStates[light][effect];
            LowVisibility.Logger.Log($"Naked vision range determined to be {visRange}m.");            
            return visRange;
        }
    }
}
