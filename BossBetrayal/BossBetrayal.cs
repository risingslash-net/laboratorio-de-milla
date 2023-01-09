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
        private static ConfigEntry<int> configCharacterIDExtended;
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
        public static float previousShieldHealth = 0f;
        public static string previousAnimationName = "";

        public static bool useEnergy = false;

        public static int buggedCounter = 0;
        
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
            
            configCharacterIDExtended = Config.Bind("General",      // The section under which the option is shown
                "CharacterIDExtended",  // The key of the configuration option in the configuration file
                5, // The default value
                "An integer value representing your currently selected character. Allows values beyond the canonical 0-4 IDs. You shouldn't need to change this manually. (5: Serpentine 6: Corazon 7: Kalaw 8: Unarmored Merga 9: Gong, 10 BFF)"); // Description of the option to show in the config file
            
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
                
                var preserveRocket = GameObject.Find("ProjectileRocket");
                if (preserveRocket != null)
                {
                    DontDestroyOnLoad(preserveRocket);
                }
                
                useEnergy = false;

                var tempRocketGo = new GameObject();
                var tempRocket = tempRocketGo.AddComponent<ProjectileRocket>();
                
                ProjectileRocket.classID = FPStage.RegisterObjectType(tempRocket, typeof(ProjectileRocket), 64);
                tempRocket.objectID = ProjectileRocket.classID;

                tempRocket.enabled = false;
                GameObject.Destroy(tempRocketGo);
                
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
                if (CurrentActiveBossInstance != null)
                {
                    CurrentActiveBossInstance.SetActive(false);
                    CurrentActiveBossInstance.GetComponent<PlayerBossSerpentine>().enabled = false;
                    Destroy(CurrentActiveBossInstance);
                    CurrentActiveBossInstance = null;
                }

                CurrentActiveBossInstance = Instantiate(DonorBossInstance);
                SceneManager.MoveGameObjectToScene(CurrentActiveBossInstance, SceneManager.GetActiveScene());
                CurrentActiveBossInstance.SetActive(true);
                DontDestroyOnLoad(CurrentActiveBossInstance);

                if (FPStage.currentStage != null && FPStage.currentStage.GetPlayerInstance() != null)
                {
                    var fpPlayerBase = FPStage.currentStage.GetPlayerInstance();
                    var playerBoss = CurrentActiveBossInstance.GetComponent<PlayerBoss>();
                    if (fpPlayerBase != null)
                    {
                        sLogger.LogInfo(
                            "Found a player object. Moving the boss to that location and hiding the old player.");
                        CurrentActiveBossInstance.GetComponent<PlayerBossSerpentine>().position =
                            new Vector2(fpPlayerBase.gameObject.transform.position.x,
                                fpPlayerBase.gameObject.transform.position.y);

                        //fpPlayerBase.gameObject.SetActive(false);
                        //fpPlayerBase.enabled = false;

                        playerBoss.enabled =  true;
                        //FPCamera.SetCameraTarget(CurrentActiveBossInstance);
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
                //forceNoEventFrames = 300;
                forceNoEventFrames = 0;
                stateInit = true;
            }

            try
            {
                var sceneChanged = false;
                
                previousSceneName = currentSceneName;
                currentSceneName = SceneManager.GetActiveScene().name;
                //if (ConvenienceMethods.bHasSceneChanged && !currentSceneName.Equals("Loading"))
                sceneChanged = (!previousSceneName.Equals(currentSceneName) && !currentSceneName.Equals("Loading"));
                if (sceneChanged)
                {
                    sLogger.LogInfo("Scene changed. We should see a second similar message when we reset the state.");
                }
                
                if (CurrentActiveBossInstance != null )
                {
                    //FPCamera.SetCameraTarget(CurrentActiveBossInstance);
                    // Note: Removed the camera retargeting because it seemed to bug things out?
                    
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
                
                if (sceneChanged)
                {
                    // Attempt to instantiate the boss again.
                    sLogger.LogInfo("Scene changed: Switching to StateWaitForPlayableLevel.");
                    CurrentActiveBossInstance.SetActive(false);
                    Destroy(CurrentActiveBossInstance);
                    CurrentActiveBossInstance = null;
                    stateInit = false;
                    CurrentState = StateWaitForPlayableLevel;
                }
                
                if (forceNoEventFrames > 0)
                {
                    FPStage.eventIsActive = false;
                    FPStage.currentStage.disablePausing = false;
                    forceNoEventFrames--;
                    CurrentActiveBossInstance.SetActive(true);
                    
                }
                
                SerpSyncToCarol();
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
                //FPAudio.PlayMusic(serp.bgmBoss);
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
                component.healthBarOffset += new Vector2(-128-64, -64-64);
                component.hudPosition.y += -64 - 64;
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
            //serp.animator.GetCurrentAnimatorStateInfo(0).
            serp.Action_FireBlaster(serp.angle);
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
            
            /*
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
            */
            
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
            
            serp.gameObject.SetActive(p1.gameObject.activeInHierarchy);
            
            FPStage.ValidateStageListPos(p1);
            FPStage.ValidateStageListPos(serp);

            NullifyPlayerSounds(p1);

            serp.enabled = true;
            serp.activationMode = FPActivationMode.ALWAYS_ACTIVE;
            serp.enablePhysics = true;
            serp.playerTerrainCheck = true;
            serp.position = p1.position;
            serp.collisionLayer = p1.collisionLayer;
            
            serp.targetPlayer = FPStage.FindNearestPlayer(serp, 100000f);

            if (permaFollow)
            {
                forceNoEventFrames++;

                if (p1.state == p1.State_Carol_JumpDiscThrow)
                {
                    p1.state = p1.State_Carol_Roll;
                }

                if (p1.state == p1.State_Victory)
                {
                    var jd = FindObjectOfType<CarolJumpDisc>();
                    GameObject.Destroy(jd);
                    p1.enabled = false;
                    p1.enablePhysics = false;
                }

                else if (p1.characterID == FPCharacterID.CAROL && p1.state != p1.State_Victory)
                {
                    p1.characterID = FPCharacterID.BIKECAROL;
                    var swapCharacter = FPStage.player[(int)FPCharacterID.BIKECAROL];
                    
                    //p1.useSpecialItem = false;
                    p1.characterID = swapCharacter.characterID;
                    p1.topSpeed = swapCharacter.topSpeed;
                    p1.acceleration = swapCharacter.acceleration;
                    p1.deceleration = swapCharacter.deceleration;
                    p1.airAceleration = swapCharacter.airAceleration;
                    p1.skidDeceleration = swapCharacter.skidDeceleration;
                    p1.skidThreshold = swapCharacter.skidThreshold;
                    p1.gravityStrength = swapCharacter.gravityStrength;
                    p1.jumpStrength = swapCharacter.jumpStrength;
                    p1.jumpRelease = swapCharacter.jumpRelease;
                    p1.energyRecoverRate = swapCharacter.energyRecoverRate;
                    p1.energyRecoverRateCurrent = swapCharacter.energyRecoverRate;
                    p1.climbingSpeed = swapCharacter.climbingSpeed;
                    
                    FPStage.DestroyStageObject(p1.carolJumpDisc);
                }
                

                //serp.health = 1000;
                //serp.health = p1.health;
                if (p1.health > previousHealth)
                {
                    serp.health = p1.health * 10;
                    serp.healthToFlinch = serp.health - 10;
                }
                if (p1.shieldHealth > previousShieldHealth)
                {
                    serp.shieldHealth = p1.shieldHealth;
                }
                else
                {
                    p1.shieldHealth = serp.shieldHealth;
                }

                if (serp.health <= serp.healthToFlinch) 
                {
                    serp.healthToFlinch -= 10;
                    
                    //serp.invincibility = 60f;
                    //serp.Action_Hurt();
                }
                
                if (p1.health < previousHealth)
                {
                    /*
                    serp.Action_Hurt();
                    */

                    //serp.healthToFlinch = serp.health;
                    //serp.Action_Hurt();
                }

                



                p1.health = serp.health / 10f;
                previousHealth = p1.health;
                
                previousShieldHealth = p1.shieldHealth;
                serp.shieldID = p1.shieldID;
                        
                p1.childRender.enabled = false;
                p1.GetComponent<Renderer>().enabled = false;
                p1.invincibilityTime = 999;
                        
                p1.attackPower = 0f;
                p1.attackHitstun = 0f;
                p1.attackEnemyInvTime = 0f;
                p1.attackKnockback.x = 0f;
                p1.attackKnockback.y = 0f;

                serp.onGround = p1.onGround;
                //p1.attackSfx = 0;

                serp.guardTime = p1.guardTime;
                if (p1.currentAnimation.Equals("Guard"))
                {
                    serp.SetPlayerAnimation("Hover");
                }

                if (serp.nextAttack == 7)
                {
                    serp.nextAttack = 0;
                }

                if (serp.health <= 0 
                    && serp.state != serp.State_Hurt
                    && (p1.state != p1.State_KO && p1.state != p1.State_CrushKO && p1.state != p1.State_KO_Recover))
                {
                    p1.health = -1;
                    p1.state = p1.State_KO;
                    p1.Action_Hurt();
                }

                if (p1.state == p1.State_KO_Recover)
                {
                    serp.health = p1.healthMax * 10;
                    SerpSetStateByPrivateMethodName(serp, "State_Recover");
                }

                if (p1.state == p1.State_CrushKO)
                {
                    serp.health = 0;
                }
            }

            serp.direction = p1.direction;

            /*
            if (p1.currentAnimation == "Jumping")
            {
                //SerpSetStateByPrivateMethodName(serp, "State_Jumping");
            }
            */
            
            UpdateSerpAnimFromPlayer(p1, serp);

            p1.GetInputFromPlayer1();
            serp.input = p1.input;

            if (serp.state == serp.State_Hurt)
            {
                // Can't act out of hurt frames.
            }
            else if (serp.state == serp.State_Init && serp.bossActivated)
            {
                serp.state = State_Serp_Physics_Idle;
            }
            else if (serp.input.attackPress)
            {
                if (serp.input.up)
                {
                    
                    if (p1.energy >= 100)
                    {
                        useEnergy = true;
                        serp.genericTimer = 1f;
                        //SerpSetStateByPrivateMethodName(serp, "State_DualCrash");
                        SerpStartDualGround(serp);
                    }
                }
                else if (serp.input.down)
                {
                    if (p1.energy >= 100)
                    {
                        useEnergy = true;
                        serp.genericTimer = 1f;
                        SerpStartDualAir(serp);
                    }

                    
                }
                else
                {
                    useEnergy = false;
                    serp.genericTimer = 1f;
                    //SerpSetStateByPrivateMethodName(serp, "State_Shooting");
                    SerpStartShooting(serp);
                }
            }
            else if (serp.input.attackHold)
            {
                serp.genericTimer -= FPStage.deltaTime;
            }
            else if (serp.input.specialPress)
            {
                serp.SetPlayerAnimation("Missile");
                serp.Action_FireRocket();
                useEnergy = false;
            }
            else if (p1.input.specialHold || p1.input.specialPress)
            {
                //serp.genericTimer = 1f;
                //SerpSetStateByPrivateMethodName(serp, "State_Missile");
                useEnergy = false;
            }
            else if (serp.input.jumpPress && p1.onGround && !p1.currentAnimation.Equals("Swimming"))
            {
                serp.SetPlayerAnimation("Jumping");
            }
            else if (serp.input.guardPress && p1.guardTime <= 0)
            {
                serp.SetPlayerAnimation("TopSpeed");
                serp.velocity.y = 0;
                serp.velocity.x += (15f * Mathf.Sign(serp.velocity.x));
            }
            else
            {
                if (p1.currentAnimation == "Running" 
                    || p1.currentAnimation == "TopSpeed"
                    || (p1.onGround && (serp.input.left || serp.input.right)))
                {
                    serp.genericTimer = 0;
                    //SerpSetStateByPrivateMethodName(serp, "State_Dash");
                    useEnergy = false;
                }
                /* Rewrite this to check against the player input, or check it after I've already set this animation myself.
                else if (p1.acceleration * p1.groundVel < 0 && Mathf.Abs(p1.groundVel) <= 2f) // Is slowing down check.
                {
                    serp.genericTimer = 1;
                    SerpSetStateByPrivateMethodName(serp, "State_Skid");
                    useEnergy = false;
                }*/
                else
                {
                    serp.genericTimer = 0;
                    serp.state = State_Serp_Physics_Idle;
                    useEnergy = false;
                }
            }
            
            if (serp.currentAnimation == "Dual_Air" || serp.currentAnimation == "Dual_Air_Loop")
            {
                if (FPCollision.CheckTerrainOOBB(serp, serp.hbTerrainCheck))
                {
                    p1.velocity.y = 2f;
                    p1.position.y = serp.position.y;
                }
                else
                {
                    p1.velocity.y = 0f;
                    p1.position.y = serp.position.y;
                }
            }
            
            if (serp.currentAnimation == "Dual_Air" || serp.currentAnimation == "Dual_Air_Loop" || serp.currentAnimation == "Dual_Ground")
            {
                useEnergy = true;

                p1.energy -= FPStage.deltaTime;
                serp.energy = p1.energy;
                
                if (p1.energy <= 0)
                {
                    //SerpSetStateByPrivateMethodName(serp, "State_Dash");
                    serp.state = serp.State_Idle;
                    serp.genericTimer = 0;
                    useEnergy = false;
                }
            }
        }

        public static void State_Serp_Physics_Idle()
        {
            var serp = CurrentActiveBossInstance.GetComponent<PlayerBossSerpentine>();
            var p1 = FPStage.currentStage.GetPlayerInstance_FPPlayer();
            
            if (serp.currentAnimation.Equals("Shooting"))
            {
                /*
                if (serp.animator.GetCurrentAnimatorStateInfo(0).normalizedTime > 0.8f)
                {
                    (serp.animator.GetCurrentAnimatorStateInfo(0).normalizedTime -= (8f / 34f));
                }
                */
                if (serp.animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1f)
                {
                    serp.SetPlayerAnimation("Laugh");
                }
            }

            serp.Process360Movement();
            if (serp.onGround)
            {
                serp.ApplyGroundForces();
                serp.angle = p1.angle;
                serp.groundAngle = p1.groundAngle;
                serp.angle = serp.groundAngle;
            }
            else
            {
                serp.ApplyAirForces();
                serp.ApplyGravityForce();
                serp.RotatePlayerUpright();
            }
            
            UpdateSerpAnimFromPlayer(p1, serp);
            
            
            switch (serp.DamageCheck())
            {
                case 1:
                    serp.flashTime = 2f;
                    break;
                case 2:
                    serp.activationMode = FPActivationMode.ALWAYS_ACTIVE;
                    serp.velocity.y = 4.5f;
                    break;
                case 4:
                    serp.state = serp.State_Frozen;
                    break;
            }
        }

        public static void UpdateSerpAnimFromPlayer(FPPlayer p1, PlayerBossSerpentine serp)
        {
            if (p1.targetGimmick != null)
            {
                //"In a snowball???"
                serp.invincibility = 10f;
            }
            else
            {
                
            }

            if (p1.onGround
                && !serp.input.attackPress
                && !serp.input.attackHold
                && !serp.input.specialPress
                && !serp.input.specialHold
                && (
                    !serp.currentAnimation.Equals("Shooting")
                    ))
            {
                if (Mathf.Abs(p1.groundVel) >= 6f) //14f
                {
                    serp.SetPlayerAnimation("TopSpeed");
                }
                else if (Mathf.Abs(p1.groundVel) < 6f 
                         && Mathf.Abs(p1.groundVel) > 0.3f)
                {
                    serp.SetPlayerAnimation("Walking");
                }
                else if (Mathf.Abs(p1.groundVel) <= 0.3f)
                {
                    serp.SetPlayerAnimation("Idle");
                }

                if (p1.groundVel > 0 && serp.input.left)
                {
                    serp.SetPlayerAnimation("Skidding");
                }
                else if (p1.groundVel < 0 && serp.input.right)
                {
                    serp.SetPlayerAnimation("Skidding");
                }

                serp.direction = p1.direction;
            }
            
            if (p1.currentAnimation.Equals("Swimming") 
                && !serp.currentAnimation.Equals("TopSpeed"))
            {
                serp.SetPlayerAnimation("TopSpeed");
            }
            
            else if (p1.currentAnimation.Equals("Victory")
                     && !serp.currentAnimation.Equals("Laugh"))
            {
                serp.SetPlayerAnimation("Laugh");
            }

            if (!previousAnimationName.Equals(p1.currentAnimation))
            {
                if ((p1.currentAnimation.Equals("Wall")
                    || p1.currentAnimation.Contains("Climbing"))
                    && !serp.currentAnimation.Equals("Hover"))
                {
                    serp.SetPlayerAnimation("Hover");
                }
            }
            previousAnimationName = p1.currentAnimation;
        }

        public static void NullifyPlayerSounds(FPPlayer fpp)
        {

            fpp.sfxJump = null;

            fpp.sfxDoubleJump = null;

            fpp.sfxSkid = null;

            fpp.sfxHurt = null;

            fpp.sfxKO = null;

            fpp.sfxRegen = null;

            fpp.sfxLilacBlink = null;

            fpp.sfxUppercut = null;

            fpp.sfxBoostCharge = null;

            fpp.sfxBoostLaunch = null;

            fpp.sfxBigBoostLaunch = null;

            fpp.sfxBoostRebound = null;

            fpp.sfxBoostExplosion = null;

            fpp.sfxDivekick1 = null;

            fpp.sfxDivekick2 = null;

            fpp.sfxCyclone = null;

            fpp.sfxCarolAttack1 = null;

            fpp.sfxCarolAttack2 = null;

            fpp.sfxCarolAttack3 = null;

            fpp.sfxPounce = null;

            fpp.sfxWallCling = null;

            fpp.sfxRolling = null;

            fpp.sfxMillaShieldSummon = null;

            fpp.sfxMillaShieldFire = null;

            fpp.sfxMillaSuperShield = null;

            fpp.sfxMillaCubeSpawn = null;

            fpp.sfxShieldBlock = null;

            fpp.sfxShieldHit = null;

            fpp.sfxShieldBreak = null;
            
            fpp.vaKO = null;
            
            fpp.sfxIdle = null;

            fpp.sfxMove = null;
                
            fpp.bgmResults = CurrentActiveBossInstance.GetComponent<PlayerBossSerpentine>().bgmBoss;

            fpp.audioChannel[4].mute = true;
            fpp.audioChannel[5].mute = true;
            
            
            /*
            if (fpp.vaAttack[0] == null && fpp.vaStart[0] == null)
            {
                return; // Exit this early if we think we've already nulled things out since this is loop-heavy.
            }
            */

            NullAllClipsInArray(fpp.vaAttack);
            NullAllClipsInArray(fpp.vaHardAttack);
            NullAllClipsInArray(fpp.vaSpecialA);
            NullAllClipsInArray(fpp.vaSpecialB);
            NullAllClipsInArray(fpp.vaHit);
            NullAllClipsInArray(fpp.vaIdle);
            NullAllClipsInArray(fpp.vaRevive);
            NullAllClipsInArray(fpp.vaStart);
            NullAllClipsInArray(fpp.vaItemGet);
            NullAllClipsInArray(fpp.vaClear);
            NullAllClipsInArray(fpp.vaJackpotClear);
            NullAllClipsInArray(fpp.vaLowDamageClear);
            NullAllClipsInArray(fpp.vaExtra);
        }
    
        public static void NullAllClipsInArray(AudioClip[] clips)
        {
            for (int i = 0; i < clips.Length; i++)
            {
                clips[i] = null;
            }
        }
    }
}

/*
 *CustomDamageInstance
 *DamageCheck_
 * ConfirmClassWithPoolTypeID
 */