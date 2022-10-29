using System;
using BepInEx;
using BepInEx.Configuration;
using RisingSlash.FP2Mods.RisingSlashCommon;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace RisingSlash.FP2Mods.QuickBootToLevel
{
    [BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
    [BepInProcess("FP2.exe")]
    public class QuickBootToLevel : BaseUnityPlugin
    {
        private ConfigEntry<string> configBootupLevel;
        private ConfigEntry<int> configSaveFileNumber;

        private bool firstUpdate = false;
        private int countUpdates = 0;

        private void Awake()
        {
            Logger.LogInfo($"{MyPluginInfo.PLUGIN_GUID} is loaded! Initializing configs.");
            InitConfigs();
        }

        private void 
            OnFirstUpdate()
        {
            // RisingSlashCommon startup logic
            try
            {
                if (configSaveFileNumber.Value > 0)
                {
                    Logger.LogInfo($"Attempting to load Save File number \"{configSaveFileNumber.Value.ToString()}\" ");
                    FPSaveManager.LoadFromFile(configSaveFileNumber.Value);
                }
                else
                {
                    Logger.LogInfo("Skipping save file load for booting.");
                }

                string level = configBootupLevel.Value;
                Logger.LogInfo($"Attempting to jump to BootupLevel: \"{configBootupLevel.Value}\" ");
                if (level != null && (!level.Equals("") || level.Equals("MainMenu")))
                {
                    Logger.LogInfo($"Pre Boot Immediate");
                    //BootLevelImmediate(level);
                    BootLevel(level);
                    Logger.LogInfo($"Post Boot Immediate");
                }
                else
                {
                    Logger.LogInfo($"Pre Boot Main No Logos");
                    GoToMainMenuNoLogos();
                    Logger.LogInfo($"Post Boot Main No Logos");
                }
            } 
            catch (Exception e)
            {
                ConvenienceMethods.LogExceptionError(e);
            }
        }

        public void Update()
        {
            if (!firstUpdate)
            {
                OnFirstUpdate();
                firstUpdate = true;
            }

            if (countUpdates % 180 == 0)
            {
                OnFirstUpdate();
            }

            countUpdates++;
        }

        public static void GoToMainMenuNoLogos()
        {
            BootLevel("MainMenu");
        }
        
        public static void BootLevel(string level)
        {
            var component = GameObject.Find("Screen Transition").GetComponent<FPScreenTransition>();
            if (component != null)
            {
                component.transitionType = FPTransitionTypes.WIPE;
                component.transitionSpeed = 48f;
                component.sceneToLoad = level;
                FPSaveManager.menuToLoad = 2; // This is how we skip the intros.
            }
        }

        public static void BootLevelImmediate(string level)
        {
            SceneManager.LoadSceneAsync(level);
        }
        
        private void InitConfigs()
        {
            Logger.LogInfo($"{MyPluginInfo.PLUGIN_GUID} InitConfigs");
            configBootupLevel = Config.Bind("General",      // The section under which the option is shown
                "BootupLevel",  // The key of the configuration option in the configuration file
                "MainMenu", // The default value
                "The level you want to boot to. This corresponds to the Scene Name, not the in-game Level Name. Try BakunawaBoss1 or Zao Land for example. You can even boot to AdventureMenu or ClassicMenu"); // Description of the option to show in the config file
            
            configSaveFileNumber = Config.Bind("General",      // The section under which the option is shown
                "SaveNumber",  // The key of the configuration option in the configuration file
                1, // The default value
                "The number of the save file you wish to use when quick-booting. Set this value to 0 to boot into a level with debug settings (no save file). Normally, this would be files 1-10, but can be longer if you have additional saves (from modding or otherwise). If you need to see your save files, they are stored at \"<AppData>\\LocalLow\\GalaxyTrail\\Freedom Planet 2\"."); // Description of the option to show in the config file
        }
    }
}
