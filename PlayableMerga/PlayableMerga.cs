using System;
using System.Collections.Generic;
using System.Reflection;
using BepInEx;
using BepInEx.Configuration;
using RisingSlash.FP2Mods.RisingSlashCommon;
using UnityEngine;
using UnityEngineInternal;
using Random = System.Random;

namespace RisingSlash.FP2Mods.PlayableMerga
{
    [BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
    [BepInProcess("FP2.exe")]
    public class PlayableMerga : BaseUnityPlugin
    {
        public static ConfigEntry<string> PHKAttack1;
        public static ConfigEntry<string> PHKAttack2;
        public static ConfigEntry<string> PHKAttack3;
        public static ConfigEntry<string> PHKAttack4;
        public static ConfigEntry<string> PHKSpawnRandomBosses;
        public static ConfigEntry<string> PHKMoveLeft;
        public static ConfigEntry<string> PHKMoveRight;
        public static ConfigEntry<string> PHKJump;
        public static bool hotkeysLoaded = false;

        public static FPPlayer currentPlayer = null;
        public static ItemFuel itemFuelReference = null;

        public static PlayerBossMerga[] pbMerga;
        public static FPBossHud[] bossHuds;

        public static List<GameObject> BossInstanceCache;
        public static List<GameObject> BossInstanceHealthBarCache;
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
            if (ConvenienceMethods.HasSceneChanged())
            {
                RefreshMergaIfNeeded();
                RefreshHealthbarsIfNeeded();
            }

            if (!hotkeysLoaded)
            {
                return;
            }

            if (CustomControls.GetButtonDown(PHKAttack1))
            {
                Logger.LogInfo($"{MyPluginInfo.PLUGIN_GUID} Hotkey pressed. Attempting to do a State_RunAttack.");
                try
                {
                    RefreshMergaIfNeeded();
                    if (pbMerga != null) {ConvenienceMethods.Log($"Mergas: {pbMerga.Length}");}
                    
                    foreach (var merga in pbMerga)
                    {
                        MethodInfo st_run_attack = merga.GetType().GetMethod("State_RunAttack", 
                            BindingFlags.NonPublic | BindingFlags.Instance);
                        //dynMethod.Invoke(this, new object[] { methodParams });
                        FPObjectState d_st_run_attack;
                        d_st_run_attack = (FPObjectState) st_run_attack.CreateDelegate(typeof(FPObjectState), merga);
                        
                        merga.ResetAllVars();
                        //merga.state = new FPObjectState(merga.State_RunAttack);
                        //merga.state = new FPObjectState(st_run_attack);
                        merga.state = d_st_run_attack;
                    }

                    currentPlayer = FPStage.currentStage.GetPlayerInstance_FPPlayer();
                }
                catch (Exception e)
                {
                    ConvenienceMethods.LogExceptionError(e);
                }
            }
            
            if (CustomControls.GetButtonDown(PHKAttack2))
            {
                Logger.LogInfo($"{MyPluginInfo.PLUGIN_GUID} Hotkey pressed. Attempting State_GroundSlashCombo");
                try
                {
                    RefreshMergaIfNeeded();
                    if (pbMerga != null) {ConvenienceMethods.Log($"Mergas: {pbMerga.Length}");}
                    foreach (var merga in pbMerga)
                    {
                        MethodInfo st_run_attack = merga.GetType().GetMethod("State_GroundSlashCombo", 
                            BindingFlags.NonPublic | BindingFlags.Instance);
                        //dynMethod.Invoke(this, new object[] { methodParams });
                        FPObjectState d_st_run_attack;
                        d_st_run_attack = (FPObjectState) st_run_attack.CreateDelegate(typeof(FPObjectState), merga);
                        
                        merga.ResetAllVars();
                        //merga.state = new FPObjectState(merga.State_RunAttack);
                        //merga.state = new FPObjectState(st_run_attack);
                        merga.state = d_st_run_attack;
                        
                        //merga.ResetAllVars();
                        //merga.state = new FPObjectState(merga.State_GroundSlashCombo);
                        //++merga.subPhase;
                    }

                    currentPlayer = FPStage.currentStage.GetPlayerInstance_FPPlayer();
                }
                catch (Exception e)
                {
                    ConvenienceMethods.LogExceptionError(e);
                }
            }
            
            
            try
            {
                RefreshMergaIfNeeded();
                
                foreach (var merga in pbMerga)
                {
                    if (CustomControls.GetButton(PHKMoveLeft))
                    {
                        RunLeft(merga);
                    }
                    else if (CustomControls.GetButton(PHKMoveRight))
                    {
                        RunRight(merga);
                    }

                    if (CustomControls.GetButtonDown(PHKJump))
                    {
                        Act_Jump(merga);
                    }
                    if (CustomControls.GetButtonUp(PHKJump))
                    {
                        Act_FastFall(merga);
                    }
                    Handle360Movement(merga);
                }
                
                
                
            }
            catch (Exception e)
            {
                ConvenienceMethods.LogExceptionError(e);
            }
        }
        

        public static void RefreshMergaIfNeeded()
        {
            if (pbMerga == null || pbMerga.Length < 1)
            {
                pbMerga = GameObject.FindObjectsOfType<PlayerBossMerga>();
                if (pbMerga.Length > 0 &&  FPCamera.stageCamera != null)
                {
                    FPCamera.stageCamera.target = pbMerga[0];
                }
            }
        }
        
        public static void RefreshHealthbarsIfNeeded()
        {
            if (bossHuds == null || bossHuds.Length < 1)
            {
                bossHuds = GameObject.FindObjectsOfType<FPBossHud>();
            }
        }

        public static void SpawnRandomBoss()
        {
            if (BossInstanceCache != null && BossInstanceCache.Count > 0)
            {
                var indBossToSpawn = UnityEngine.Random.RandomRangeInt(0, BossInstanceCache.Count);
                var goBoss = GameObject.Instantiate(BossInstanceCache[indBossToSpawn]);
                var goBossHud = GameObject.Instantiate(BossInstanceHealthBarCache[indBossToSpawn]);
                
                goBoss.SetActive(true);
                goBossHud.SetActive(true);

                var bossEneBase = goBoss.GetComponent<FPBaseEnemy>();

                goBossHud.GetComponent<FPBossHud>().targetBoss = bossEneBase;

                var fpp = FPStage.currentStage.GetPlayerInstance();
                if (fpp != null)
                {
                    bossEneBase.position = new Vector2(fpp.position.x + 128, fpp.position.y);
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
            PHKAttack1 = CreateEntryAndBindHotkey("PHKAttack1", "Alpha1");
            PHKAttack2 = CreateEntryAndBindHotkey("PHKAttack2", "Alpha2");
            PHKAttack3 = CreateEntryAndBindHotkey("PHKAttack3", "Alpha3");
            PHKAttack4 = CreateEntryAndBindHotkey("PHKAttack4", "Alpha4");
            PHKSpawnRandomBosses = CreateEntryAndBindHotkey("PHKSpawnRandomBosses", "Alpha0");
            
            
            
            PHKMoveLeft = CreateEntryAndBindHotkey("PHKMoveLeft", "A");
            PHKMoveRight = CreateEntryAndBindHotkey("PHKMoveRight", "D");
            PHKJump = CreateEntryAndBindHotkey("PHKJump", "W");
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

        public static void RunLeft(PlayerBossMerga merga)
        {
            //merga.groundVel -= 3f;
            merga.direction = FPDirection.FACING_LEFT;
            HandleGeneralRun(merga);
        }
        public static void RunRight(PlayerBossMerga merga)
        {
            merga.direction = FPDirection.FACING_RIGHT;
            HandleGeneralRun(merga);
        }
        public static void Act_Jump(PlayerBossMerga merga)
        {
            if (merga.onGround)
            {
                merga.velocity.x = merga.groundVel;
                merga.groundVel = 0.0f;
                
                merga.velocity.y = 9.1f;
                merga.onGround = false;
                merga.SetPlayerAnimation("Jumping");
                if ((UnityEngine.Object) merga.hotspot != (UnityEngine.Object) null)
                {
                    merga.hotspot.followTargetX = true;
                    merga.hotspot.followTargetY = true;
                }
            }
        }

        public static void Act_FastFall(PlayerBossMerga merga)
        {
            if (merga.velocity.y > -0.375f)
            {
                merga.velocity.y = -0.375f;
                merga.SetPlayerAnimation("Jumping_Loop");
            }
        }
        
        public static void State_PlayerDirectControl()
        {
            
        }

        public static void HandleGeneralRun(PlayerBossMerga merga)
        {
            merga.state = new FPObjectState(State_PlayerDirectControl);
            
            if (merga.direction == FPDirection.FACING_LEFT)
            {
                if ((double) merga.groundVel > -(double) merga.topSpeed)
                {
                    merga.groundVel -= merga.acceleration * FPStage.deltaTime;
                }
                else
                {
                    merga.groundVel = -merga.topSpeed;
                    merga.SetPlayerAnimation("TopSpeed");
                }
            }
            else if (merga.direction == FPDirection.FACING_RIGHT)
            {
                if ((double) merga.groundVel < (double) merga.topSpeed)
                {
                    merga.groundVel += merga.acceleration * FPStage.deltaTime;
                }
                else
                {
                    merga.groundVel = merga.topSpeed;
                    merga.SetPlayerAnimation("TopSpeed");
                }
            }
            float num1 = Mathf.Abs(merga.groundVel) / (merga.deceleration * 2f);
            float num2 = Mathf.Abs(merga.groundVel) / 2f * num1;
            float num3 = merga.targetPlayer.position.x + merga.targetPlayer.velocity.x * num1;
            if ((UnityEngine.Object) merga.targetPlayer != (UnityEngine.Object) null && ((double) Mathf.Abs(merga.groundVel) >= (double) merga.topSpeed || merga.persistentChase) && (merga.direction == FPDirection.FACING_LEFT && (double) num3 >= (double) merga.position.x - (double) num2 - 180.0 && (double) num3 <= (double) merga.position.x - (double) num2 - 80.0 || merga.direction == FPDirection.FACING_RIGHT && (double) num3 >= (double) merga.position.x + (double) num2 + 80.0 && (double) num3 <= (double) merga.position.x + (double) num2 + 180.0 || (UnityEngine.Object) merga.colliderWall != (UnityEngine.Object) null))
            {
                merga.SetPlayerAnimation("AttackRun");
                //merga.genericFlag = true;
            }
        }

        public static void Handle360Movement(PlayerBossMerga merga)
        {
            merga.position.y += merga.velocity.y * FPStage.deltaTime;
            merga.velocity.y -= 0.375f * FPStage.deltaTime;
            merga.Process360Movement();
            merga.angle = merga.groundAngle;
        }
    }
}
