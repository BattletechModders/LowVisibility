namespace LowVisibility.Helper
{
    public class NightVisionHelper
    {

        public static void EnableNightVisionMode()
        {
            Mod.Log.Info?.Write($"Enabling night vision mode.");   
            ModState.IsNightVisionMode = true;
        }

        public static void DisableNightVisionMode()
        {
            Mod.Log.Info?.Write($"Disabling night vision mode.");
            ModState.IsNightVisionMode = false;
        }
    }
}
