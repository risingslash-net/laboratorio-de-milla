using System;
using UnityEngine;

namespace RisingSlash.FP2Mods.PrototypePhantom;

[Serializable]
public class PSCW
{
    public string command = "";
    public System.Object[] args = null;
}

[Serializable]
public class PhantomServerCommand
{
    public PSCW pscw = new PSCW();

    public string ToJson()
    {
        return JsonUtility.ToJson(pscw, true);
    }

    public static string AddPlayer(int lobbyID = 0, int roomID = 0
        , string playerName = "Phantom Chaser", string playerDiscriminator = "0000", int characterID = 0)
    {
        var temp = new PhantomServerCommand();
        temp.pscw.command = "add_player";
        temp.pscw.args = new System.Object[]{lobbyID, roomID, playerName, playerDiscriminator, characterID};
        return temp.ToJson();
    }
    
    public static string RemovePlayer(int lobbyID = 0, int roomID = 0
        , string playerName = "Phantom Chaser", string playerDiscriminator = "0000", int characterID = 0)
    {
        var temp = new PhantomServerCommand();
        temp.pscw.command = "remove_player";
        temp.pscw.args = new System.Object[]{lobbyID, roomID, playerName, playerDiscriminator, characterID};
        return temp.ToJson();
    }
    
    public static string KeepAlivePlayer(string playerName = "Phantom Chaser", string playerDiscriminator = "0000")
    {
        var temp = new PhantomServerCommand();
        temp.pscw.command = "keepalive_player";
        temp.pscw.args = new System.Object[]{playerName, playerDiscriminator};
        return temp.ToJson();
    }
    
    public static string GetRoomStatus(int lobbyID, int roomID)
    {
        var temp = new PhantomServerCommand();
        temp.pscw.command = "get_room_status";
        temp.pscw.args = new System.Object[]{lobbyID, roomID};
        return temp.ToJson();
    }

    public static void TestAddPlayer()
    {
        Debug.Log(AddPlayer());
    }
}