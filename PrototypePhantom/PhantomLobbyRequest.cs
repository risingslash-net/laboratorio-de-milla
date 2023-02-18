using System.Collections.Generic;

namespace RisingSlash.FP2Mods.PrototypePhantom;

[System.Serializable]
public class PhantomLobbyRequest
{
    public string command;
    public string[] args = null;
}

[System.Serializable]
public class PhantomLobbyRequestRoot
{
    public PhantomLobbyRequest request = new PhantomLobbyRequest();
}
