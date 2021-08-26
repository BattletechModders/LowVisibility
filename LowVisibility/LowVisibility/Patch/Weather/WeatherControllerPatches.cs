using BattleTech.Rendering.Mood;
using Harmony;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using static BattleTech.Rendering.Mood.WeatherController;

namespace LowVisibility
{
    [HarmonyPatch(typeof(WeatherController), "UpdateWeather")]
    static class WeatherController_UpdateWeather
    {
        static void Postfix(WeatherController __instance)
        {
			if (__instance == null) return;

			switch (__instance.weatherSettings.weatherEffect)
			{
				case WeatherEffect.Rain:
					Mod.Log.Info?.Write($"Rain detected, current intensity: {__instance.weatherSettings.weatherEffectIntensity} newSetting: {1.0f}");
					Shader.SetGlobalFloat("_WeatherAmount", 1.0f);
					break;
				case WeatherEffect.Snow:
					Mod.Log.Info?.Write($"Snow detected, current intensity: {__instance.weatherSettings.weatherEffectIntensity} newSetting: {1.0f}");
					Shader.SetGlobalFloat("_WeatherAmount", 1.0f);
					break;
				default:
					break;
			}
		}
    }
}
