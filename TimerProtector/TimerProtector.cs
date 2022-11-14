using System;
using BepInEx;
using BepInEx.Configuration;
using RisingSlash.FP2Mods.RisingSlashCommon;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace RisingSlash.FP2Mods.TimerProtector
{
    [BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
    [BepInProcess("FP2.exe")]
    public class TimerProtector : BaseUnityPlugin
    {      
        public void Update()
        {
            if (FPStage.currentStage != null)
            {
                EnforceTenMinuteTimerPenalty(FPStage.currentStage);
            }
        }

        private void EnforceTenMinuteTimerPenalty(FPStage fpStage)
        {
            if (fpStage.minutes < 10)
            {
                fpStage.minutes += 10;
            }
        }
    }
}
