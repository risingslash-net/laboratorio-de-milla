using BepInEx;

namespace RisingSlash.FP2Mods.RisingSlashCommon
{
    [BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
    [BepInProcess("FP2.exe")]
    public class RisingSlashCommon : BaseUnityPlugin
    {
        //private ConfigEntry<string> configGreeting;
        //private ConfigEntry<bool> configDisplayGreeting;
        
        
        private void Awake()
        {
            // RisingSlashCommon startup logic
            Logger.LogInfo($"RisingSlashCommon {MyPluginInfo.PLUGIN_GUID} is loaded!");
        }
    }
}
