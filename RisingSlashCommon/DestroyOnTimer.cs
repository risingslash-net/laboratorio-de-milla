using System;
using UnityEngine;

namespace RisingSlash.FP2Mods.RisingSlashCommon;

public class DestroyOnTimer : MonoBehaviour
{
    private float destroyTimer = 10f;

    public void Update()
    {
        destroyTimer -= Time.deltaTime;
        if (destroyTimer <= 1f)
        {
            var mr = gameObject.GetComponent<MeshRenderer>();
            if (mr != null)
            {
                mr.material.color = new Color(mr.material.color.r, mr.material.color.g, mr.material.color.b, Mathf.Max(destroyTimer, 0f));
            }
        }
        if (destroyTimer <= 0)
        {
            Destroy(gameObject);
        }
    }

    public void SetTimer(float seconds)
    {
        destroyTimer = seconds;
    }
}