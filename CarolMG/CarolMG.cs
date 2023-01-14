using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using BepInEx;
using BepInEx.Configuration;
using MonoMod.Utils;
using RisingSlash.FP2Mods.RisingSlashCommon;
using UnityEngine;

namespace RisingSlash.FP2Mods.CarolMG
{
    [BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
    [BepInDependency("RisingSlashCommon")]
    [BepInProcess("FP2.exe")]
    public class CarolMG : BaseUnityPlugin
    {
        
        public static ConfigEntry<float> configTopSpeed;
        public static ConfigEntry<float> configAcceleration;
        public static ConfigEntry<float> configAccelerationAir;
        public static ConfigEntry<bool> configNoBike;

        public float tmrNoBike = 0f;
        public Sprite spriteCarolMG;

        public List<FPBaseEnemy> debugEnemies;
        
        private void Awake()
        {
            // PluginGreetingsAvalice startup logic
            Logger.LogInfo($"{MyPluginInfo.PLUGIN_NAME} {MyPluginInfo.PLUGIN_GUID} is loaded!");
            InitConfigs();
        }

        public void Update()
        {
            if (FPStage.currentStage == null || FPStage.currentStage.GetPlayerInstance() == null)
            {
                return;
            }

            var fpp = ApplyCarolStats();

            if (configNoBike.Value)
            {
                if (tmrNoBike <= 0)
                {
                    var fuel = FPStage.FindObjectOfType<ItemFuel>();
                    if (fuel != null)
                    {
                        FPStage.DestroyStageObject(fuel);
                    }
    
                    if (fpp.characterID == FPCharacterID.BIKECAROL)
                    {
                        fpp.characterID = FPCharacterID.CAROL;
                    }

                    tmrNoBike = 10f;
                }
                else
                {
                    tmrNoBike -= Time.deltaTime;
                }
            }

        }

        public void LateUpdate()
        {
            if (FPStage.currentStage == null || FPStage.currentStage.GetPlayerInstance() == null)
            {
                return;
            }

            ApplyCarolStats();
        }

        public FPPlayer ApplyCarolStats()
        {
            var fpp = FPStage.currentStage.GetPlayerInstance_FPPlayer();
            if (fpp.characterID == FPCharacterID.CAROL)
            {
                fpp.hbAttack.visible = true;
                fpp.hbAttack.enabled = true;
                fpp.topSpeed = configTopSpeed.Value;
                fpp.acceleration = configAcceleration.Value;
                fpp.airAceleration = configAccelerationAir.Value;

                fpp.animator.enabled = false;

                if (fpp.state == fpp.State_Ground)
                {
                    fpp.state = State_Ground_NoCap;
                }


                //fpp.SetPlayerAnimation("Whoa");

                if (fpp.velocity.magnitude >= 10f)
                {
                    fpp.attackStats = AttackStats_CarolMGFast;
                }
                else
                {
                    fpp.attackStats = AttackStats_CarolMGSlow;
                }
                ManipulateCarolSprite(fpp);
            }

            return fpp;
        }
        
        public Sprite ManipulateCarolSprite(FPPlayer fpp)
        {
            var spriteRenderer = fpp.GetComponent<SpriteRenderer>();
            var oldSprite = spriteRenderer.sprite;
            var newSprite = Sprite.Create(oldSprite.texture, oldSprite.rect, oldSprite.pivot, 1.0f);
            
            
            /*
            var field = newSprite.GetType().GetField("<textureRect>k__BackingField", BindingFlags.Instance | BindingFlags.NonPublic);
            field.SetValue(newSprite, new Rect(409, 1456, 57, 67));

            //newSprite.textureRect = new Rect();
            //newSprite.textureRectOffset = Vector2.down;
            
            //newSprite.uv = oldSprite.texture;

            spriteRenderer.sprite = newSprite;
            */

            if (spriteCarolMG == null)
            {
                spriteCarolMG = LoadCarolMGSprite();
            }

            if (spriteCarolMG != null)
            {
                spriteRenderer.sprite = spriteCarolMG;
                fpp.childRender.enabled = false;
            }

            return newSprite;
        }

        public Sprite LoadCarolMGSprite()
        {
            var sprite = new Sprite();
            Logger.LogInfo(Path.Combine(Paths.PluginPath, "CarolMG/carolmg"));
            var assetBundle = ConvenienceMethods.LoadAssetBundleAssetsFromPath(Path.Combine(Paths.PluginPath, "CarolMG/carolmg"));
            if (assetBundle != null)
            {
                sprite = assetBundle.LoadAsset<Sprite>("carolmg");
            }

            return sprite;
        }

        public void AttackStats_CarolMGFast()
        {
            var fpp = FPStage.currentStage.GetPlayerInstance_FPPlayer();
            fpp.attackPower = 20f * fpp.velocity.magnitude;
            fpp.attackHitstun = Mathf.Min(0.3f * fpp.velocity.magnitude, 10f);
            fpp.attackEnemyInvTime = 0f;
            fpp.attackKnockback.x = Mathf.Max(Mathf.Abs(fpp.prevVelocity.x * 1.5f), 6f);
            if (fpp.direction == FPDirection.FACING_LEFT)
            {
                fpp.attackKnockback.x = 0f - fpp.attackKnockback.x;
            }
            fpp.attackKnockback.y = fpp.prevVelocity.y * 0.5f;
            fpp.attackSfx = 7;
            fpp.attackPower *= fpp.GetAttackModifier();
            
            fpp.hbAttack.enabled = true;
            fpp.hbHurt.enabled = false;

            fpp.hbAttack.top = fpp.hbTouch.top + 4;
            fpp.hbAttack.right = fpp.hbTouch.right + 4;
            fpp.hbAttack.bottom = fpp.hbTouch.bottom - 4;
            fpp.hbAttack.left = fpp.hbTouch.left - 4;

            fpp.hbHurt = fpp.hbAttack;
            
            
            //fpp.hbAttack.visible = true;
            //fpp.hbHurt.visible = true;
            

            debugEnemies = FPStage.GetActiveEnemies();
            PlantBlock plant;
            foreach (var enemy in debugEnemies)
            {
                plant = enemy.GetComponent<PlantBlock>();
                if (plant != null)
                {
                    continue;
                    Logger.LogInfo("plant");
                    
                    if (plant.transform.GetComponent<SpriteRenderer>().isVisible)
                    {
                        Logger.LogInfo("dist");
                        /*Vector2.Distance(enemy.position, fpp.position) <= 70f
                        ||
                        */
                        //FPCollision.CheckOOBB(fpp, fpp.hbTouch, plant, plant.hbWeakpoint)
                        var methodInfoPlantDeath =
                            plant.GetType().GetMethod("State_Death", BindingFlags.Instance | BindingFlags.NonPublic);
                        plant.state = (FPObjectState)methodInfoPlantDeath.CreateDelegate(typeof(FPObjectState), plant);
                        plant.GetComponent<BoxCollider2D>().enabled = false;
                        FPAudio.PlayStaticSfx(plant.sfxHit);
                        Logger.LogInfo("hit");
                        
                        /*
                         // No clue why this doesn't work.
                        var proxDamage = new FPBaseEnemy.CustomDamageInstance(fpp, fpp.faction, fpp.attackPower,
                            fpp.attackEnemyInvTime);
                        enemy.RegisterCustomDamageInstance(proxDamage);
                        */
                    }
                }

                break;
            }
        }

        public void AttackStats_CarolMGSlow()
        {
            var fpp = FPStage.currentStage.GetPlayerInstance_FPPlayer();
            fpp.attackPower = 0.001f;
            fpp.attackHitstun = 3f;
            fpp.attackEnemyInvTime = 6f;
            fpp.attackKnockback.x = Mathf.Max(Mathf.Abs(fpp.prevVelocity.x * 1.5f), 6f);
            if (fpp.direction == FPDirection.FACING_LEFT)
            {
                fpp.attackKnockback.x = 0f - fpp.attackKnockback.x;
            }
            fpp.attackKnockback.y = fpp.prevVelocity.y;
            fpp.attackSfx = 21;
            fpp.attackPower *= fpp.GetAttackModifier();
            
            fpp.hbAttack.enabled = false;
            fpp.hbHurt.enabled = true;
        }

        public void State_Ground_NoCap()
        {
            var fpp = FPStage.currentStage.GetPlayerInstance_FPPlayer();
            var groundVelPreCap = fpp.groundVel;
            fpp.State_Ground();
            if (Math.Abs(fpp.groundVel) >= 24f
                || Math.Abs(groundVelPreCap) >= 24f)
            {
                fpp.groundVel = groundVelPreCap;
                var speedMultField = fpp.GetType()
                    .GetField("speedMultiplier", BindingFlags.Instance | BindingFlags.NonPublic);
                float speedMult = (float)speedMultField.GetValue(fpp);
                
                if (fpp.input.left)
                {
                    fpp.groundVel -= fpp.acceleration * speedMult * FPStage.deltaTime;
                }
                if (fpp.input.right)
                {
                    fpp.groundVel += fpp.acceleration * speedMult * FPStage.deltaTime;
                }
            }
        }

        public void InitConfigs()
        {
            configTopSpeed = Config.Bind("General",      // The section under which the option is shown
                "TopSpeed",  // The key of the configuration option in the configuration file
                (7.5f * 100f), // The default value
                "Set the max speed. Default is 100 times Carol's natural."); // Description of the option to show in the config file
            
            configAcceleration = Config.Bind("General",      // The section under which the option is shown
                "Acceleration",  // The key of the configuration option in the configuration file
                (0.110625f * 5), // The default value
                "Set the ground acceleration. Default is 5 times Carol's natural."); // Description of the option to show in the config file
            
            configAccelerationAir = Config.Bind("General",      // The section under which the option is shown
                "AccelerationAir",  // The key of the configuration option in the configuration file
                (0.22125f * 5), // The default value
                "Set the air acceleration. Default is 5 times Carol's natural."); // Description of the option to show in the config file
            
            configNoBike = Config.Bind("General",      // The section under which the option is shown
                "NoBike",  // The key of the configuration option in the configuration file
                true, // The default value
                "Toggle this on to disable using the Bike."); // Description of the option to show in the config file
            
        }
    }
}
