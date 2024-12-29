using UnityEngine;
using Unity.NetCode;

[UnityEngine.Scripting.Preserve]
public class GameBootstrap : ClientServerBootstrap
{
    public override bool Initialize(string defaultWorldName)
    {
        //code for testing
        //AutoConnectPort = 7979;
        //return base.Initialize(defaultWorldName);
        return false;
    }
}
