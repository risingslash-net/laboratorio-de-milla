using System;
using UnityEngine;

namespace RisingSlash.FP2Mods.PrototypePhantom;

public class PhantomPlayerTracker : MonoBehaviour
{
    public FPPlayer fpplayer;
    public float timeSinceTick = 0;
    public float tickTime = 1.0f / 30f; //Only update 30 times per second. _maybe_ allow 60. More than that is overkill.

    public void Update()
    {
        if (fpplayer == null)
        {
            fpplayer = gameObject.GetComponent<FPPlayer>();
        }
        else
        {
            timeSinceTick += Time.deltaTime;
            if (timeSinceTick < tickTime)
            {
                return;
            }

            var newStatus = new PhantomStatus("@UpPl", PrototypePhantom.playerName.Value, PrototypePhantom.playerDiscriminator
                , fpplayer.currentAnimation, fpplayer.position.y, fpplayer.position.y
                , fpplayer.velocity.x, fpplayer.velocity.y, fpplayer.angle, (int)fpplayer.characterID);
            ProtoPhanUDPDirector.Instance.SendData(JsonUtility.ToJson(newStatus));
            timeSinceTick -= tickTime;
        }
    }

    public static void BindToMainPlayer()
    {
        if (FPStage.currentStage == null || FPStage.currentStage.GetPlayerInstance() == null)
        {
            return;
        }

        var goPlayer = FPStage.currentStage.GetPlayerInstance().gameObject;
        var existingBinding = goPlayer.GetComponent<PhantomPlayerTracker>();
        if (existingBinding != null)
        {
            return;
        }

        goPlayer.AddComponent<PhantomPlayerTracker>();
    }
}