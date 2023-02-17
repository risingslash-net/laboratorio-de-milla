using BepInEx;
using BepInEx.Configuration;
using UnityEngine;

namespace RisingSlash.FP2Mods.PrototypePhantom
{
    [BepInPlugin("mods.risingslash.net.prototypephantom", "Prototype Phantom", "1.0.230216")]
    [BepInDependency("net.risingslash.fp2mods.risingslashcommon", "1.1.230127")]
    [BepInProcess("FP2.exe")]
    public class PrototypePhantom : BaseUnityPlugin
    {
        public static ConfigEntry<string> playerName;
        public static ConfigEntry<string> lobbyServers;
        public static string playerDiscriminator = "0000";
        private void Awake()
        {
            Logger.LogInfo("mods.risingslash.net.prototypephantom is loaded!");
            playerDiscriminator = Random.Range(0, 9999).ToString().PadLeft(4, '0');
            
            playerName = Config.Bind("General",      // The section under which the option is shown
                "PlayerName",  // The key of the configuration option in the configuration file
                "PhantomChaser", // The default value
                "Your display name. Set it to anything you like, but keep it short. Longer names may slightly increase network lag."); // Description of the option to show in the config file
            
            lobbyServers = Config.Bind("General",      // The section under which the option is shown
                "LobbyServers",  // The key of the configuration option in the configuration file
                "phantomchase.risingslash.net:20232,127.0.0.1:20232", // The default value
                "A comma-separated list of ipaddresses and ports that host lobby servers to use for matching up with other players. The first one listed will be joined by default."); // Description of the option to show in the config file
            
            var director = ProtoPhanUDPDirector.Instantiate();
            foreach (var lobbyConnectionString in lobbyServers.Value.Split(','))
            {
                if (lobbyConnectionString.IsNullOrWhiteSpace())
                {
                    continue;
                }

                var connectInfo = lobbyConnectionString.Trim().Split(':');
                ProtoPhanUDPDirector.AddLobbyServer(connectInfo[0], int.Parse(connectInfo[1]));
                ProtoPhanUDPDirector.AddLobbyServer(connectInfo[0], int.Parse(connectInfo[1]));
            }
        }
    }
}
