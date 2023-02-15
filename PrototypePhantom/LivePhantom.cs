using UnityEngine;

namespace RisingSlash.FP2Mods.PrototypePhantom;

public class LivePhantom : FPBase360
{
    private Animator[] playerAnimators;
    private GameObject[] currentPhantoms;
    public static void CachePlayerAnimators()
    {
        
    }

    public void UpdatePlayer(int playerID, PhantomStatus updatedStatus)
    {
        // if the currentPhantoms does not contain an object for this ID, create it;
        
        //Update the status of the corresponding player ID's phantom to match the updated status info.
    }
}