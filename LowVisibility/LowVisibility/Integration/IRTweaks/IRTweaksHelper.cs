using System.Diagnostics;

namespace LowVisibility.Integration.IRTweaks
{
    public static class IRTweaksHelper
    {
        public static void LogIfEnabled(string message)
        {
            if (Mod.Log.IsTrace) Mod.Log.Trace?.Write($"[IRTweaks-CombatLog][{GetMethod()}] {message}");
            else if (Mod.Config.Integrations.IRTweaks.EnableLogging) Mod.Log.Info?.Write($"[IRTweaks-CombatLog][{GetMethod()}] {message}");
        }

        private static string GetMethod()
        {
            StackFrame stackFrame = new StackFrame(2);
            return $"{stackFrame.GetMethod().DeclaringType.Name}-{stackFrame.GetMethod().Name}";
        }
    }
}
