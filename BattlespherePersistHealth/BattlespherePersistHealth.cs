using System;
using System.Reflection;
using BepInEx;
using BepInEx.Configuration;
using RisingSlash.FP2Mods.RisingSlashCommon;
using UnityEngine;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random;

namespace RisingSlash.FP2Mods.BattlespherePersistHealth
{
    [BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
    [BepInProcess("FP2.exe")]
    public class BattlespherePersistHealth : BaseUnityPlugin
    {      
        private static ConfigEntry<bool> configPersistBattlesphereHealth;
        public static ConfigEntry<bool> configAutoBossSurvival;
        
        public float LastBattlesphereHealth = -1f;
        public bool AppliedStartHealth = false;
        TextMesh debugText;
        public int levelCount = 0;
        public float confirmTime = 3f;
        public float confirmTimeMax = 3f;

        public MenuArena menuArena;
        public MenuArenaBossSelect menuArenaBossSelect;
        

        public void Awake()
        {
            InitConfigs();
        }

        public void Update()
        {
            HandleAutoSurvival();
            if ( !configPersistBattlesphereHealth.Value
                 ||FPStage.currentStage == null 
                 || FPStage.currentStage.GetPlayerInstance_FPPlayer() == null
                 || FPSaveManager.currentSave == null)
            {
                return;
            }

            if (!(debugText != null))
            {
                debugText = OnScreenTextUtil.CreateOnScreenText($"HP: {LastBattlesphereHealth}");
                debugText.transform.position += new Vector3(0f, 0f, 16f); 
                DontDestroyOnLoad(debugText.gameObject);
            }
            else
            {
                debugText.text = $"HP: {LastBattlesphereHealth.ToString("0.00")}, Level: {levelCount}"; 
            }

            var fpp = FPStage.currentStage.GetPlayerInstance_FPPlayer();
            
            if (AppliedStartHealth)
            {
                //Logger.LogInfo("Health Already Applied");
                if (ConvenienceMethods.bHasSceneChanged)
                {
                    AppliedStartHealth = false;
                }
                else
                {
                    if (fpp.health < LastBattlesphereHealth + 1.1f)
                    {
                        LastBattlesphereHealth = fpp.health;
                    }
                    else
                    {
                        fpp.health = LastBattlesphereHealth;
                    }

                    if (fpp.state == fpp.State_KO || fpp.state == fpp.State_CrushKO)
                    {
                        LastBattlesphereHealth = -1f;
                        levelCount = 0;
                    }
                }

                return;
            }
            else
            {
                bool inBattlesphere = SceneManager.GetActiveScene().name.StartsWith("Battlesphere");
                if (inBattlesphere)
                {
                    Logger.LogInfo("Health Not Applied Yet");
                    
                        if (LastBattlesphereHealth == -1f)
                    {
                        LastBattlesphereHealth = fpp.healthMax;
                    }
                    else
                    {
                        if (fpp.health == LastBattlesphereHealth
                            && FPSaveManager.health == LastBattlesphereHealth)
                        {
                            confirmTime -= Time.deltaTime;
                            if (confirmTime <= 0)
                            {
                                levelCount++;
                                AppliedStartHealth = true;
                            }
                        }
                        else
                        {
                            confirmTime = confirmTimeMax;
                        }

                        if (FPStage.player != null)
                        {
                            foreach (var pffpp in FPStage.player)
                            {
                                pffpp.health = LastBattlesphereHealth;
                            }
                        }
                        fpp.health = LastBattlesphereHealth;
                    }
                
                    Logger.LogInfo("Setting Health To Applied.");

                    if (FPSaveManager.currentSave != null)
                    {
                        FPSaveManager.health = LastBattlesphereHealth;
                        if (FPSaveManager.targetPlayer != null)
                        {
                            FPSaveManager.targetPlayer.health = LastBattlesphereHealth;
                        }
                    }

                    
                }
                else
                {
                    //Logger.LogInfo("Not in Battlesphere...");

                }
            }
        }

        public void HandleAutoSurvival()
        {
            try
            {
                //Logger.LogInfo("AutoSurv1");
                if (configAutoBossSurvival.Value && levelCount > 0)
                {
                    //Logger.LogInfo("AutoSurv2");
                    if (menuArena == null && SceneManager.GetActiveScene().name.StartsWith("ArenaMenu"))
                    {
                        Logger.LogInfo("AutoSurv3");
                        //var go = GameObject.Find("ArenaBossSelect");
                        var go = GameObject.Find("ArenaMenu(Clone)");
                        if (go == null)
                        {
                            return;
                        }
                        menuArena = go.GetComponent<MenuArena>();
                        if (menuArena == null)
                        {
                            return;
                        }
                        
                        Logger.LogInfo("AutoSurv4");
                        Logger.LogInfo("AutoSurv5");
                        menuArenaBossSelect = Instantiate(menuArena.bossMenu);
                        var startMe = menuArenaBossSelect.GetType()
                            .GetMethod("Start", BindingFlags.Instance | BindingFlags.NonPublic);
                        startMe.Invoke(menuArenaBossSelect, null); // Force an early start so the values actually get set without having to wait for the next frame.
                        
                        Logger.LogInfo("AutoSurv5a");
                        var fieldInfo = menuArenaBossSelect.GetType().GetField("internalSpawnID", BindingFlags.Instance | BindingFlags.NonPublic);
                        Logger.LogInfo("AutoSurv5b");
                        var internalSpawnIDs = (int[])fieldInfo.GetValue(menuArenaBossSelect);
                        Logger.LogInfo("AutoSurv5c");
                        var bossSelection = Random.RandomRangeInt(0, internalSpawnIDs.Length - 1);
                        
                        Logger.LogInfo("AutoSurv6");
                        
                        fieldInfo = menuArenaBossSelect.GetType().GetField("bossSlotSceneList", BindingFlags.Instance | BindingFlags.NonPublic);
                        var bossSlotSceneList = (string[])fieldInfo.GetValue(menuArenaBossSelect);
                        
                        Logger.LogInfo("AutoSurv7");
                        
                        //FPSaveManager.currentArenaChallenge = internalSpawnIDs[bossSelection];
                        var sceneToLoad = bossSlotSceneList[bossSelection];
                        //SceneManager.LoadSceneAsync(sceneToLoad);
                        

                        Logger.LogInfo("AutoSurv8");
                        StartTransition(sceneToLoad, internalSpawnIDs[bossSelection]);
                    }

                    return;
                }
            }
            catch (Exception e)
            {
                ConvenienceMethods.LogExceptionError(Logger, e);
                menuArena = null;
            }

            
        }

        public void StartTransition(string sceneToload = "", int internalSpawnID = -1)
        {
            FPScreenTransition component = GameObject.Find("Screen Transition").GetComponent<FPScreenTransition>();
            component.transitionType = FPTransitionTypes.WIPE;
            component.transitionSpeed = 48f;
            component.sceneToLoad = sceneToload;
            component.SetTransitionColor(0f, 0f, 0f);
            component.BeginTransition();
            FPAudio.PlayMenuSfx(3);
            if (internalSpawnID > -1)
            {
                FPSaveManager.currentArenaChallenge = internalSpawnID;
            }
            //base.enabled = false;
        }

        public void InitConfigs()
        {
            configPersistBattlesphereHealth = Config.Bind("General",      // The section under which the option is shown
                "PersistentBattlesphereHealth",  // The key of the configuration option in the configuration file
                true, // The default value
                "Toggle this on if you want to challenge multiple Battlesphere challenges where your HP doesn't refill between levels."); // Description of the option to show in the config file
            
            configAutoBossSurvival = Config.Bind("General",      // The section under which the option is shown
                "AutoBossSurvival",  // The key of the configuration option in the configuration file
                true, // The default value
                "If enabled, every time you return to the Battlesphere Arena menu, a boss will be chosen at random for you to fight until you are defeated."); // Description of the option to show in the config file
            
            /*
            configMainPlayerOnly = Config.Bind("General",      // The section under which the option is shown
                "MainPlayerOnly",  // The key of the configuration option in the configuration file
                true, // The default value
                "Toggle this on if you're planning to spawn in multiple FPPlayers and need to iterate through all of them."); // Description of the option to show in the config file
            */
        }
    }
}
