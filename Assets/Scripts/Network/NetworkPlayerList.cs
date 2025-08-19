using Fusion;
using UnityEngine;

public class NetworkPlayerList : NetworkBehaviour
{
    [UnitySerializeField]
    [Networked, Capacity(10)]
    public NetworkArray<NetworkString<_32>> PlayerNetworkArray { get; }
}
