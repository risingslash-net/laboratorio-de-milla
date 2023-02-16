using System.Collections.Generic;
using UnityEngine;

namespace RisingSlash.FP2Mods.PrototypePhantom;

public class LivePhantom : FPBase360
{
    private Animator[] playerAnimators;
    public static Dictionary<string, LivePhantom> currentPhantoms;
    public string currentAnimation = "";
    public int charID = 0;
    public static void CachePlayerAnimators()
    {
        
    }

    public static void UpdatePlayer(PhantomStatus updatedStatus)
    {
        string playerKey = updatedStatus.pName + "#" + updatedStatus.pDisc;
        LivePhantom phantom = null;
        // if the currentPhantoms does not contain an object for this ID, create it;
        if (currentPhantoms == null)
        {
            currentPhantoms = new Dictionary<string, LivePhantom>();
        }
        if (!currentPhantoms.ContainsKey(playerKey))
        {
            phantom = LivePhantom.Instantiate(updatedStatus.charID, updatedStatus.pName, updatedStatus.pDisc);
        }
        else
        {
            phantom = currentPhantoms[playerKey];
        }

        phantom.position = new Vector2(updatedStatus.posX, updatedStatus.posY);
        phantom.velocity = new Vector2(updatedStatus.velX, updatedStatus.velY);
        phantom.angle = updatedStatus.angle;
        if (updatedStatus.dirR == 1)
        {
            phantom.direction = FPDirection.FACING_RIGHT;
        }
        else
        {
            updatedStatus.dirR = -1;
            phantom.direction = FPDirection.FACING_LEFT;
        }
        
        phantom.SetScale(new Vector3(updatedStatus.dirR, phantom.scale.y, phantom.scale.z));

        if (phantom.charID != updatedStatus.charID)
        {
            phantom.charID = updatedStatus.charID;
            var animator = phantom.GetComponent<Animator>();
            animator.runtimeAnimatorController = FPStage.player[phantom.charID].animator.runtimeAnimatorController;
        }

        phantom.SetAnimation(updatedStatus.anim);
        
        // if (!phantom.currentAnimation.Equals(updatedStatus.anim))
        // {
        // }

        //Update the status of the corresponding player ID's phantom to match the updated status info.
    }

    public void SetAnimation(string aniName, float aniPos = 0f, float aniChildPos = 0f, bool skipNameCheck = false,
        bool resetAnimationControlledVariables = true)
    {
        if (!skipNameCheck && !(currentAnimation != aniName))
        {
            return;
        }

        currentAnimation = aniName;
        var animator = gameObject.GetComponent<Animator>();
        animator.SetSpeed(1f);
        if (aniPos == 0f)
        {
            animator.Play(aniName);
        }
        else
        {
            animator.Play(aniName, -1, aniPos);
        }
    }

    public static LivePhantom Instantiate(int fpCharacterID = 0, string playerName = "Phantom", string playerDiscriminator = "")
    {
        if (FPStage.currentStage == null)
        {
            return null;
        }

        //var go = Instantiate(FPStage.player[fpCharacterID]);
        //var go = Instantiate(FPStage.currentStage.playerList[fpCharacterID].);
        bool playerObjectValidated = true;
        var fpPlayer = FPStage.InstantiateFPBaseObject(FPStage.player[fpCharacterID], out playerObjectValidated);
        var go = fpPlayer.gameObject;
        var phantom = go.AddComponent<LivePhantom>();
        phantom.position = fpPlayer.position;
        phantom.velocity = fpPlayer.velocity;
        phantom.angle = fpPlayer.angle;
        phantom.charID = fpCharacterID;

        phantom.useRotation = true;
        phantom.useScaling = true;
        phantom.terrainCollision = true;
        phantom.interactWithObjects = true; //Maybe don't?

        var renderer = go.GetComponent<SpriteRenderer>();
        renderer.color = new Color(1, 1, 1, 0.7f);
        
        Destroy(fpPlayer); // This resolves on next frame.

        if (currentPhantoms == null)
        {
            currentPhantoms = new Dictionary<string, LivePhantom>();
        }
        currentPhantoms.Add(playerName + "#" + playerDiscriminator, phantom);

        return phantom;
    }
}