using System;
using UnityEngine;

namespace RisingSlash.FP2Mods.PrototypePhantom;

[Serializable]
public class PhantomStatus
{
    //public int id = 0; // Number ID would be a good optimization but I don't want to work on that right now.
    public string cmd = "@UpPl";
    public string pName = "";
    public string pDisc = "";
    public string anim = "";
    public float posX = 0;
    public float posY = 0;
    public float velX = 0;
    public float velY = 0;
    public float angle = 0;
    public int charID = 0;

    public string ToJson()
    {
        return JsonUtility.ToJson(this);
    }
    
    public static PhantomStatus FromJson(string json)
    {
        var stat = JsonUtility.FromJson<PhantomStatus>(json);
        return stat;
    }

    public PhantomStatus(string cmd, string pName, string pDisc, string anim, float posX, float posY, float velX, float velY, float angle, int charID)
    {
        this.cmd = cmd;
        this.pName = pName;
        this.pDisc = pDisc;
        this.anim = anim;
        this.posX = posX;
        this.posY = posY;
        this.velX = velX;
        this.velY = velY;
        this.angle = angle;
        this.charID = charID;
    }
}