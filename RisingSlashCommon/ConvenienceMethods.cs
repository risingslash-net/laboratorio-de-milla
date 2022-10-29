using System;
using BepInEx.Logging;

namespace RisingSlash.FP2Mods.RisingSlashCommon;

public static class ConvenienceMethods
{
    public static ManualLogSource LocalLog;
    
    public static void LogExceptionError(Exception e)
    {
        LocalLog.LogError($"{MyPluginInfo.PLUGIN_GUID} Is logging an exception:\r\n{e.Message}\r\n{e.StackTrace}\r\n");
    }
    
    public static void Log(string str)
    {
        LocalLog.LogInfo($"{MyPluginInfo.PLUGIN_GUID} {str}");
    }
    
    public static void Log(string plugin_guid, string str)
    {
        LocalLog.LogInfo($"{plugin_guid} {str}");
    }
}