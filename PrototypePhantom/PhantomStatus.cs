using System;
using UnityEngine;

namespace RisingSlash.FP2Mods.PrototypePhantom;

[Serializable]
public class PhantomStatus
{
    public int id = 0;
    public string anim = "";
    public float posX = 0;
    public float posY = 0;
    public float velX = 0;
    public float velY = 0;
    public float angle = 0;

    public string ToJson()
    {
        return JsonUtility.ToJson(this);
    }
    
    public static PhantomStatus FromJson(string json)
    {
        var stat = JsonUtility.FromJson<PhantomStatus>(json);
        return stat;
    }
}