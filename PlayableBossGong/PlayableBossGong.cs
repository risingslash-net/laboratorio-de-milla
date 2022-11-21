using System;
using BepInEx;
using BepInEx.Configuration;
using RisingSlash.FP2Mods.RisingSlashCommon;
using UnityEngine;

namespace RisingSlash.FP2Mods.PlayableBossGong
{
    [BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
    [BepInProcess("FP2.exe")]
    public class PlayableBossGong : BaseUnityPlugin
    {
        public static ConfigEntry<string> SpawnPowerup;
        public static bool hotkeysLoaded = false;

        public static FPPlayer currentPlayer = null;
        public static ItemFuel itemFuelReference = null;
        private void Awake()
        {
            // RisingSlashCommon startup logic
            Logger.LogInfo($"{MyPluginInfo.PLUGIN_GUID} is loaded! Initializing configs.");
            try
            {
                InitConfigs();
            } 
            catch (Exception e)
            {
                LogExceptionError(e);
            }
        }

        public void Update()
        {
            if (!hotkeysLoaded)
            {
                return;
            }

            if (CustomControls.GetButtonDown(SpawnPowerup))
            {
                Logger.LogInfo($"{MyPluginInfo.PLUGIN_GUID} Hotkey pressed. Attempting to spawn powerup.");
                try
                {
                    Vector2 powerupOffset = Vector2.zero; // Maybe should scrape the values for this from the player?
                    
                    currentPlayer = FPStage.currentStage.GetPlayerInstance_FPPlayer();
                    // This flag indicates if the object was given a valid index position in the stage's list of FP objects.
                    // The object will have all kinds of strange behaviors if we don't make sure it gets set properly.
                    bool powerupObjectValidated = false;
                    GameObject powerupInstance = FPStage.InstantiateFPBaseObject(GetItemFuelReference().gameObject, out powerupObjectValidated);
                    Logger.LogInfo($"{MyPluginInfo.PLUGIN_GUID} Spawned.");
                    
                    // This step is easy to forget. Don't.
                    FPStage.ValidateStageListPos(GetItemFuelReference());
                    
                    powerupInstance.transform.position = currentPlayer.transform.position + new Vector3(powerupOffset.x, powerupOffset.y, 0f);
                }
                catch (Exception e)
                {
                    ConvenienceMethods.LogExceptionError(e);
                }
            }
        }

        private void LogExceptionError(Exception e)
        {
            Logger.LogError($"{MyPluginInfo.PLUGIN_GUID} Threw an exception:\r\n{e.Message}\r\n{e.StackTrace}\r\n");
        }

        public ItemFuel GetItemFuelReference()
        {
            // If we already have a live reference, returning it is much faster than searching the scene for one.
            /*
            if (itemFuelReference != null)
            {
                return itemFuelReference;
            }
            */
            
            // We don't care where the fuel/powerup item is, we just want one to copy as fast as possible.
            itemFuelReference = GameObject.FindObjectOfType<ItemFuel>();
            
            // Somehow we couldn't find a reference in the scene, but we know the PlayerSpawnPoint should have one if it exists.
            // Slow to double-try this. Consider removing?
            if (itemFuelReference == null)
            {
                itemFuelReference = GameObject.FindObjectOfType<PlayerSpawnPoint>().powerup;
            }

            return itemFuelReference;
        }

        private void InitConfigs()
        {
            Logger.LogInfo($"{MyPluginInfo.PLUGIN_GUID} InitConfigs");
            InitConfigsCustomHotkeys();
        }

        private void InitConfigsCustomHotkeys()
        {
            Logger.LogInfo($"{MyPluginInfo.PLUGIN_GUID} InitConfigsCustomHotkeys");
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
            Logger.LogInfo($"{MyPluginInfo.PLUGIN_GUID} Hotkeys loaded.");
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
