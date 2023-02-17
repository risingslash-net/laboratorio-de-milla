using System;
using System.Collections.Generic;

namespace RisingSlash.FP2Mods.PrototypePhantom;

[System.Serializable]
public class PlayerData
{
    public string player_name;
    public string player_discriminator;
    public int character_id;
    public bool ready;
    public object vote;
    public List<Object> address;
    public int missed_keepalive_count;
    public List<string> mod_list;
}

[System.Serializable]
public class PlayersData
{
    public Dictionary<string, PlayerData> players;
}

[System.Serializable]
public class MapVotesData
{
    public Dictionary<string, int> votes_for_map = new Dictionary<string, int>();
}

[System.Serializable]
public class PhantomLobbyResponse
{
    public string status;
    public PlayersData data;
    public int spectators;
    public MapVotesData map_votes;
}
