using BattleTech;
using BattleTech.Rendering;
using BattleTech.Rendering.Mood;
using CustAmmoCategories;
using CustomUnits;
using Harmony;
using HBS.Collections;
using IRBTModUtils.Extension;
using LowVisibility.Helper;
using System;

namespace LowVisibility.Patch
{
	[HarmonyPatch(typeof(MoodController), "ApplyMoodSettings")]
	static class MoodController_ApplyMoodSettings
	{
		static void Postfix(MoodController __instance, FogScattering ___mainFogScattering)
		{
			if (__instance == null || ___mainFogScattering == null) return;

			// Check for CU manual deploy
			if (ModState.Combat == null || ModState.Combat.ActiveContract == null) return;

			// If we're past here, safe to set
			if (MoodHelper.IsFoggy(__instance))
			{
				bool isHeavyFog = MoodHelper.IsHeavyFog(__instance);
				Mod.Log.Info?.Write($"Fog detected => isHeavy: {isHeavyFog}");

				___mainFogScattering.fogSettings = __instance.currentMood.fogSettings;
				Mod.Log.Info?.Write($"Mood settings for fog are: " +
					$" fogG: {___mainFogScattering.fogSettings.fogG}" +
					$" fogMieMultiplier: {___mainFogScattering.fogSettings.fogMieMultiplier}" +
					$" fogRayleighMultiplier: {___mainFogScattering.fogSettings.fogRayleighMultiplier}" +
					$" fogTintColor: {___mainFogScattering.fogSettings.fogTintColor}" +
					$" heightFogDensity: {___mainFogScattering.fogSettings.heightFogDensity}" +
					$" heightFogStart: {___mainFogScattering.fogSettings.heightFogStart}" +
					$" heightMieMultiplier: {___mainFogScattering.fogSettings.heightMieMultiplier}" +
					$" heightRayleighMultiplier: {___mainFogScattering.fogSettings.heightRayleighMultiplier}" +
					$" revealedIntensity: {___mainFogScattering.fogSettings.revealedIntensity}" +
					$" revealedMieMultiplier: {___mainFogScattering.fogSettings.revealedMieMultiplier}" +
					$" surveyedIntensity: {___mainFogScattering.fogSettings.surveyedIntensity}" +
					$" surveyedMieMultiplier: {___mainFogScattering.fogSettings.surveyedMieMultiplier}"
					);

				if (!LanceSpawnerGameLogic_OnEnterActive.isObjectivesReady(ModState.Combat.ActiveContract))
				{
					Mod.Log.Info?.Write("Deferring mood settings until after manual deployment. Disabling fog");
					___mainFogScattering.fogSettings.heightFogDensity = 100f;
					return;
				}
				else
                {
					___mainFogScattering.fogSettings.heightFogDensity = isHeavyFog ?
						Mod.Config.Weather.HeavyFogDensity : Mod.Config.Weather.LightFogDensity;
					Mod.Log.Info?.Write($"Updated heightFogDensity to: {___mainFogScattering.fogSettings.heightFogDensity}");
				}
			}
		}
	}

	//[HarmonyPatch(typeof(AbstractActor), "DespawnActor")]
	//[HarmonyPatch(new Type[] { typeof(MessageCenterMessage) })]
	//[HarmonyAfter(new string[]{ "CustomUnits" })]
	//public static class AbstractActor_DespawnActor
	//{
	//	public static void Prefix(AbstractActor __instance, MessageCenterMessage message, ref bool __state)
	//	{
	//		try
	//		{
	//			__state = false;

	//			DespawnActorMessage despawnActorMessage = message as DespawnActorMessage;
	//			if (despawnActorMessage == null)
	//				return;

	//			if (!(despawnActorMessage.affectedObjectGuid == __instance.GUID))
	//				return;

	//			if (__instance.TeamId != __instance.Combat.LocalPlayerTeamGuid)
	//				return;

	//			if (__instance.IsDeployDirector())
	//			{
	//				Mod.Log.Info?.Write($"Detected DeployDirector: {__instance.DistinctId()}, need to apply mood.");
	//				__state = true;
	//			}
	//		}
	//		catch (Exception e)
 //           {
	//			Mod.Log.Warn?.Write(e, $"Failed during despawn check for actor: {__instance.DistinctId()}");
 //           }
				
	//	}

	//	public static void Postfix(AbstractActor __instance, bool __state)
	//	{
	//		if (__state == true)
 //           {
	//			if (ModState.GetMoodController() == null)
	//				Mod.Log.Error?.Write("Mood controller was null when attempting to update after manual deploy!");

	//			Mod.Log.Info?.Write("Applying MoodController logic now that manual deploy is done.");
	//			Traverse applyMoodSettingsT = Traverse.Create(ModState.GetMoodController())
	//				.Method("ApplyMoodSettings", new object[] { true, false });
	//			applyMoodSettingsT.GetValue();
 //           }

 //       }
	//}

}
