using Harmony;
using HBS.Logging;
using System;
using System.Diagnostics;
using System.Reflection;

namespace LowVisibility.Patch {

    // TODO: DISABLE THIS
    [HarmonyPatch]
    public static class Logger_LogAtLevel {

        public static MethodInfo TargetMethod() {
            var clazz = AccessTools.Inner(typeof(HBS.Logging.Logger), "LogImpl");
            var method = AccessTools.Method(clazz, "LogAtLevel", new Type[] {
                typeof(HBS.Logging.LogLevel),
                typeof(object),
                typeof(UnityEngine.Object),
                typeof(Exception),
                typeof(HBS.Logging.IStackTrace)
            });
            return method;
        }

        public static void Postfix(HBS.Logging.Logger __instance, LogLevel level, object message) {
            string mess = message as string;
            if (mess != null && mess.Contains("Dectected")) {
                StackTrace st = new StackTrace(true);
                LowVisibility.Logger.Log($"INTERCEPTED LOG:{mess} - ST:{st}");
            }
        }

    }
}
