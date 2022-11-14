using System;
using System.Collections.Generic;
using System.IO;
using BepInEx;
using BepInEx.Configuration;
using RisingSlash.FP2Mods.RisingSlashCommon;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace RisingSlash.FP2Mods.DeadlyBread
{
    [BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
    [BepInProcess("FP2.exe")]
    public class DeadlyBread : BaseUnityPlugin
    {
        private string previousSceneName = "";
        private string currentSceneName = "";
        
        public static List<AssetBundle> loadedAssetBundles;
        public static bool assetsLoaded = false;
        
        public static UnityEngine.Object[] deadlyBreadAllSprites;
        public static Sprite deadlyBread;
        public static RuntimeAnimatorController deadlyBreadAnimatorController;

        public static List<int> enemyIds;

        public void Start()
        {
            enemyIds = new List<int>();
            loadedAssetBundles = new List<AssetBundle>();
        }

        public void Update()
        {
            if (FPStage.currentStage != null)
            {
                //EnforceTenMinuteTimerPenalty(FPStage.currentStage);
                previousSceneName = currentSceneName;
                currentSceneName = FPStage.currentStage.name;

                if (!currentSceneName.Equals(previousSceneName))
                {
                    OnSceneChange();
                }
            }
            else
            {
                previousSceneName = currentSceneName = "";
            }
            
            ReplaceBossAnimatorsGeneric(); // Expensive, but it'll probably only work if used every update.
        }

        private void CacheDeadlyBreadSpritesAndAnimators()
        {
            try
            {
                if (assetsLoaded && loadedAssetBundles.Count > 0)
                {
                    //deadlyBreadAllSprites = loadedAssetBundles[0].LoadAssetWithSubAssets(@"assets/ann/textures/danger bread-sheet.png");
                    deadlyBreadAllSprites = loadedAssetBundles[0].LoadAllAssets<Sprite>();
                    Debug.Log($"Trying to load deadlyBread, got {deadlyBreadAllSprites.Length} assets");
                    for (int i = 0; i < deadlyBreadAllSprites.Length; i++) {
                        if (deadlyBreadAllSprites[i] is Sprite)
                        {
                            Debug.Log("Found a suitable deadlyBread sprite.");
                            deadlyBread = (Sprite)deadlyBreadAllSprites[i];
                        }
                    }
                    
                    //deadlyBread = loadedAssetBundles[0].LoadAsset<Sprite>(@"assets/ann/textures/danger bread-sheet.png");
                    
                    Debug.Log("Loading DeadlyBreadController");
                    deadlyBreadAnimatorController = loadedAssetBundles[0].LoadAsset<RuntimeAnimatorController>(@"assets/ann/textures/anims/danger bread-sheet_0.controller");
                    if (deadlyBreadAnimatorController != null)
                    {
                        Debug.Log("deadlyBread Controller Loaded");
                    }
                    else
                    {
                        Debug.Log("deadlyBread Controller Load FAILED");
                    }
                    //deadlyBreadAnimatorAssets = loadedAssetBundles[0].LoadAssetWithSubAssets(@"assets/ann/textures/anims/danger bread-sheet_0.controller;");
                }
            }
            catch (Exception e)
            {
                Debug.Log("Some kind of exception when trying to store the deadlyBread stuff???");
                Debug.Log(e.StackTrace);
                assetsLoaded = false;
            }

            
        }

        public void OnSceneChange()
        {
            enemyIds.Clear();
            Debug.Log($"Scene Name Changed: {previousSceneName} -> {currentSceneName}");
            
            enemyIds.Clear();
            Debug.Log($"Enemy IDs cleared");
            
            LoadAssetBundlesFromModsFolder();
            CacheDeadlyBreadSpritesAndAnimators();
            ReplaceBossAnimatorsGeneric();
            ReplaceAnimatorsWithDeadlyBread();
        }

        public void ReplaceAnimatorsWithDeadlyBread()
        {
            GameObject replaceMe;
            replaceMe = GameObject.Find("GnawsaLock (Main)");
            
            var spriteRenderers = GetComponentsInChildren<SpriteRenderer>(); // works recursively by default.
            foreach (SpriteRenderer sr in spriteRenderers)
            {
                sr.sprite = deadlyBread;
            }
        }

        public void ReplaceBossAnimatorsGeneric()
        {
            var bossHuds = Transform.FindObjectsOfType<FPBossHud>();
            foreach (FPBossHud bossHud in bossHuds)
            {
                if (bossHud.targetBoss != null && !enemyIds.Contains(bossHud.targetBoss.objectID))
                {
                    var b = bossHud.targetBoss;
                    enemyIds.Add(b.objectID);
                    Debug.Log($"Removing ID{b.objectID} from replace list.");
                    
                    var animators = b.gameObject.GetComponentsInChildren<Animator>();
                    foreach (var animator in animators)
                    {
                        animator.runtimeAnimatorController = deadlyBreadAnimatorController;
                        //animator.enabled = false;
                    }
                    
                    var spriteRenderers = b.gameObject.GetComponentsInChildren<SpriteRenderer>();
                    foreach (var sr in spriteRenderers)
                    {
                        // animator.Controller = INSERT_DEADLY_BREAD_ANIMATOR_HERE;
                        sr.sprite = deadlyBread;
                    }
                    


                }
            }
        }

        //Deletme???
        public void ReplaceBubbleSprite()
        {
            var bubble = GameObject.Find("bubble");
            if (bubble == null)
            {
                return;
            }

            bubble.GetComponent<SpriteRenderer>().sprite = deadlyBread;

            GameObject tempGoChild;
            SpriteRenderer tempSpriteRenderer;
            
            for (int i = 0; i < bubble.transform.childCount; i++)
            {
                tempGoChild = bubble.transform.GetChild(i).gameObject;
                tempSpriteRenderer = tempGoChild.GetComponent<SpriteRenderer>();
                if (tempSpriteRenderer != null)
                {
                    tempSpriteRenderer.sprite = deadlyBread;
                }
            }
        }

        public static void   LoadAssetBundlesFromModsFolder()
        {
            try
            {
                if (assetsLoaded)
                {
                    return;
                }

                var pathApp = Application.dataPath;
                pathApp = Paths.PluginPath;
                //var pathMod = Path.Combine(Directory.GetParent(pathApp).FullName, "laboratorio_de_milla");
                
                var pathMod = Path.Combine(pathApp, "laboratorio_de_milla");
                var pathModAssetBundles = Path.Combine(pathMod, "assetbundles");

                var assetBundlePaths = Directory.GetFiles(pathModAssetBundles, "*.*");
                foreach (var abp in assetBundlePaths)
                {
                    Debug.Log(abp);
                    if (abp.Contains("."))
                    {
                        Debug.Log("Skipping this file, as it appears to have a " +
                            "file extension (.whatever) at the end, " +
                            "and is probably not an asset bundle.");
                        continue;
                    }

                    Debug.Log("finna gonna load from file");
                    var currentAB = AssetBundle.LoadFromFile(abp);
                    Debug.Log("finna gonna actually DID load from file");

                    if (currentAB == null)
                    {
                        Debug.Log("Failed to load AssetBundle. Bundle may already be loaded, or the file may be corrupt.");
                        continue;
                    }

                    //currentAB.LoadAllAssets(); //Uncomment if the scenes are still unloadable?
                    loadedAssetBundles.Add(currentAB);
                    Debug.Log("AssetBundle loaded successfully as loadedAssetBundles[" + (loadedAssetBundles.Count - 1) +
                        "]:");
                    Debug.Log("-----{AB Scene Paths:}-----");
                    Debug.Log(ExpandArrayToString(currentAB.GetAllScenePaths()));
                    Debug.Log("-----{AB Asset Names:}-----");
                    Debug.Log(ExpandArrayToString(currentAB.GetAllAssetNames()));
                }

                assetsLoaded = true;
            }
            catch (NullReferenceException e)
            {
                Debug.Log("Null reference exception when trying to load asset bundles for modding. Canceling.");
                Debug.Log(e.StackTrace);
                assetsLoaded = false;
            }
        }

        public static string ExpandArrayToString(string[] arr)
        {
            var str = "";
            foreach (var s in arr)
            {
                str = $"{str};\r\n {s} ({s.GetType().Name})";
            }

            return str;
        }

        public static void ExamineAssetBundleContents(AssetBundle assetBundle)
        {
            //var assets = assetBundle.LoadAllAssets();
        }
    }
}
