using System;
using System.Collections.Generic;
using BepInEx;
using BepInEx.Configuration;
using RisingSlash.FP2Mods.RisingSlashCommon;
using UnityEngine;

namespace RisingSlash.FP2Mods.PowerUpSpawner
{
    [BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
    [BepInProcess("FP2.exe")]
    public class SplitScreenCamera : BaseUnityPlugin
    {
        public static ConfigEntry<string> PHKStartSplitscreen;
        public static bool hotkeysLoaded = false;

        public static FPPlayer currentPlayer = null;
        public static ItemFuel itemFuelReference = null;

        public static int numSplitScreenTargets = 1;
        public static List<FPBaseObject> cameraTargets; 
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
        
        public static void ToggleSplitScreen()
        {
            EnableSplitScreen.Value = !EnableSplitScreen.Value;
            ConvenienceMethods.Log("Toggle Splitscreen... NOT IMPLEMENTED YET.");
        }

        public static void StartSplitscreen()
        {
            foreach (var ssci in SplitScreenCameraInfos)
            {
                ssci.SplitCamRenderTexture.Release();
            }

            SplitScreenCameraInfos.Clear();

            try
            {
                var numPlayers = fpplayers.Count;
                //var sortedCameraTargets = fpplayers.OrderBy(fpp => fpp.characterID).ToList();
                var sortedCameraTargets = cameraTargets.OrderBy(ct => ct.characterID).ToList();
                EnableSplitScreen.Value = true;
                
                // Memo to go back and dispose these objects since we're creating new ones for player 1 as well...
                // Would also be nice to duplicate the HUD to give everyone their own...
                var goStageCamera = GameObject.Find("Stage Camera"); ConvenienceMethods.Log($"{goStageCamera}");
                var goRenderCamera = GameObject.Find("Render Camera"); ConvenienceMethods.Log($"{goRenderCamera}");
                var goPixelArtTarget = GameObject.Find("Pixel Art Target");  ConvenienceMethods.Log($"{goPixelArtTarget}");// has render cam as child object.
            
                var stageCamera = goStageCamera.GetComponent<FPCamera>(); ConvenienceMethods.Log($"{stageCamera}");
                var renderCamera = goRenderCamera.GetComponent<FPCameraFit>(); ConvenienceMethods.Log($"{renderCamera}");
                var pixelArtTarget = goPixelArtTarget.GetComponent<MeshRenderer>(); ConvenienceMethods.Log($"{pixelArtTarget}");
                
                
                
                for (int p = 0; p < numPlayers; p++) 
                {
                    var cameraRect = SplitScreenCamInfo.GetCamRectByPlayerIndexAndCount(p, numPlayers);
                    ConvenienceMethods.Log($"Rect: {cameraRect}");
                    // Short verison for first player.
                    /*
                    if (p == 0)
                    {
                        SplitScreenCameraInfos.Add(new SplitScreenCamInfo(stageCamera, goRenderCamera, stageCamera.renderTarget)); // First cam is pretty much guarenteed.
                        stageCamera.target = fpplayers[p];
                        stageCamera.targetPlayer = fpplayers[p];
                        goRenderCamera.GetComponent<Camera>().rect = new Rect(cameraRect);
                        continue;
                    }
                    */

                    var goSplitScreenPixelArtTarget = GameObject.Instantiate(goPixelArtTarget);  ConvenienceMethods.Log($"{goSplitScreenPixelArtTarget}");//shouldn't we be using the FPStage instantiate instead???
                    //var goSplitScreenRenderCamera = goSplitScreenPixelArtTarget.transform.Find("Render Camera (Clone)"); Log($"{goSplitScreenRenderCamera}");
                    //var goSplitScreenRenderCamera = GameObject.Find("Render Camera (Clone)"); Log($"{goSplitScreenRenderCamera}");
                    var goSplitScreenRenderCamera = goSplitScreenPixelArtTarget.transform.GetChild(0); ConvenienceMethods.Log($"{goSplitScreenRenderCamera}");
                    /*
                    for (int i = 0; i < goSplitScreenPixelArtTarget.transform.childCount; i++)
                    {
                        Log($"New Pixel Art Target Children: {goSplitScreenPixelArtTarget.transform.GetChild(i).gameObject.ToString()}");
                    }*/

                    var goSplitScreenStageCamera = GameObject.Instantiate(goStageCamera);
                    
                    //var splitScreenRenderCamera = goSplitScreenRenderCamera.GetComponent<FPCameraFit>(); 
                
                    var splitScreenStageCamera = goSplitScreenStageCamera.GetComponent<FPCamera>();

                    SplitScreenCameraInfos.Add(new SplitScreenCamInfo(splitScreenStageCamera, goSplitScreenRenderCamera.gameObject, splitScreenStageCamera.renderTarget));
                    
                    /*
                     *SplitScreenCameraInfos.Add(new SplitScreenCamInfo(stageCamera, goRenderCamera.GetComponent<Camera>()));
                    SplitScreenCameraInfos.Add(new SplitScreenCamInfo(splitScreenStageCamera, goSplitScreenRenderCamera.GetComponent<Camera>()));
                     * 
                     */
                    
                    splitScreenStageCamera.renderTarget = new RenderTexture(stageCamera.renderTarget.width, stageCamera.renderTarget.height, stageCamera.renderTarget.depth, stageCamera.renderTarget.format);
                    // Reminder: RenderTextures are not auto-disposed. I should probably create and cache these at the start for reuse throughout the game rather than creating them on the fly.
                    
                    
                    // Move down to not overlap.
                    goSplitScreenPixelArtTarget.transform.position +=
                        new Vector3(0, goPixelArtTarget.transform.localScale.y * (p + 1), 0); ConvenienceMethods.Log($"{goSplitScreenPixelArtTarget}");
                
                    // Set the material on the new render target to be unique and use the new renderTexture we just made.
                    goSplitScreenPixelArtTarget.GetComponent<MeshRenderer>().material.mainTexture =
                        splitScreenStageCamera.renderTarget;
                    
                    // Set the targets to the players
                    if (numPlayers > 1)
                    {
                        // StageCamera has a SetCameraTarget method, but it's static and assumes one camera so we don't use it.
                        splitScreenStageCamera.target = sortedCameraTargets[p];
                        splitScreenStageCamera.targetPlayer = sortedCameraTargets[p];
                        ConvenienceMethods.Log($"Set new target to sortedCameraTargets[p] p:{p} fpp: {splitScreenStageCamera.target} : {splitScreenStageCamera.target.name}");
                    }
                    else
                    {
                        splitScreenStageCamera.target = stageCamera.target; ConvenienceMethods.Log($"{splitScreenStageCamera}");
                        ConvenienceMethods.Log($"Set target to {splitScreenStageCamera.target}, the original player.");
                    }

                    //cameraRect = SplitScreenCamInfo.GetCamRectByPlayerIndexAndCount(p, numPlayers);

                    goSplitScreenRenderCamera.GetComponent<Camera>().rect = new Rect(cameraRect);
                    
                }

                if (numPlayers > 2)
                {
                    FPSaveManager.SetResolution(640 * 2, 360 * 2);
                }
                else if (numPlayers == 2)
                {
                    FPSaveManager.SetResolution(640, 360 * 2);
                }
                else
                {
                    FPSaveManager.SetResolution(640, 360 * 1);
                }

                /*goSplitScreenPixelArtTarget.transform.position +=
                    new Vector3(0, 360, 0); Log($"{goSplitScreenPixelArtTarget}");*/
                
                    //DEBUG
                    //goRenderCamera.GetComponent<Camera>().rect = new Rect(0, 0, 1, 1); Log($"{goRenderCamera.GetComponent<Camera>().rect}");
                    //goSplitScreenRenderCamera.GetComponent<Camera>().rect = new Rect(0, 0, 1, 1); Log($"{goSplitScreenRenderCamera.GetComponent<Camera>().rect}");

                    //END DEBUG
                
                // FPCamera.CreateNewCamera is used to make Lighting cameras, but I don't know when or where it's used so for now it's not factored into this. Fix later.

            }
            catch (Exception e)
            {
                ConvenienceMethods.LogExceptionError(e);
            }
        }

        public static void UpdateSplitScreens()
        {
            try
            {
                return;
                foreach (var camInfo in SplitScreenCameraInfos)
                {
                    camInfo.RenderCamera = camInfo.GoRenderCamera.GetComponent<Camera>();
                    ConvenienceMethods.Log($"UpdateRenderCam: {camInfo.RenderCamera}");
                    if (camInfo.FpCamera.lightingCamera != null)
                    {
                        camInfo.FpCamera.lightingCamera.rect = camInfo.RenderCamera.rect;
                        camInfo.FpCamera.lightingCamera.targetTexture = camInfo.SplitCamRenderTexture;
                    }


                    ParallaxLayer pl = null;
                    var highestLayerDepth = -1f;
                    int indexOfHighestLayerCam = -1;
                    bool flag = false;
                    
                    for (int pli = 0; pli < camInfo.FpCamera.parallaxLayers.Length; pli++)
                    {
                        // Set the camera rects to match player rect.
                        pl = camInfo.FpCamera.parallaxLayers[pli];
                        if (pl != null && pl.cam != null)
                        {
                            pl.cam.rect = camInfo.RenderCamera.rect;
                            pl.cam.targetTexture = camInfo.SplitCamRenderTexture; //Causes both views to stop clearing properly...

                            // Imitate CameraStart for handling Lighting and Foreground
                            if (pl.layerMask != StageLayerIDs.LIGHTING)
                            {
                                pl.cam.targetTexture = camInfo.FpCamera.renderTarget;
                                pl.cam.clearFlags = CameraClearFlags.Nothing;
                            }
                            else
                            {
                                pl.cam.targetTexture = camInfo.FpCamera.lightingTarget;
                                pl.cam.clearFlags = CameraClearFlags.Color;
                                pl.cam.backgroundColor = camInfo.FpCamera.shadowTint;
                                camInfo.FpCamera.lightingCamera = pl.cam;
                                flag = true;
                            }
                            if (pl.layerMask == StageLayerIDs.FG_PLANE)
                            {
                                pl.cam.cullingMask = 3856;
                            }
                            
                            // Get layer with highest depth.
                            if ( pl.cam.depth > highestLayerDepth)
                            {
                                highestLayerDepth = pl.cam.depth;
                                indexOfHighestLayerCam = pli;
                            }
                            
                            //UI Cam is affected by Lighting flag?
                            //... except the UI cam isn't accessible.
                            /*
                            if (!flag)
                            {
                                camInfo.FpCamera.uiCam.targetTexture = camInfo.FpCamera.renderTarget;
                                camInfo.FpCamera.uiCam.clearFlags = CameraClearFlags.Nothing;
                            }
                            else
                            {
                                camInfo.FpCamera.uiCam.targetTexture = camInfo.FpCamera.uiTarget;
                                camInfo.FpCamera.uiCam.clearFlags = CameraClearFlags.Color;
                                camInfo.FpCamera.uiCam.backgroundColor = Color.clear;
                            }
                            */
                        }
                        else
                        {
                            ConvenienceMethods.Log("funky parallax null");
                        }
                    }
                    pl = camInfo.FpCamera.parallaxLayers[indexOfHighestLayerCam];
                    pl.cam.clearFlags = CameraClearFlags.Color;
                    
                    
                    int num = -1;
                    float num2 = -1f;
                    for (int j = 0; j < camInfo.FpCamera.parallaxLayers.Length; j++)
                    {
                        //var pl = camInfo.FpCamera.parallaxLayers[j];
                        //pl.cam.rect = camInfo.RenderCamera.rect;
                        //pl.cam.targetTexture = camInfo.RenderCamera.targetTexture; //Causes both views to stop clearing properly...
                        
                        if (camInfo.FpCamera.parallaxLayers[j].layerDepth > num2)
                        {
                            num2 = camInfo.FpCamera.parallaxLayers[j].layerDepth;
                            num = j;
                        }
                    }
                    if (num > -1)
                    {
                        camInfo.FpCamera.parallaxLayers[num].cam.clearFlags = CameraClearFlags.Color;
                    }
                }
            }
            catch (Exception e)
            {
                ConvenienceMethods.LogExceptionError(e);
            }
        }

        public static Vector3 GetPositionRelativeToCamera(FPCamera cam, Vector3 pos)
        {
            return pos - cam.transform.position;
        }
    }
}
