using System;
using System.Collections.Generic;
using System.Reflection;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using MonoMod.Utils;
using RisingSlash.FP2Mods.RisingSlashCommon;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = System.Object;

namespace RisingSlash.FP2Mods.BossBetrayal
{
    [BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
    [BepInProcess("FP2.exe")]
    
    [DefaultExecutionOrder(-100)]
    public class BossBetrayal : BaseUnityPlugin
    {
        private static ConfigEntry<string> configSerpBootupLevel;
        private static ConfigEntry<string> configBossToLoad;
        private static ConfigEntry<int> configSaveFileNumber;
        private static ConfigEntry<bool> configShowTransitionWipe;

        private static bool firstUpdate = false;

        public delegate void BossBetrayalState();

        public static ManualLogSource sLogger;

        public static BossBetrayalState CurrentState = StateDoNothing;
        
        public static string currentSceneName = "";
        public static string previousSceneName = "";

        public static bool stateInit = false;

        public static string donorLevel = "";

        public static GameObject DonorBossInstance;
        public static GameObject CurrentActiveBossInstance;
        public static int forceNoEventFrames = 10;
        public static bool permaFollow = true;

        public static Dictionary<string, FPObjectState> serpStates;


        public static float previousHealth = 0f;
        
        private void Awake()
        {
            sLogger = Logger;
            sLogger.LogInfo($"{MyPluginInfo.PLUGIN_GUID} is loaded! Initializing configs.");
            InitConfigs();
            CurrentState = StateLoadDonorLevel;
            serpStates = new Dictionary<string, FPObjectState>();
        }

        private static void OnFirstUpdateDonor()
        {
            // RisingSlashCommon startup logic
            try
            {
                if (configSaveFileNumber.Value > 0)
                {
                    sLogger.LogInfo($"Attempting to load Save File number \"{configSaveFileNumber.Value.ToString()}\" ");
                    FPSaveManager.LoadFromFile(configSaveFileNumber.Value);
                }
                else
                {
                    sLogger.LogInfo("Skipping save file load for booting.");
                }

                string level = configSerpBootupLevel.Value;
                

                if (configBossToLoad.Value.ToLower().Equals("serpentine"))
                {
                    donorLevel = "Snowfields";
                }
                
                sLogger.LogInfo($"Attempting to jump to Donor Level: \"{donorLevel}\" ");
                sLogger.LogInfo($"Pre Boot Immediate");
                if (configShowTransitionWipe.Value)
                {
                    BootLevel(donorLevel);
                }
                else
                {
                    BootLevelImmediate(donorLevel);
                }
                sLogger.LogInfo($"Post Boot Immediate");
            } 
            catch (Exception e)
            {
                ConvenienceMethods.LogExceptionError(e);
            }
        }
        
        private static void OnFirstUpdatePlay()
        {
            // RisingSlashCommon startup logic
            try
            {
                if (configSaveFileNumber.Value > 0)
                {
                    sLogger.LogInfo($"Attempting to load Save File number \"{configSaveFileNumber.Value.ToString()}\" ");
                    FPSaveManager.LoadFromFile(configSaveFileNumber.Value);
                }
                else
                {
                    sLogger.LogInfo("Skipping save file load for booting.");
                }

                string level = configSerpBootupLevel.Value;
                sLogger.LogInfo($"Attempting to jump to SerpBootupLevel: \"{configSerpBootupLevel.Value}\" ");
                if ( level != null && !level.Equals("") && !level.Equals("MainMenu"))
                {
                    sLogger.LogInfo($"Pre Boot Immediate");
                    if (configShowTransitionWipe.Value)
                    {
                        BootLevel(configSerpBootupLevel.Value);
                    }
                    else
                    {
                        BootLevelImmediate(configSerpBootupLevel.Value);
                    }
                    sLogger.LogInfo($"Post Boot Immediate");
                }
                else if (level != null && (level.Equals("")))
                {
                    // In this particular case, we don't want to try to quick-skip the menu intro since wse need to load it properly from a different scene.
                    sLogger.LogInfo($"Blank stage value. Waiting to return to Main Menu.");
                    if (configShowTransitionWipe.Value)
                    {
                        BootLevel("MainMenu");
                    }
                    else
                    {
                        BootLevelImmediate("MainMenu");
                    }
                }
                else 
                if (level != null && (level.Equals("MainMenu")))
                {
                    sLogger.LogInfo($"Pre Boot Main No Logos");
                    GoToMainMenuNoLogos();
                    sLogger.LogInfo($"Post Boot Main No Logos");
                }
            } 
            catch (Exception e)
            {
                ConvenienceMethods.LogExceptionError(e);
            }
        }

        public void Update()
        {
            CurrentState();
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
            sLogger.LogInfo($"{MyPluginInfo.PLUGIN_GUID} InitConfigs");
            configSerpBootupLevel = Config.Bind("General",      // The section under which the option is shown
                "SerpBootupLevel",  // The key of the configuration option in the configuration file
                "", // The default value
                "The level you want to boot to. This corresponds to the Scene Name, not the in-game Level Name. Try BakunawaBoss1 or Zao Land for example. You can even boot to AdventureMenu or ClassicMenu"); // Description of the option to show in the config file
            
            configBossToLoad = Config.Bind("General",      // The section under which the option is shown
                "BossToLoad",  // The key of the configuration option in the configuration file
                "serpentine", // The default value
                "Name of the boss to load. Valid options are: serpentine"); // Description of the option to show in the config file
            
            configSaveFileNumber = Config.Bind("General",      // The section under which the option is shown
                "SaveNumber",  // The key of the configuration option in the configuration file
                1, // The default value
                "The number of the save file you wish to use when quick-booting. Set this value to 0 to boot into a level with debug settings (no save file). Normally, this would be files 1-10, but can be longer if you have additional saves (from modding or otherwise). If you need to see your save files, they are stored at \"<AppData>\\LocalLow\\GalaxyTrail\\Freedom Planet 2\"."); // Description of the option to show in the config file
            
            configShowTransitionWipe = Config.Bind("General",      // The section under which the option is shown
                "ShowTransitionWipe",  // The key of the configuration option in the configuration file
                false, // The default value
                "Showing the black wipe transition looks cleaner and is more likely to work with built-in levels. Disable this when loading scenes from asset bundles."); // Description of the option to show in the config file
        }

        public static void StateDoNothing()
        {
            return;
        }
        
        public static void StateLoadSettings()
        {
            //InitConfigs();
            return;
        }
        
        public static void StateLoadDonorLevel()
        {
            if (!stateInit)
            {
                firstUpdate = false;
                stateInit = true;
            }
            if (!firstUpdate)
            {
                OnFirstUpdateDonor();
                firstUpdate = true;
            }
            
            if (SceneManager.GetActiveScene().name.Equals(donorLevel))
            {
                sLogger.LogInfo("We're in the donor level now.");
                stateInit = false;
                CurrentState = StateCacheBossInstance;
            }
            return;
        }
        
        public static void StateCacheBossInstance()
        {
            if (!stateInit)
            {
                stateInit = true;
            }

            DonorBossInstance = GameObject.Find("Boss Serpentine");
            
            // Backup approach if the boss is inactive at scene start. This is very slow
            /*
             * var gos = Object.FindObjectsOfType<GameObject>(true);
             * foreach (var go in gos) {
             *  if (go.name.equals("Boss Serpentine")) { DonorBossInstance = go;}
             * }
             */
            
            if (DonorBossInstance != null)
            {
                sLogger.LogInfo("Found boss instance. Making a cached copy.");
                DonorBossInstance = Instantiate(DonorBossInstance);
                DontDestroyOnLoad(DonorBossInstance);
                DonorBossInstance.SetActive(false);
                DonorBossInstance.GetComponent<PlayerBossSerpentine>().faction = "Player";

                try
                {
                    var p1 = FPStage.currentStage.GetPlayerInstance_FPPlayer();
                    var serp = DonorBossInstance.GetComponent<PlayerBossSerpentine>();
                    var bossHUD = serp.GetComponent<FPBossHud>();
                    if (p1 != null)
                    {
                        serp.health = p1.healthMax * 10;
                        bossHUD.maxHealth = serp.health;
                        bossHUD.maxPetals = Mathf.FloorToInt(bossHUD.maxHealth / 10 );
                    }
                }
                catch (Exception e)
                {
                    ConvenienceMethods.LogExceptionError(e);
                }
            }


            if (SceneManager.GetActiveScene().name.Equals(donorLevel))
            {
                sLogger.LogInfo("Boss is cached, time to load the level to play.");
                FPStage.eventIsActive = false;
                FPStage.currentStage.disablePausing = false;
                stateInit = false;
                CurrentState = StateLoadLevelToPlay;
            }
            return;
        }
        public static void StateLoadLevelToPlay()
        {
            if (!stateInit)
            {
                firstUpdate = false;
                stateInit = true;
                if (configSerpBootupLevel.Equals(""))
                {
                    firstUpdate = true;
                }
            }
            if (!firstUpdate)
            {
                OnFirstUpdatePlay();
                firstUpdate = true;
                
                if (!firstUpdate && configSerpBootupLevel.Value.Equals(""))
                {
                    sLogger.LogInfo("Bootup Level is Blank. Waiting to return to MainMenu.");
                }
            }

            previousSceneName = currentSceneName;
            currentSceneName = SceneManager.GetActiveScene().name;

            if (configSerpBootupLevel.Value.Equals(""))
            {
                if (currentSceneName.Equals("MainMenu"))
                {
                    sLogger.LogInfo("Main Menu has loaded again. Waiting for a playable level to start.");
                    sLogger.LogInfo("Switching to StateWaitForPlayableLevel.");
                    stateInit = false;
                    CurrentState = StateWaitForPlayableLevel;
                }
            }
            else if (currentSceneName.Equals(configSerpBootupLevel.Value))
            {
                sLogger.LogInfo("We're in the level we want to play now. Instantiating Boss Instance from Cached copy.");

                sLogger.LogInfo("Switching to Boss Instantiation State.");
                stateInit = false;
                CurrentState = StateInstanceTheBoss;
            }

            return;
        }
        
        public static void StateWaitForPlayableLevel()
        {
            previousSceneName = currentSceneName;
            currentSceneName = SceneManager.GetActiveScene().name;

            if (!currentSceneName.Contains("Menu") 
                && !currentSceneName.Contains("Loading")
                && !currentSceneName.Contains("Credits")
                && !currentSceneName.Contains("Cutscene_"))
            {
                sLogger.LogInfo("This appears to be a normal play able stage. Instantiating Boss Instance from Cached copy.");

                //InstanceTheBoss();
                
                sLogger.LogInfo("Switching to Boss Instantiation State.");
                stateInit = false;
                CurrentState = StateInstanceTheBoss;
            }

            return;
        }

        public static void InstanceTheBoss()
        {
            try
            {
                CurrentActiveBossInstance = Instantiate(DonorBossInstance);
                //SceneManager.MoveGameObjectToScene(CurrentActiveBossInstance, SceneManager.GetActiveScene());
                CurrentActiveBossInstance.SetActive(true);
                DontDestroyOnLoad(CurrentActiveBossInstance);

                if (FPStage.currentStage != null && FPStage.currentStage.GetPlayerInstance() != null)
                {
                    var fpPlayerBase = FPStage.currentStage.GetPlayerInstance();
                    var playerBoss = CurrentActiveBossInstance.GetComponent<PlayerBoss>();
                    if (fpPlayerBase != null)
                    {
                        sLogger.LogInfo(
                            "Found a player object. Moving the boss to that location and deactivating the old player.");
                        CurrentActiveBossInstance.GetComponent<PlayerBossSerpentine>().position =
                            new Vector2(fpPlayerBase.gameObject.transform.position.x,
                                fpPlayerBase.gameObject.transform.position.y);

                        //fpPlayerBase.gameObject.SetActive(false);
                        //fpPlayerBase.enabled = false;

                        playerBoss.enabled =  true;
                        FPCamera.SetCameraTarget(CurrentActiveBossInstance);
                        //FPStage.currentStage.SetPlayerInstance(CurrentActiveBossInstance);
                        FPStage.ValidateStageListPos(playerBoss);
                    }
                    sLogger.LogInfo("Finished boss instantiation without throwing an exception.");
                }
                
            }
            catch (Exception e)
            {
                ConvenienceMethods.LogExceptionError(e);
            }
        }

        public static void SerpSetStateJumping(PlayerBossSerpentine serp)
        {
            MethodInfo st_jumping = serp.GetType().GetMethod("State_Jumping", 
                BindingFlags.NonPublic | BindingFlags.Instance);
            //dynMethod.Invoke(this, new object[] { methodParams });
            FPObjectState d_st_jumping;
            d_st_jumping = (FPObjectState) st_jumping.CreateDelegate(typeof(FPObjectState), serp);
                        
            //serp.ResetAllVars();
            //merga.state = new FPObjectState(merga.State_RunAttack);
            //merga.state = new FPObjectState(st_run_attack);
            serp.state = d_st_jumping;
        }
        
        public static void SerpSetStateByPrivateMethodName(PlayerBossSerpentine serp, string stateName)
        {
            MethodInfo st_jumping = serp.GetType().GetMethod(stateName, 
                BindingFlags.NonPublic | BindingFlags.Instance);
            //dynMethod.Invoke(this, new object[] { methodParams });
            FPObjectState d_st_jumping;
            d_st_jumping = (FPObjectState) st_jumping.CreateDelegate(typeof(FPObjectState), serp);
                        
            //serp.ResetAllVars();
            //merga.state = new FPObjectState(merga.State_RunAttack);
            //merga.state = new FPObjectState(st_run_attack);
            serp.state = d_st_jumping;
        }
        
        public static void SerpSetStateByPrivateFieldName(PlayerBossSerpentine serp, string fieldName, object newValue)
        {
            try
            {
                FieldInfo fl_name = serp.GetType().GetField(fieldName, 
                    BindingFlags.NonPublic | BindingFlags.Instance);
                fl_name.SetValue(serp, newValue);
            }
            catch (Exception e) 
            {
                ConvenienceMethods.LogExceptionError(e);
            }
        }

        public static void StateInstanceTheBoss()
        {
            InstanceTheBoss();
            sLogger.LogInfo("Switching to PlayerComponentAdd State.");
            stateInit = false;
            CurrentState = StateAddPlayerComponents;
            return;
        }

        public static void StateAddPlayerComponents()
        {
            if (!stateInit)
            {
                sLogger.LogInfo("Skipping over adding the playercomponents to the enemies as a debugging measure.");
                AddPlayerComponents();
                
                SerpStart();

                sLogger.LogInfo("Switching to Idle State.");
                stateInit = false;
                CurrentState = Idle;
                //stateInit = true;
            }
            return;
        }

        public static void AddPlayerComponents()
        {
            //var allEnemies = FPStage.GetActiveEnemies();
            // Not using ActiveEnemies here since that only works for enemies close enough to the player to be enabled and visible.
            FPPlayer fpp;
            var allEnemies = FindObjectsOfType<FPBaseEnemy>();
            foreach (var enemy in allEnemies)
            {
                try
                {
                    if (enemy.gameObject.GetComponent<PlayerBoss>() != null)
                    {
                        continue;
                    }

                    //continue; //Terminating early to see if adding this script is why these objects are disappearing.

                    enemy.gameObject.AddComponent<FPPlayer>();
                    fpp = enemy.gameObject.GetComponent<FPPlayer>();
                    // Don't forget to validate the type or else it won't run properly.
                    FPStage.ValidateStageListPos(fpp);
                    fpp.enabled = false;
                    //fpp.gameObject.SetActive(false);
                }
                catch (Exception e)
                {
                    ConvenienceMethods.LogExceptionError(e);
                }
            }
        }



        public static void Idle()
        {
            if (!stateInit)
            {
                forceNoEventFrames = 300;
                stateInit = true;
            }

            try
            {
                if (CurrentActiveBossInstance != null )
                {
                    FPCamera.SetCameraTarget(CurrentActiveBossInstance);
                    
                    //SerpForceUpdateIgnoreEnabled();
                    
                    FPPlayer tempFpp;
                    foreach (var currentEnemy in FPStage.GetActiveEnemies())
                    {
                        tempFpp = currentEnemy.GetComponent<FPPlayer>();
                        if (tempFpp != null)
                        {
                            tempFpp.position = currentEnemy.position;
                        }
                    }
                }
                else
                {
                    previousSceneName = currentSceneName;
                    currentSceneName = SceneManager.GetActiveScene().name;
                    //if (ConvenienceMethods.bHasSceneChanged && !currentSceneName.Equals("Loading"))
                    if (!previousSceneName.Equals(currentSceneName) && !currentSceneName.Equals("Loading"))
                    {
                        // Attempt to instantiate the boss again.
                        sLogger.LogInfo("Scene changed. Switching to StateWaitForPlayableLevel.");
                        CurrentActiveBossInstance.SetActive(false);
                        DestroyImmediate(CurrentActiveBossInstance);
                        CurrentActiveBossInstance = null;
                        //Destroy(CurrentActiveBossInstance);
                        stateInit = false;
                        CurrentState = StateWaitForPlayableLevel;
                    }
                }
                
                if (forceNoEventFrames > 0)
                {
                    FPStage.eventIsActive = false;
                    FPStage.currentStage.disablePausing = false;
                    forceNoEventFrames--;
                    CurrentActiveBossInstance.SetActive(true);
                    SerpSyncToCarol();
                }
            }
            catch (Exception e)
            {
                ConvenienceMethods.LogExceptionError(e);
            }

            

            return;
        }

        public static void SerpStart()
        {
            var serp = CurrentActiveBossInstance.GetComponent<PlayerBossSerpentine>();
            if (serp.bgmBoss != null)
            {
                FPAudio.PlayMusic(serp.bgmBoss);
            }
            serp.Action_PlayVoice(serp.vaStart[UnityEngine.Random.Range(0, serp.vaStart.Length - 1)]);
            serp.isTalking = true;
            serp.voiceTimer = 240f;
            FPStage.timeEnabled = true;
            serp.bossActivated = true;
            FPBossHud component = serp.GetComponent<FPBossHud>();
            if (component != null)
            {
                component.MoveIn();
                component.hudPosition += new Vector2(-128, 64);
            }
            if (serp.nextBoss != null)
            {
                serp.nextBoss.gameObject.SetActive(value: false);
            }
            serp.Action_Dash();
        }
        
        public static void SerpForceUpdateIgnoreEnabled()
        {
            var serp = CurrentActiveBossInstance.GetComponent<PlayerBossSerpentine>();
            if (serp == null)
            {
                sLogger.LogInfo("Can't run Serp state machine, instance was not found?");
            }
            else if (serp.state == null)
            {
                sLogger.LogInfo("Serp instance exists, but the state is null????");
            }

            try
            {
                serp.state();
            }
            catch (Exception e)
            {
                ConvenienceMethods.LogExceptionError(e);
            }
        }

        public static void SerpStartShooting(PlayerBossSerpentine serp)
        {
            serp.SetPlayerAnimation("Shooting");
            //serp.state = serp.State_Shooting;
            SerpSetStateByPrivateMethodName(serp, "State_Shooting");
            serp.Action_PlaySoundUninterruptable(serp.sfxCharge);
            //nextAttack++;
        }
        
        public static void SerpStartDualGround(PlayerBossSerpentine serp)
        {
            serp.direction = FPDirection.FACING_RIGHT;
            serp.SetPlayerAnimation("Dual_Ground");
            SerpSetStateByPrivateFieldName(serp, "rapidShoot", false);
            //serp.rapidShoot = false;
            SerpSetStateByPrivateMethodName(serp, "State_DualCrash");
            //serp.state = serp.State_DualCrash;
            //nextAttack++;
        }
        
        public static void SerpStartDualAir(PlayerBossSerpentine serp)
        {
            
            serp.SetPlayerAnimation("Dual_Ground");
            SerpSetStateByPrivateFieldName(serp, "rapidShoot", false);
            //serp.rapidShoot = false;
            SerpSetStateByPrivateMethodName(serp, "State_DualCrash");
            //serp.state = serp.State_DualCrash;
            //nextAttack++;
            
            serp.SetPlayerAnimation("Dual_Air");
            SerpSetStateByPrivateMethodName(serp, "State_DualCrashAir");
            
            Warning warning = (Warning)FPStage.CreateStageObject(Warning.classID, serp.position.x, serp.position.y);
            warning.parentObject = serp.gameObject;
            warning.warningBoxSize.x = 64f;
            warning.warningBoxSize.y = 64f;
            warning.top = true;
            warning.right = true;
            warning.left = true;
            warning.bottom = true;
            warning.duration = 120f;
            warning.snapToParent = true;
            warning.parentOffset.x = 0f;
            warning.parentOffset.y = 0f;
            warning.SetupLayer();
            warning.SetupPosition();
        }
        
        public static void SerpStartJump(PlayerBossSerpentine serp)
        {
            serp.velocity.y = 12f;
            serp.onGround = false;
            serp.animationFlag = false;
            serp.direction = FPDirection.FACING_RIGHT;
            serp.SetPlayerAnimation("Jumping");
            SerpSetStateByPrivateMethodName(serp, "State_Jumping");
            serp.Action_PlaySound(serp.sfxJump);
        }

        public static void SerpSyncToCarol()
        {
            var serp = CurrentActiveBossInstance.GetComponent<PlayerBossSerpentine>();
            var p1 = FPStage.currentStage.GetPlayerInstance_FPPlayer();
            serp.enabled = true;
            serp.activationMode = FPActivationMode.ALWAYS_ACTIVE;
            serp.enablePhysics = true;
            serp.playerTerrainCheck = true;
            serp.position = p1.position;
            
            serp.targetPlayer = FPStage.FindNearestPlayer(serp, 100000f);

            if (permaFollow)
            {
                forceNoEventFrames++;

                //serp.health = 1000;
                //serp.health = p1.health;
                if (p1.health > previousHealth)
                {
                    serp.health = p1.health * 10;
                }

                if (p1.health < previousHealth)
                {
                    serp.healthToFlinch = serp.health;
                    serp.invincibility = 240f;
                }
                
                if (serp.healthToFlinch == serp.health) 
                {
                    serp.healthToFlinch = 0;
                }



                p1.health = serp.health / 10f;
                previousHealth = p1.health;
                        
                p1.childRender.enabled = false;
                p1.GetComponent<Renderer>().enabled = false;
                p1.invincibilityTime = 999;
                        
                p1.attackPower = 0f;
                p1.attackHitstun = 0f;
                p1.attackEnemyInvTime = 0f;
                p1.attackKnockback.x = 0f;
                p1.attackKnockback.y = 0f;
                p1.attackSfx = 0;

                serp.guardTime = p1.guardTime;
                /*
                if (serp.currentAnimation.Equals("Missile"))
                {
                    serp.currentAnimation = "Shooting";
                }
                */

                if (serp.nextAttack == 7)
                {
                    serp.nextAttack = 0;
                }
            }

            serp.direction = p1.direction;

            /*
            if (p1.currentAnimation == "Jumping")
            {
                //SerpSetStateByPrivateMethodName(serp, "State_Jumping");
            }
            */

            p1.GetInputFromPlayer1();
            serp.input = p1.input;

            if (serp.input.attackHold ||  serp.input.attackPress)
            {
                if (serp.input.up)
                {
                    serp.genericTimer = 1f;
                    SerpSetStateByPrivateMethodName(serp, "State_DualCrash");
                }
                else if (serp.input.up)
                {
                    serp.genericTimer = 1f;
                    SerpSetStateByPrivateMethodName(serp, "State_DualCrashAir");
                }
                else if (p1.input.attackPress)
                {
                    serp.genericTimer = 1f;
                    //SerpSetStateByPrivateMethodName(serp, "State_Shooting");
                    SerpStartShooting(serp);
                } 
                else if (p1.input.specialPress)
                {
                    serp.genericTimer = 1f;
                    SerpSetStateByPrivateMethodName(serp, "State_Missile");
                }
                else if (p1.acceleration * p1.groundVel < 0) // Is slowing down check.
                {
                    serp.genericTimer = 1;
                    SerpSetStateByPrivateMethodName(serp, "State_Skid");
                }
            }
            else
            {
                if (p1.currentAnimation == "Running" 
                    || p1.currentAnimation == "TopSpeed"
                    || (p1.onGround && (serp.input.left || serp.input.right)))
                {
                    serp.genericTimer = 0;
                    SerpSetStateByPrivateMethodName(serp, "State_Dash");
                }
            }

            /*
            if (serp.input.attackPress && serp.input.up)
            {
                serp.genericTimer = 1f;
                SerpSetStateByPrivateMethodName(serp, "State_DualCrash");
            }
            else if (p1.input.attackPress && p1.input.down)
            {
                serp.genericTimer = 1f;
                SerpSetStateByPrivateMethodName(serp, "State_DualCrashAir");
            }
            else if (p1.input.attackPress)
            {
                serp.genericTimer = 1f;
                //SerpSetStateByPrivateMethodName(serp, "State_Shooting");
                SerpStartShooting(serp);
            } 
            else if (p1.input.specialPress)
            {
                serp.genericTimer = 1f;
                SerpSetStateByPrivateMethodName(serp, "State_Missile");
            }
            else if (p1.currentAnimation == "Running" || p1.currentAnimation == "TopSpeed")
            {
                serp.genericTimer = 1;
                SerpSetStateByPrivateMethodName(serp, "State_Dash");
            }
            else if (p1.acceleration * p1.groundVel < 0) // Is slowing down check.
            {
                serp.genericTimer = 1;
                SerpSetStateByPrivateMethodName(serp, "State_Skid");
            }
            
            */

            /*
            else if (p1.state == p1.State_Carol_Punch)
            {
                //SerpSetStateByPrivateMethodName(serp, "State_Jumping");
                SerpStartShooting(serp);
            }
            
            else if (p1.state == p1.State_Carol_JumpDiscThrow || p1.state == p1.State_Carol_JumpDiscWarp)
            {
                //SerpSetStateByPrivateMethodName(serp, "State_Jumping");
                SerpStartDualGround(serp);
            }
            */

            //state != new FPObjectState(State_Carol_Punch) && state != new FPObjectState(State_Carol_JumpDiscThrow)
        }
    }
}

/*
 *CustomDamageInstance
 *DamageCheck_
 * 
 */