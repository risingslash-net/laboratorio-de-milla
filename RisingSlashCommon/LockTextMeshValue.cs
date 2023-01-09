using System;
using UnityEngine;

namespace RisingSlash.FP2Mods.RisingSlashCommon;

public class LockTextMeshValue : MonoBehaviour
{
    public TextMesh targetTextMesh;
    public string text = "";

    public void LateUpdate()
    {
        if (targetTextMesh != null)
        {
            targetTextMesh.text = text;
        }
    }
}