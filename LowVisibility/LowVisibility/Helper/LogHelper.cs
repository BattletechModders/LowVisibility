using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Harmony;
using HBS.Logging;
using us.frostraptor.modUtils.logging;

namespace LowVisibility
{
    public static class LogHelper
    {
        public delegate void Log(string message);

        public static Log IntraModLog =
            (Log)Delegate.CreateDelegate(typeof(Log), Mod.Log, typeof(IntraModLogger).GetMethod("Log", AccessTools.all));

        public static Log IntraModWarn =
            (Log)Delegate.CreateDelegate(typeof(Log), Mod.Log, typeof(IntraModLogger).GetMethod("Warn", AccessTools.all));

         public static ILog HBSLogger =
            (ILog)typeof(IntraModLogger).GetField(nameof(HBSLogger), AccessTools.all).GetValue(Mod.Log);

        private static readonly bool isDebug = (bool)typeof(IntraModLogger).GetField("IsDebug", AccessTools.all).GetValue(Mod.Log);

        private static readonly bool isTrace = (bool)typeof(IntraModLogger).GetField("IsTrace", AccessTools.all).GetValue(Mod.Log);

        public static Log Debug(this IntraModLogger logger)
        {
            if (!isDebug)
                return null;

            return IntraModLog;
        }

        public static Log Trace(this IntraModLogger logger)
        {
            if (!isTrace)
                return null;

            return IntraModLog;
        }

        public static Log Warn(this IntraModLogger logger)
        {
            if (HBSLogger.IsWarningEnabled)
                return IntraModWarn;

            return null;
        }
    }
}
