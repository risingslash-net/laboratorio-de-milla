using BepInEx;
using BepInEx.Configuration;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace RisingSlash.FP2Mods.HalfGameSpeed
{
    [BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
    [BepInProcess("FP2.exe")]
    public class HalfGameSpeed : BaseUnityPlugin
    {
        private ConfigEntry<float> configGameSpeed;
        
        private void Awake()
        {
            // HalfGameSpeed startup logic
            InitConfigs();
        }

        private void InitConfigs()
        {
            configGameSpeed = Config.Bind("General",      // The section under which the option is shown
                "GameSpeed",  // The key of the configuration option in the configuration file
                0.5f, // The default value
                "Game speed to enforce. 1.0f is full (100%) speed. 0.5f is half (50%) speed. 0.33f is 1/3rd (33%) speed. 0.25f is 25% speed..."); // Description of the option to show in the config file
        }

        private void Update()
        {
            SetTimeScale(configGameSpeed.Value);
        }
        
        private void LateUpdate()
        {
            SetTimeScale(configGameSpeed.Value);
        }

        public void SetTimeScale(float timeScale)
        {
            if (FPStage.currentStage != null)
            {
                FPStage.SetTimeScale(timeScale);
                Time.timeScale = timeScale;
            }
        }
    }
}
