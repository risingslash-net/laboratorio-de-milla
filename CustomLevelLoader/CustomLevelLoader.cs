using System;
using System.Collections.Generic;
using System.IO;
using BepInEx;
using BepInEx.Configuration;
using RisingSlash.FP2Mods.RisingSlashCommon;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace RisingSlash.FP2Mods.CustomLevelLoader
{
    [BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
    [BepInProcess("FP2.exe")]
    public class CustomLevelLoader : BaseUnityPlugin
    {
        private ConfigEntry<string> configBootupLevel;
        private ConfigEntry<int> configSaveFileNumber;
        private ConfigEntry<bool> configShowTransitionWipe;
        private ConfigEntry<string> configAssetBundlePaths;
        
        private ConfigEntry<bool> configUseDonorScene;

        private bool firstUpdate = false;
        
        public static List<AssetBundle> loadedAssetBundles;
        
        public static List<SceneNamePair> availableScenes;

        public static Dictionary<string, GameObject> dictObjectPrefabs;

        private void Awake()
        {
            Logger.LogInfo($"{MyPluginInfo.PLUGIN_GUID} is loaded! Initializing configs.");
            InitConfigs();
            
            loadedAssetBundles = new List<AssetBundle>();
        }

        private void OnFirstUpdate()
        { 
            // RisingSlashCommon startup logic
            try
            {
                if (configUseDonorScene.Value && dictObjectPrefabs == null)
                {
                    PollForObjectsToCache();
                }
                else
                { 
                    AutoStartLevelFromAssetBundles();  
                }
            } 
            catch (Exception e)
            {
                ConvenienceMethods.LogExceptionError(e);
            }
        }

        public static void PollForObjectsToCache()
        {
            
        }

        public void AutoStartLevelFromAssetBundles()
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

            LoadAllAssetBundles();
            EnumerateLoadedScenes();

            string level = configBootupLevel.Value;
            if (level != null && level.Equals("_"))
            {
                // Attempt to load the last scene loaded from asset bundles.
                Logger.LogInfo($"Attempting to jump to a custom level.");
                LoadSceneAtScenePairIndex(-1);
            }
            else if (level != null && (!level.Equals("") && !level.Equals("MainMenu")))
            {
                Logger.LogInfo($"Attempting to jump to BootupLevel: \"{configBootupLevel.Value}\" ");
                Logger.LogInfo($"Pre Boot Immediate");
                if (configShowTransitionWipe.Value)
                {
                    BootLevel(configBootupLevel.Value);
                }
                else
                {
                    BootLevelImmediate(configBootupLevel.Value);
                }

                Logger.LogInfo($"Post Boot Immediate");
            }
            else
            {
                Logger.LogInfo($"Attempting to jump to BootupLevel: \"{configBootupLevel.Value}\" ");
                Logger.LogInfo($"Pre Boot Main No Logos");
                GoToMainMenuNoLogos();
                Logger.LogInfo($"Post Boot Main No Logos");
            }
        }

        public void LoadAllAssetBundles()
        {
            List<DirectoryInfo> assetBundlePaths = GetAssetBundleFolders();
            foreach (var abp_di in assetBundlePaths)
            {
                LoadAssetBundlesFromDirectory(abp_di);
            }
        }
        
        public void LoadAssetBundlesFromDirectory(DirectoryInfo di)
        {
            foreach (var abp in Directory.GetFiles(di.FullName))
            {
                Logger.LogInfo($"Checking for AssetBundles at {di.FullName}");
                LoadAssetBundleFromPath(abp);
            }
        }

        private void LoadAssetBundleFromPath(string abp)
        {
            try
            {
                Logger.LogInfo(abp);
                var currentAB = AssetBundle.LoadFromFile(abp);

                if (currentAB == null)
                {
                    Logger.LogInfo("Failed to load AssetBundle. Bundle may already be loaded, might not be an AssetBundle, or the file may be corrupt.");
                    return;
                }

                //currentAB.LoadAllAssets(); //Uncomment if the scenes are still unloadable?
                loadedAssetBundles.Add(currentAB);
                Logger.LogInfo("AssetBundle loaded successfully as loadedAssetBundles[" + (loadedAssetBundles.Count - 1) +
                               "]:");
                Logger.LogInfo("--------");
                Logger.LogInfo(currentAB.GetAllScenePaths().ToString());
            }
            catch (Exception e)
            {
                Logger.LogInfo("Null reference exception when trying to load asset bundles for modding. Canceling.");
                Logger.LogInfo(e.StackTrace);
            }
        }

        public void EnumerateLoadedScenes()
        {
            //Logger.LogInfo("Level Select");

            availableScenes = new List<SceneNamePair>();
            var i = 0;
            for (i = 0; i < SceneManager.sceneCountInBuildSettings; i++)
            {
                var sceneName =
                    Path.GetFileNameWithoutExtension(SceneUtility
                        .GetScenePathByBuildIndex(i));
                availableScenes.Add(new SceneNamePair(SceneManager.GetSceneByBuildIndex(i), sceneName));
            }

            for (i = 0; i < loadedAssetBundles.Count; i++)
                foreach (var scenePath in loadedAssetBundles[i].GetAllScenePaths())
                {
                    var sceneName = Path.GetFileNameWithoutExtension(scenePath);
                    availableScenes.Add(new SceneNamePair(SceneManager.GetSceneByPath(scenePath), sceneName,
                        scenePath));
                }

            for (i = 0; i < availableScenes.Count; i++) Logger.LogInfo(i + " | " + availableScenes[i].name);

            //ShowLevelSelect(availableScenes);
        }

        public List<DirectoryInfo> GetAssetBundleFolders()
        {
            List<DirectoryInfo> assetBundlePaths = new List<DirectoryInfo>();
            
            string fp2BasePath = Directory.GetParent(Application.dataPath).FullName;
            string pathToAdd = "";
            var dirExists = false;

            var tempPathStrings = configAssetBundlePaths.Value.Trim().Trim(';').Split(';'); 
            foreach (var path in tempPathStrings)
            {
                try
                {
                    if (path.Contains(":"))
                    {
                        // Assume it's absolute
                        pathToAdd = path.Trim('*');
                    }
                    else
                    {
                        // Assume relative to FP2 dir.
                        pathToAdd = Path.Combine(fp2BasePath, path.Trim('*'));
                    }

                    dirExists = Directory.Exists(pathToAdd);
                    Logger.LogInfo($"Exists? ({(dirExists ? "Y":"N")}): {pathToAdd}");
                    if (dirExists)
                    {
                        assetBundlePaths.Add(new DirectoryInfo(pathToAdd));
                        
                        if (path.EndsWith("*"))
                        {
                            var subdirs = Directory.GetDirectories(pathToAdd);
                            foreach (var p in subdirs)
                            {
                                assetBundlePaths.Add(new DirectoryInfo(p));
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    // Should probably actually have a useful message here.
                    Logger.LogError("something freaking BLEW UP when trying to find the asset bundles??? Oops!!");
                    Logger.LogError(e.StackTrace);
                }
            }

            return assetBundlePaths;
        }

        

        public void LoadSceneAtScenePairIndex(int scenePairIndex)
        {
            if (scenePairIndex < 0)
            {
                if (availableScenes != null)
                {
                    scenePairIndex = availableScenes.Count - 1;
                }
            }

            Logger.LogInfo($"Loading scene at index {scenePairIndex}: ");
            if (availableScenes != null)
            {
                var sceneToLoad = availableScenes[scenePairIndex];
                Logger.LogInfo(sceneToLoad.name);
                SceneManager.LoadScene(sceneToLoad.name);
            }
            else
            {
                Logger.LogInfo("...But there don't seem to be any scene info saved yet??? Were there errors during loading?");
            }
        }

        public void Update()
        {
            if (!firstUpdate)
            {
                OnFirstUpdate();
                firstUpdate = true;
            }
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
            else
            {
                ConvenienceMethods.Log(MyPluginInfo.PLUGIN_GUID, "Failed to get reference to FPScreenTransition. Will not do level change.");
            }
        }

        public static void BootLevelImmediate(string level)
        {
            SceneManager.LoadSceneAsync(level);
        }
        
        public void PerformStageTransition()
        {
            try
            {
                var component = GameObject.Find("Screen Transition").GetComponent<FPScreenTransition>();
                component.transitionType = FPTransitionTypes.LOCAL_WIPE;
                component.transitionSpeed = 48f;
                component.SetTransitionColor(0f, 0f, 0f);
                component.BeginTransition();
                FPAudio.PlayMenuSfx(3);
            }
            catch (Exception e)
            {
                ConvenienceMethods.LogExceptionError(e);
            }
        }
        
        private void InitConfigs()
        {
            Logger.LogInfo($"{MyPluginInfo.PLUGIN_GUID} InitConfigs");
            configBootupLevel = Config.Bind("General",      // The section under which the option is shown
                "BootupLevel",  // The key of the configuration option in the configuration file
                "_", // The default value
                "The level you want to boot to. This corresponds to the Scene Name, not the in-game Level Name. Try BakunawaBoss1 or Zao Land for example. Leave this as \"_\" if you want load the last-loaded scene from the asset bundles. You can even boot to MainMenu, AdventureMenu, or ClassicMenu"); // Description of the option to show in the config file
            
            configSaveFileNumber = Config.Bind("General",      // The section under which the option is shown
                "SaveNumber",  // The key of the configuration option in the configuration file
                1, // The default value
                "The number of the save file you wish to use when quick-booting. Set this value to 0 to boot into a level with debug settings (no save file). Normally, this would be files 1-10, but can be longer if you have additional saves (from modding or otherwise). If you need to see your save files, they are stored at \"<AppData>\\LocalLow\\GalaxyTrail\\Freedom Planet 2\"."); // Description of the option to show in the config file
            
            configShowTransitionWipe = Config.Bind("General",      // The section under which the option is shown
                "ShowTransitionWipe",  // The key of the configuration option in the configuration file
                false, // The default value
                "Showing the black wipe transition looks cleaner and is more likely to work with built-in levels. Disable this when loading scenes from asset bundles."); // Description of the option to show in the config file
            
            configAssetBundlePaths = Config.Bind("General",      // The section under which the option is shown
                "AssetBundlePaths",  // The key of the configuration option in the configuration file
                @"mod_overrides/*;assetbundles/*;asset_bundles/*;BepInEx\plugins\custom_level_loader\assetbundles\*", // The default value
                "Paths to search for asset bundles to load. If you don't know how to use this, don't touch it. Assumes the location of the FP2 executable as the base folder. Use an asterisk at the end of a path to indicate single depth use of all folders in the folder. Separate with semi-colons."); // Description of the option to show in the config file
            
            configUseDonorScene = Config.Bind("General",      // The section under which the option is shown
                "UseDonorScene",  // The key of the configuration option in the configuration file
                true, // The default value
                "Toggles whether or not to load a donor scene to grab object instances from before loading the custom scenes."); // Description of the option to show in the config file

        }
    }
}
