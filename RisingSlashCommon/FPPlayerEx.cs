using System.Collections.Generic;
using UnityEngine;

namespace RisingSlash.FP2Mods.RisingSlashCommon;

public class FPPlayerEx : FPPlayer
{
    public FPPlayer manipulationTarget;
    public List<FPBaseEnemy> enemies;

    private new void Start()
    {
        base.Start();
    }
    
    public override void ResetStaticVars()
    {
        base.ResetStaticVars();
        classID = -1;
    }
    
    public void State_Init()
    {
        if (base.name == "Player 1 EX")
        {
            position = FPStage.checkpointPos;
            if (FPStage.checkpointFaceLeft)
            {
                direction = FPDirection.FACING_LEFT;
            }
        }
        lastGround = FPStage.checkpointPos;
        genericTimer += FPStage.deltaTime;
        if (genericTimer > 8f)
        {
            genericTimer = 0f;
            state = State_InAir;
        }
    }

    public void DealAutoDamageToAllVisibleEnemies(float attackPower = 1.0f, int characterID = 0)
    {
        enemies = FPStage.GetActiveEnemies();
        //while (FPStage.ForEach(NeeraFreeze.classID, objectRef)) // Object Ref being an FPBaseObject pointing to an enemy for the damage check.
        foreach (var ene in enemies)
        {
            var renderer = ene.GetComponent<SpriteRenderer>();
            if (renderer != null)
            {
                if (renderer.isVisible) //If this can be seen by _any_ camera, we will deal damage to it.
                {
                    FPStage.CreateStageObject(HitSpark.classID,
                        ene.position.x +
                        UnityEngine.Random.Range(-10f, 10f),
                        ene.position.y + //ene.hbWeakpoint.left
                        UnityEngine.Random.Range(-10f, 10f));
                    if (!ene.isHarmless)
                    {
                        ene.HealthDrain(fPPlayer.attackPower,
                            fPPlayer.characterID); // Gotta grab this method and call it elsewise
                    }

                    if (health <= 0f && !cannotBeKilled)
                    {
                        SetDeath(neeraFreeze.attackKnockback.x, 4.5f + neeraFreeze.attackKnockback.y);
                        deathSpinSpeed = 0f - Mathf.Max(Mathf.Abs(neeraFreeze.attackKnockback.x * 1.5f), 8f);
                        FPStage.ForEachBreak();
                        return 2;
                    }

                    FPAudio.PlayHitSfx(7);
                }
            }
        }
    }
}