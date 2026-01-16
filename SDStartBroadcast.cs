using FishNet.Broadcast;
using FishNet.Serializing;
using UnityEngine;

public struct SDStartBroadcast : IBroadcast
{
    public Vector3 center;

    public static void WriteSDStartBroadcast(Writer writer, SDStartBroadcast value)
    {
        writer.WriteVector3(value.center);
    }

    public static SDStartBroadcast ReadSDStartBroadcast(Reader reader)
    {
        SDStartBroadcast broadcast = new SDStartBroadcast();
        broadcast.center = reader.ReadVector3();
        return broadcast;
    }
}