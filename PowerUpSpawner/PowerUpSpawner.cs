using BepInEx;
using BepInEx.Configuration;
using RisingSlash.FP2Mods.RisingSlashCommon;
using UnityEngine;

namespace RisingSlash.FP2Mods.PowerUpSpawner
{
    [BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
    [BepInProcess("FP2.exe")]
    public class PowerUpSpawner : BaseUnityPlugin
    {
        public static ConfigEntry<string> SpawnPowerup;
        public static bool hotkeysLoaded = false;
        private void Awake()
        {
            // RisingSlashCommon startup logic
            Logger.LogInfo($"RisingSlashCommon {MyPluginInfo.PLUGIN_GUID} is loaded! Initializing configs.");
            InitConfigs();
        }
        
        private void InitConfigs()
        {
            InitConfigsCustomHotkeys();
        }

        private void InitConfigsCustomHotkeys()
        {
            SpawnPowerup = CreateEntryAndBindHotkey("SpawnPowerup", "Backspace");
            //KeyCode.Backspace

            /*
            PHKToggleInstructions = CreateEntryAndBindHotkey("PHKToggleInstructions", "F1");

            PHKSetWarpPoint = CreateEntryAndBindHotkey("PHKSetWarpPoint", "Shift+F4");
            PHKGotoWarpPoint = CreateEntryAndBindHotkey("PHKGotoWarpPoint", "F4");

            PHKKOCharacter = CreateEntryAndBindHotkey("PHKKOCharacter", "Shift+F1");
            PHKKOBoss = CreateEntryAndBindHotkey("PHKKOBoss", "Backspace");
            PHKInvincibleBoss = CreateEntryAndBindHotkey("PHKInvincibleBoss", "Shift+Backspace");
            PHKInvinciblePlayers = CreateEntryAndBindHotkey("PHKInvinciblePlayers", "Ctrl+Backspace");

            PHKToggleNoClip = CreateEntryAndBindHotkey("PHKToggleNoClip", "F2");

            PHKSpawnExtraChar = CreateEntryAndBindHotkey("PHKSpawnExtraChar", "F12");
            PHKSwapBetweenSpawnedChars = CreateEntryAndBindHotkey("PHKSwapBetweenSpawnedChars", "F11");
            PHKToggleMultiCharStart = CreateEntryAndBindHotkey("PHKToggleMultiCharStart", "Shift+F12");
            PHKCyclePreferredAllyControlType =
                CreateEntryAndBindHotkey("F", "Shift+F11");
            PHKStartInputPlayback =
                CreateEntryAndBindHotkey("PHKStartInputPlayback", "Insert");
            PHKToggleLockP1ToGhostFiles =
                CreateEntryAndBindHotkey("PHKToggleLockP1ToGhostFiles", "Shift+Insert");
            
            PHKStartSplitscreen = CreateEntryAndBindHotkey("PHKStartSplitscreen", "Slash");
            
            PHKSwitchCurrentPlayerToLilac = CreateEntryAndBindHotkey("PHKSwitchCurrentPlayerToLilac", "Alpha0");
            PHKSwitchCurrentPlayerToCarol = CreateEntryAndBindHotkey("PHKSwitchCurrentPlayerToCarol", "Alpha1");
            PHKSwitchCurrentPlayerToCarolBike = CreateEntryAndBindHotkey("PHKSwitchCurrentPlayerToCarolBike", "Alpha2");
            PHKSwitchCurrentPlayerToMilla = CreateEntryAndBindHotkey("PHKSwitchCurrentPlayerToMilla", "Alpha3");
            PHKSwitchCurrentPlayerToNeera = CreateEntryAndBindHotkey("PHKSwitchCurrentPlayerToNeera", "Alpha4");
            PHKSwitchCurrentPlayerToNext = CreateEntryAndBindHotkey("PHKSwitchCurrentPlayerToNext", "Alpha9");
            PHKSwitchCurrentPlayerToPrev = CreateEntryAndBindHotkey("PHKSwitchCurrentPlayerToPrev", "Alpha8");

            PHKGetOutGetOutGetOut = CreateEntryAndBindHotkey("PHKGetOutGetOutGetOut", "Delete");

            PHKCameraZoomIn = CreateEntryAndBindHotkey("PHKCameraZoomIn", "Plus");
            PHKCameraZoomOut = CreateEntryAndBindHotkey("PHKCameraZoomOut", "Minus");
            PHKCameraZoomReset = CreateEntryAndBindHotkey("PHKCameraZoomReset", "Period");

            PHKShowNextDataPage = CreateEntryAndBindHotkey("PHKShowNextDataPage", "PageDown");
            PHKShowPreviousDataPage = CreateEntryAndBindHotkey("PHKShowPreviousDataPage", "PageUp");
            PHKHideDataView = CreateEntryAndBindHotkey("PHKHideDataView", "Backslash");

            PHKGoToMainMenu = CreateEntryAndBindHotkey("PHKGoToMainMenu", "F7");
            PHKLoadDebugRoom = CreateEntryAndBindHotkey("PHKLoadDebugRoom", "F8");

            PHKGoToLevelSelectMenu = CreateEntryAndBindHotkey("PHKGoToLevelSelectMenu", "F9");

            PHKLoadAssetBundles = CreateEntryAndBindHotkey("PHKLoadAssetBundles", "Shift+F9");
            //PHKTogglePauseMenuOrTrainerMenu = CreateEntryAndBindHotkey("PHKTogglePauseMenuOrTrainerMenu", "F1");
            PHKGoToLevelAtLastIndex = CreateEntryAndBindHotkey("PHKGoToLevelAtLastIndex", "BackQuote");
            PHKIncreaseFontSize = CreateEntryAndBindHotkey("PHKIncreaseFontSize", "Shift+Plus");
            PHKDecreaseFontSize = CreateEntryAndBindHotkey("PHKDecreaseFontSize", "Shift+Minus");

            PHKReturnToCheckpoint = CreateEntryAndBindHotkey("PHKReturnToCheckpoint", "R");
            PHKRestartStage = CreateEntryAndBindHotkey("PHKRestartStage", "Shift+R");

            PHKTogglePlaneSwitcherVisualizers = CreateEntryAndBindHotkey("PHKTogglePlaneSwitcherVisualizers", "F3");
            PHKToggleShowColliders = CreateEntryAndBindHotkey("PHKToggleShowColliders", "Shift+F3");

            //PHKNextWarppointSaveSlot = CreateEntryAndBindHotkey("PHKNextWarppointSaveSlot", "F10");
            //PHKPrevWarppointSaveSlot = CreateEntryAndBindHotkey("PHKPrevWarppointSaveSlot", "F9");

            PHKRebindAllHotkeys = CreateEntryAndBindHotkey("PHKRebindAllHotkeys", "Pause");*/

            hotkeysLoaded = true;
        }

        public ConfigEntry<string> CreateEntryAndBindHotkey(string identifier,
            string default_value)
        {
            var configHotkey = Config.Bind("Keybinds",      // The section under which the option is shown
                identifier,  // The key of the configuration option in the configuration file
                default_value, // The default value
                $"A custom input binding for {identifier}"); // Description of the option to show in the config file
            //FP2TrainerCustomHotkeys.Add(melonPrefEntry);
            CustomControls.Add(configHotkey);
            return configHotkey;
        }
    }
}
