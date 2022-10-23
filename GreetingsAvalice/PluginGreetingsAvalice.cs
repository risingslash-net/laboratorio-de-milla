using BepInEx;

namespace RisingSlash.FP2Mods.GreetingsAvalice
{
    [BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
    [BepInProcess("FP2.exe")]
    public class PluginGreetingsAvalice : BaseUnityPlugin
    {
        private void Awake()
        {
            // PluginGreetingsAvalice startup logic
            Logger.LogInfo($"PluginGreetingsAvalice {MyPluginInfo.PLUGIN_GUID} is loaded!");
        }
    }
}
