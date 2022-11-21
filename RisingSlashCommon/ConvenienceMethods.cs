using System;
using BepInEx.Logging;
using UnityEngine;

namespace RisingSlash.FP2Mods.RisingSlashCommon;

public static class ConvenienceMethods
{
    public static ManualLogSource LocalLog;
    public static bool bHasSceneChanged = false;
    
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
    
    public static void LogWarning(string plugin_guid, string str)
    {
        InitLogIfNecessary();
        LocalLog.LogWarning($"{plugin_guid} {str}");
    }
    
    public static void LogWarning(string str)
    {
        InitLogIfNecessary();
        LocalLog.LogWarning($"{MyPluginInfo.PLUGIN_GUID} {str}");
    }

    public static void InitLogIfNecessary()
    {
        if (LocalLog != null)
        {
            return;
        }

        LocalLog = new ManualLogSource("RisingSlashCommon");
    }

    public static bool HasSceneChanged()
    {
        return bHasSceneChanged;
    }
    
    public static void ShowPressedButtons()
    {
        foreach (KeyCode kcode in Enum.GetValues(typeof(KeyCode)))
        {
            if (Input.GetKeyDown(kcode))
                ConvenienceMethods.Log("KeyCode down: " + kcode);
        }
    }
}