using BepInEx;
using BepInEx.Configuration;
using UnityEngine.SceneManagement;

namespace RisingSlash.FP2Mods.ParametersSet
{
    [BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
    [BepInProcess("FP2.exe")]
    public class ParametersSet : BaseUnityPlugin
    {
        private ConfigEntry<string> configGreeting;
        private ConfigEntry<bool> configDisplayGreeting;
        
        private ConfigEntry<string> configLastKnownScene;
        
        private string activeSceneName = "";
        private string previousSceneName = "";
        
        private void Awake()
        {
            // ParametersSet startup logic
            InitConfigs();
            
            Logger.LogInfo($"PluginParametersSet {MyPluginInfo.PLUGIN_GUID} is loaded!");
            Logger.LogInfo($"The value of {configGreeting.Definition.Key} in config file is {configGreeting.Value}.");
        }

        private void InitConfigs()
        {
            configGreeting = Config.Bind("General",      // The section under which the option is shown
                "GreetingText",  // The key of the configuration option in the configuration file
                "Hello, world!", // The default value
                "A greeting text to show when the game is launched"); // Description of the option to show in the config file

            configDisplayGreeting = Config.Bind("General.Toggles", 
                "DisplayGreeting",
                true,
                "Whether or not to show the greeting text");
            
            configLastKnownScene = Config.Bind("General",      // The section under which the option is shown
                "LastKnownScene",  // The key of the configuration option in the configuration file
                "Zao Land", // The default value
                "The scene name of the last scene that was active in-game before closing."); // Description of the option to show in the config file
        }

        private void Update()
        {
            previousSceneName = activeSceneName;
            activeSceneName = SceneManager.GetActiveScene().name;
            if (!activeSceneName.Equals(previousSceneName))
            {
                Logger.LogInfo($"Scene name changed from {previousSceneName} to {activeSceneName}.");
                Logger.LogInfo($"Saving last scene name to config file.");

                configLastKnownScene.Value = activeSceneName;
            }
        }
    }
}
