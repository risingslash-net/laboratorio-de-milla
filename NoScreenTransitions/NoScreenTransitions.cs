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
    public class NoScreenTransitions : BaseUnityPlugin
    {
        private string previousSceneName = "";
        private string currentSceneName = "";

        public float tmrFindRenderers = 2f;
        public float tmrFindRenderersReset = 10f;
        

        private SpriteRenderer[] renderers;

        public void Start()
        {
            
        }

        public void Update()
        {
            previousSceneName = currentSceneName;
            currentSceneName = SceneManager.GetActiveScene().name;
            if (previousSceneName != currentSceneName)
            {
                OnSceneChange();
            }

            /*
            if (renderers != null && renderers.Length > 0)
            {
                for (int i = 0; i < renderers.Length; i++)
                {
                    if (renderers[i] != null)
                    {
                        renderers[i].sprite = null;
                        //renderers[i].color = new Color(0f, 0f, 0f, 0f);
                    }
                }
            }
            */
        }

        public void UpdateTimer()
        {
            tmrFindRenderers -= Time.deltaTime;
            if (tmrFindRenderers <= 0)
            {
                BlankOutAllTransitions();
                tmrFindRenderers += tmrFindRenderersReset;
            }
        }

        public void OnSceneChange()
        {
            Debug.Log($"[NoScreenTransition] Scene Name Changed: {previousSceneName} -> {currentSceneName}");
            tmrFindRenderers = 2f;
            BlankOutAllTransitions();
        }

        private void BlankOutAllTransitions()
        {
            try
            {
                var screenTransitions = Transform.FindObjectsOfType<FPScreenTransition>();
                foreach (FPScreenTransition fpst in screenTransitions)
                {
                    renderers = fpst.gameObject.GetComponentsInChildren<SpriteRenderer>();
                    foreach (SpriteRenderer sr in renderers)
                    {
                        sr.sprite = null;
                        //sr.enabled = false;
                        //sr.color = new Color(0f, 0f, 0f, 0f);
                    }
                }
            }
            catch (Exception e)
            {
                Debug.Log(
                    $"[NoScreenTransition] Threw an exception when trying to remove the screen transition object?\r\n{e}");
            }
        }
    }
}
