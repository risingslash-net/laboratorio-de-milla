using UnityEngine;

namespace RisingSlash.FP2Mods.RisingSlashCommon;

public class OnScreenTextUtil
{
    public static Font fpMenuFont;
    public static Material fpMenuMaterial;

    public static TextMesh CreateOnScreenText(string textToShow = "Waiting for text...")
    {
        TextMesh theTextMesh;
        //GameObject go = new GameObject("finna Show a Text");
        GameObject go = ClonePauseMenuText();
        if (go == null)
        {
            go = new GameObject("finna Show a Text");
        }
        else
        {
            go.name = "finna FOUND a text wow";
        }

        go.AddComponent<TextMesh>();
        theTextMesh = go.GetComponent<TextMesh>();

        if (fpMenuFont == null || fpMenuMaterial == null)
        {
            AttemptToFindFPFont();
        }

        theTextMesh.font = fpMenuFont;
        theTextMesh.GetComponent<MeshRenderer>().materials[0] = fpMenuMaterial;
        theTextMesh.characterSize = 10;
        theTextMesh.anchor = TextAnchor.UpperLeft;
        
        return theTextMesh;
    }
    
    public static void AttemptToFindFPFont()
    {
        if (fpMenuFont != null) return;

        foreach (var textMesh in Resources.FindObjectsOfTypeAll(typeof(TextMesh)) as TextMesh[])
            if (textMesh.font != null && textMesh.font.name.Equals("FP Menu Font"))
                //if (textMesh.font!= null && textMesh.font.name.Equals("FP Small Font Light"))
            {
                ConvenienceMethods.Log("Found the FP Menu Font loaded in memory. Saving reference.");
                //Log("Found the FP Small Font loaded in memory. Saving reference.");
                fpMenuFont = textMesh.font;
                fpMenuMaterial = textMesh.GetComponent<MeshRenderer>().materials[0];
                break;
            }
    }

    public static GameObject ClonePauseMenuText()
    {
        var goStageHUD = GameObject.Find("Stage HUD");
        //GameObject goStageHUD = GameObject.Find("Hud Pause Menu");
        if (goStageHUD == null) return null;

        ConvenienceMethods.Log("Successfully found HUD to attach text to.");

        //goStageHUD.energyBarGraphic.transform.parent;
        ConvenienceMethods.Log("Looking for Energy Bar");
        var temp = goStageHUD.GetComponent<FPHudMaster>();
        GameObject temp2;
        if (temp != null)
        {
            temp2 = temp.pfHudEnergyBar;
        }
        else
        {
            ConvenienceMethods.Log("This aint it.");
            return null;
        }
        
        var energyBarGraphic = Object.Instantiate(temp2, temp2.transform.parent);

        energyBarGraphic.transform.localScale *= 2;
        energyBarGraphic.SetActive(true);
        
        var tempGo = new GameObject();
        tempGo.transform.parent = energyBarGraphic.transform;
        tempGo.transform.localPosition = Vector3.zero;
        
        energyBarGraphic.transform.position = new Vector3(16, -80, 0);
        //energyBarGraphic = tempGo;
        
        GameObject output;
        //output = Object.Instantiate(goExampleText);
        output = tempGo;

        return output;
    }
}