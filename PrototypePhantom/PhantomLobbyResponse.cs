using System.Collections.Generic;
using UnityEngine;
using Object = System.Object;

namespace RisingSlash.FP2Mods.PrototypePhantom;

[System.Serializable]
public class PlayerData
{
    public string player_name;
    public string player_discriminator;
    public int character_id;
    public bool ready;
    public string vote;
    //public string[] address;
    public string addressHost;
    public string addressPort;
    public int missed_keepalive_count;
    //public string[] mod_list;
}

[System.Serializable]
public class PlayersData
{
    //public Dictionary<string, PlayerData> players;
    public string[] players_keys = new string[0];
    public PlayerData[] players_values = new PlayerData[0];
}

[System.Serializable]
public class MapVotesData
{
    //public Dictionary<string, int> votes_for_map = new Dictionary<string, int>();
    public string[] votes_for_map_keys = new string[0];
    public int[] votes_for_map_values = new int[0];
}

[System.Serializable]
public class PhantomLobbyResponse
{
    public string status;
    //public PlayersData data;
    public int spectators;
    public MapVotesData map_votes;
    
    public string[] player_names;
    public string[] player_discriminators;
    public int[] character_ids;
    public bool[] readys;
    public string[] votes;
    public string[] addressHosts;
    public string[] addressPorts;
    public int[] missed_keepalive_counts;
    public string[] map_votes_keys;
    public int[] map_votes_values;

    public static string GenerateExampleJSON()
    {
        var example = new PhantomLobbyResponse();
        example.status = "ok";
        //example.data = new PlayersData();
        example.spectators = 0;
        example.player_names = new string[1];
        example.player_names[0] = "Phantom Chaser";
        example.player_discriminators = new string[1];
        example.player_discriminators[0] = "0000";
        example.character_ids = new int[1];
        example.character_ids[0] = 0;
        example.readys = new bool[1];
        example.readys[0] = false;
        example.votes = new string[1];
        example.votes[0] = "";
        example.addressHosts = new string[1];
        example.addressHosts[0] = "127.0.0.1";
        example.addressPorts = new string[1];
        example.addressPorts[0] = "7777";
        example.missed_keepalive_counts = new int[1];
        example.missed_keepalive_counts[0] = 0;

        example.map_votes_keys = new string[1];
        example.map_votes_keys[0] = "map1";
        example.map_votes_values = new int[1];
        example.map_votes_values[0] = 0;

        var json = JsonUtility.ToJson(example, true);
        return json;
    }
}

[System.Serializable]
public class PhantomLobbyResponseRoot
{
    public PhantomLobbyResponse response;
}
