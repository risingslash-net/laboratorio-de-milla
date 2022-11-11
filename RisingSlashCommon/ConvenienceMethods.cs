using System;
using BepInEx.Logging;

namespace RisingSlash.FP2Mods.RisingSlashCommon;

public static class ConvenienceMethods
{
    public static ManualLogSource LocalLog;
    
    public static void LogExceptionError(Exception e)
    {
        InitLogIfNecessary();
        LocalLog.LogError($"{MyPluginInfo.PLUGIN_GUID} Is logging an exception:\r\n{e.Message}\r\n{e.StackTrace}\r\n");
    }
    
    public static void Log(string str)
    {
        InitLogIfNecessary();
        LocalLog.LogInfo($"{MyPluginInfo.PLUGIN_GUID} {str}");
    }
    
    public static void Log(string plugin_guid, string str)
    {
        InitLogIfNecessary();
        LocalLog.LogInfo($"{plugin_guid} {str}");
    }

    public static void InitLogIfNecessary()
    {
        if (LocalLog != null)
        {
            return;
        }

        LocalLog = new ManualLogSource("RisingSlashCommon");
    }
}