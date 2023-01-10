using System;
using BepInEx.Logging;
using UnityEngine;

namespace RisingSlash.FP2Mods.RisingSlashCommon;

public static class ConvenienceMethods
{
    public static ManualLogSource LocalLog;
    public static bool bHasSceneChanged = false;
    
    public static void LogExceptionError(Exception e)
    {
        InitLogIfNecessary();
        LocalLog.LogError($"{MyPluginInfo.PLUGIN_GUID} Is logging an exception:\r\n{e.Message}\r\n{e.StackTrace}\r\n");
    }
    
    public static void Log(string str)
    {
        InitLogIfNecessary();
        LocalLog.LogInfo($"{MyPluginInfo.PLUGIN_GUID} {str}");
    }
    
    public static void Log(string plugin_guid, string str)
    {
        InitLogIfNecessary();
        LocalLog.LogInfo($"{plugin_guid} {str}");
    }
    
    public static void LogWarning(string plugin_guid, string str)
    {
        InitLogIfNecessary();
        LocalLog.LogWarning($"{plugin_guid} {str}");
    }
    
    public static void LogWarning(string str)
    {
        InitLogIfNecessary();
        LocalLog.LogWarning($"{MyPluginInfo.PLUGIN_GUID} {str}");
    }

    public static void InitLogIfNecessary()
    {
        if (LocalLog != null)
        {
            return;
        }

        LocalLog = new ManualLogSource("RisingSlashCommon");
    }

    public static bool HasSceneChanged()
    {
        return bHasSceneChanged;
    }
    
    public static void ShowPressedButtons()
    {
        foreach (KeyCode kcode in Enum.GetValues(typeof(KeyCode)))
        {
            if (Input.GetKeyDown(kcode))
                ConvenienceMethods.Log("KeyCode down: " + kcode);
        }
    }

    public static BadgeMessage ShowMessageAsBadge(string txt)
    {
        return ShowMessageAsBadge(txt, "", 0f - FPStage.badgeDisplayOffset);
    }
    
    public static BadgeMessage ShowMessageAsBadge(string txt, string txtHeader)
    {
        return ShowMessageAsBadge(txt, txtHeader, 0f - FPStage.badgeDisplayOffset);
    }

    public static BadgeMessage ShowMessageAsBadge(string txt, string txtHeader, float timer)
    {
        BadgeMessage badgeMessage = UnityEngine.Object.Instantiate(FPStage.currentStage.badgeMessage);
        badgeMessage.id = 0;
        badgeMessage.timer = 0f - FPStage.badgeDisplayOffset;
        badgeMessage.transform.localPosition += new Vector3(0f, Mathf.Ceil(FPStage.badgeDisplayOffset / 100f) % 3f * 64f, 0f);
        
        badgeMessage.badgeIcon.sprite = badgeMessage.badgeSprites[0];
        badgeMessage.badgeIcon.enabled = false;
        var goHeader = badgeMessage.transform.Find("Header");
        if (goHeader != null)
        {
            var header = goHeader.GetComponent<TextMesh>();
            header.text = txtHeader;
            LockTextMeshText(header);
        }
        else
        {
            ConvenienceMethods.Log("Couldn't find header in badge?");
        }

        var bodyText = badgeMessage.GetComponent<TextMesh>();
        if (bodyText != null)
        {
            bodyText.text = txt;
            LockTextMeshText(bodyText);
        }
        else
        {
            ConvenienceMethods.Log("Couldn't find TextMesh Component in badge????");
        }

        FPStage.badgeDisplayOffset += 100f;
        FPSaveManager.currentBadgeMessages.Add(badgeMessage);
       
        badgeMessage.BadgeTimerExpired += OnBadgeTimerExpired;
        return badgeMessage;
    }

    public static void OnBadgeTimerExpired(BadgeMessage badgeMessage)
    {
        badgeMessage.BadgeTimerExpired -= OnBadgeTimerExpired;
        FPSaveManager.currentBadgeMessages.Remove(badgeMessage);
    }

    public static void LockTextMeshText(TextMesh tm)
    {
        var ltm = tm.gameObject.AddComponent<LockTextMeshValue>();
        ltm.targetTextMesh = tm;
        ltm.text = tm.text;
    }

    public static void LockPlayerMovement(FPPlayer fpp)
    {
        fpp.state = fpp.State_Ball;
        fpp.targetGimmick = null;
    }
    
    public static void UnlockPlayerMovement(FPPlayer fpp)
    {
        fpp.state = fpp.State_Ball_Inert;
        fpp.targetGimmick = null;
        fpp.genericTimer = 0f;
    }
    
}