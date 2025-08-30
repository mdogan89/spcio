using Fusion;
using UnityEngine;

public class NetworkPlayerList : NetworkBehaviour
{
    public static NetworkPlayerList Instance { get; private set; }

    [Networked]
    [Capacity(20)] // Sets the fixed capacity of the collection
    [UnitySerializeField] // Show this private property in the inspector.
    public NetworkDictionary<NetworkString<_32>, Color> PlayerNetworkDict { get; }

    [Networked, Capacity(10)]
    public NetworkArray<NetworkString<_32>> PlayerNetworkArray { get; }

    public override void Spawned()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogWarning($"Multiple {nameof(NetworkPlayerList)} detected. Destroying duplicate.");
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    public override void Despawned(NetworkRunner runner, bool hasState)
    {
        if (Instance == this)
            Instance = null;
    }
}
