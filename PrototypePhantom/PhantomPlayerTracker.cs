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

            var dirR = 1;
            if (fpplayer.direction != FPDirection.FACING_RIGHT)
            {
                dirR = 0;
            }

            // Consider passing localScale instead of manually setting scale?
            var newStatus = new PhantomStatus("@UpPl", PrototypePhantom.playerName.Value, PrototypePhantom.playerDiscriminator
                , fpplayer.currentAnimation
                , (float)Math.Round(fpplayer.position.x, 2)
                , (float)Math.Round(fpplayer.position.y, 2)
                , (float)Math.Round(fpplayer.velocity.x, 2)
                , (float)Math.Round(fpplayer.velocity.y, 2)
                , (float)Math.Round(fpplayer.angle, 2)
                , dirR
                , (int)fpplayer.characterID);
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