using UnityEngine.SceneManagement;

namespace RisingSlash.FP2Mods.RisingSlashCommon;

public class SceneNamePair
{
    public string name = "";
    public string path = "PATH NOT SET";
    public Scene scene;

    public SceneNamePair(Scene theScene, string theName)
    {
        name = theName;
        scene = theScene;
        path = theName;
    }

    public SceneNamePair(Scene theScene, string theName, string thePath)
    {
        name = theName;
        scene = theScene;
        path = thePath;
    }

    public override string ToString()
    {
        return name + " ->" + path;
    }
}