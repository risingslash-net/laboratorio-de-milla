using BepInEx;
using BepInEx.Configuration;
using UnityEngine;

namespace RisingSlash.FP2Mods.PrototypePhantom
{
    [BepInPlugin("mods.risingslash.net.prototypephantom", "Prototype Phantom", "1.0.230216")]
    [BepInProcess("FP2.exe")]
    public class PrototypePhantom : BaseUnityPlugin
    {
        public static ConfigEntry<string> playerName;
        public static string playerDiscriminator = "0000";
        private void Awake()
        {
            Logger.LogInfo("mods.risingslash.net.prototypephantom is loaded!");
            playerDiscriminator = Random.Range(0, 9999).ToString().PadLeft(4, '0');
            
            playerName = Config.Bind("General",      // The section under which the option is shown
                "PlayerName",  // The key of the configuration option in the configuration file
                "PhantomChaser", // The default value
                "Your display name. Set it to anything you like, but keep it short. Longer names may slightly increase network lag."); // Description of the option to show in the config file
            
            ProtoPhanUDPDirector.Instantiate();
        }
    }
}
