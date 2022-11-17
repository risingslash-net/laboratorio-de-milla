using System;
using BepInEx;
using BepInEx.Configuration;
using RisingSlash.FP2Mods.RisingSlashCommon;
using UnityEngine;

namespace RisingSlash.FP2Mods.NoClip
{
    [BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
    [BepInProcess("FP2.exe")]
    public class NoClip : BaseUnityPlugin
    {
        public static ConfigEntry<string> PHKToggleNoClip;
        public static ConfigEntry<float> NoClipMoveSpeed;
        public static bool hotkeysLoaded = false;

        public static FPPlayer fpplayer = null;

        public static TextMesh noClipTextMesh;
        
        public bool noClip = false;
        Vector3 noClipStartPos = Vector3.zero;
        public int noClipCollisionLayer = -999;
        public float noClipGravityStrength = 0;

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

            if (CustomControls.GetButtonDown(PHKToggleNoClip))
            {
                Logger.LogInfo($"{MyPluginInfo.PLUGIN_GUID} Hotkey pressed. Toggle NoClip");
                try
                {
                    //Vector2 powerupOffset = Vector2.zero; // Maybe should scrape the values for this from the player?
                    
                    //currentPlayer = FPStage.currentStage.GetPlayerInstance_FPPlayer();
                    
                    RisingSlashCommon.ConvenienceMethods.Log("NoClip Toggle");
                    ToggleNoClip();
                }
                catch (Exception e)
                {
                    ConvenienceMethods.LogExceptionError(e);
                }
            }

            if (noClip)
            {
                HandleNoClip();
            }
        }
        
        private void HandleNoClip()
        {
            //fpplayer.enablePhysics = false;

            if (noClip && fpplayer != null)
            {

                if (noClipTextMesh == null)
                {
                    noClipTextMesh = OnScreenTextUtil.CreateOnScreenText("Hello world");
                }

                if (noClipTextMesh != null)
                {
                    noClipTextMesh.text = "No Clip: Enabled";
                }

                fpplayer.collisionLayer = -999;
                fpplayer.invincibilityTime = 100f;
                fpplayer.gravityStrength = 0;
                fpplayer.hitStun = -1;

                fpplayer.velocity.x = 0;
                fpplayer.velocity.y = 0;

                float modifiedNoClipMoveSpeed = NoClipMoveSpeed.Value;
                if (InputControl.GetButton(Controls.buttons.special))
                {
                    modifiedNoClipMoveSpeed *= 4f;
                }

                fpplayer.velocity = Vector2.zero;
                if (fpplayer.input.up
                    || InputControl.GetAxis(Controls.axes.vertical) > 0.2f)
                {
                    fpplayer.position.y += modifiedNoClipMoveSpeed * 1;
                }

                if (fpplayer.input.down
                    || InputControl.GetAxis(Controls.axes.vertical) < -0.2f)
                {
                    fpplayer.position.y -= modifiedNoClipMoveSpeed * 1;
                }

                if (fpplayer.input.right
                    || InputControl.GetAxis(Controls.axes.horizontal) > 0.2f)
                {
                    fpplayer.position.x += modifiedNoClipMoveSpeed * 1;
                }

                if (fpplayer.input.left
                    || InputControl.GetAxis(Controls.axes.horizontal) < -0.2f)
                {
                    fpplayer.position.x -= modifiedNoClipMoveSpeed * 1;
                }


                if (InputControl.GetButtonDown(Controls.buttons.attack))
                {
                    EndNoClip();
                }

                if (InputControl.GetButtonDown(Controls.buttons.jump))
                {
                    EndNoClipAndReturnToStartPosition();
                }
            }
            else
            {
                if (noClipTextMesh != null)
                {
                    noClipTextMesh.text = "";
                }
            }
        }

        public void ToggleNoClip()
        {
            FetchFPPlayerIfNeeded();
            if (noClip)
            {
                EndNoClip();
            }
            else
            {
                noClip = true;
                fpplayer.terrainCollision = false;
                noClipStartPos = fpplayer.position;
                noClipCollisionLayer = fpplayer.collisionLayer;
                noClipGravityStrength = fpplayer.gravityStrength;
            }
        }
        
        public void EndNoClip()
        {
            FetchFPPlayerIfNeeded();
            fpplayer.invincibilityTime = 0f;
            fpplayer.gravityStrength = noClipGravityStrength;
            fpplayer.hitStun = 0f;
            fpplayer.collisionLayer = noClipCollisionLayer;
            fpplayer.terrainCollision = true;

            /*
            if (currentDataPage == DataPage.NO_CLIP)
            {
                currentDataPage++;
            }
            */

            noClip = false;

            //fpplayer.enablePhysics = true;
        }

        public void EndNoClipAndReturnToStartPosition()
        {
            fpplayer.position = noClipStartPos;
            EndNoClip();
        }

        public void FetchFPPlayerIfNeeded()
        {
            if (fpplayer == null)
            {
                fpplayer = FPStage.currentStage.GetPlayerInstance_FPPlayer();   
            }
        }

        private void LogExceptionError(Exception e)
        {
            Logger.LogError($"{MyPluginInfo.PLUGIN_GUID} Threw an exception:\r\n{e.Message}\r\n{e.StackTrace}\r\n");
        }

        private void InitConfigs()
        {
            Logger.LogInfo($"{MyPluginInfo.PLUGIN_GUID} InitConfigs");
            NoClipMoveSpeed = Config.Bind("NoClip Preferences", 
                "NoClipMoveSpeed",
                30f,
                "Default movement speed when in no-clip mode.");
            InitConfigsCustomHotkeys();
        }

        private void InitConfigsCustomHotkeys()
        {
            Logger.LogInfo($"{MyPluginInfo.PLUGIN_GUID} InitConfigsCustomHotkeys");
            PHKToggleNoClip = CreateEntryAndBindHotkey("PHKToggleNoClip", "F2");
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
            CustomControls.Add(configHotkey);
            return configHotkey;
        }
    }
}
