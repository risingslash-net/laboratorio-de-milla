using System;
using UnityEngine;

namespace RisingSlash.FP2Mods.PrototypePhantom;

[Serializable]
public class PSCW
{
    public string command = "";
    public string[] args = null;
}

[Serializable]
public class PhantomServerCommand
{
    public PSCW request = new PSCW();

    public string ToJson()
    {
        return JsonUtility.ToJson(request, true);
    }

    public static string AddPlayer(int lobbyID = 0, int roomID = 0
        , string playerName = "Phantom Chaser", string playerDiscriminator = "0000", int characterID = 0)
    {
        var temp = new PhantomServerCommand();
        temp.request.command = "add_player";
        temp.request.args = new string[]{lobbyID.ToString(), roomID.ToString(), playerName, playerDiscriminator, characterID.ToString()};
        return temp.ToJson();
    }
    
    public static string RemovePlayer(int lobbyID = 0, int roomID = 0
        , string playerName = "Phantom Chaser", string playerDiscriminator = "0000", int characterID = 0)
    {
        var temp = new PhantomServerCommand();
        temp.request.command = "remove_player";
        temp.request.args = new string[]{lobbyID.ToString(), roomID.ToString(), playerName, playerDiscriminator, characterID.ToString()};
        return temp.ToJson();
    }
    
    public static string KeepAlivePlayer(string playerName = "Phantom Chaser", string playerDiscriminator = "0000")
    {
        var temp = new PhantomServerCommand();
        temp.request.command = "keepalive_player";
        temp.request.args = new string[]{playerName, playerDiscriminator};
        return temp.ToJson();
    }
    
    public static string GetRoomStatus(int lobbyID, int roomID)
    {
        var temp = new PhantomServerCommand();
        temp.request.command = "get_room_status";
        temp.request.args = new string[]{lobbyID.ToString(), roomID.ToString()};
        return temp.ToJson();
    }

    public static void TestAddPlayer()
    {
        Debug.Log(AddPlayer());
    }
}