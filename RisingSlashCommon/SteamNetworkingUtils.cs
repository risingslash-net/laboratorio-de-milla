using System;
using System.Reflection;

namespace RisingSlash.FP2Mods.RisingSlashCommon;
//using Steamworks;

public class SteamNetworkingUtils
{
    public static bool IsSteamManagerInitialized()
    {
        var isInit = false;
        try
        {
            // Reminder: Internal classes are internal for a reason. Messing with this is playing with fire.
            var asm = typeof(FPSaveManager).Assembly;
        
            var steamManager = asm.GetType("SteamManager");
            PropertyInfo propertyInfo = steamManager.GetType().GetProperty("Initialized", 
                BindingFlags.Public | BindingFlags.Static);
            propertyInfo.GetValue((object)steamManager, null);
        }
        catch (Exception e)
        {
            ConvenienceMethods.LogExceptionError(e);
        }

        return isInit;
    }

    public static void TestSelfSendMessage(string text)
    {
        
        
    }

}