using Fusion;
using System.Collections;
using UnityEngine;

public class NetworkRunnerCloudConnectionLost : MonoBehaviour
{
    private void Start()
    {
        NetworkRunner.CloudConnectionLost += OnCloudConnectionLost;
    }

    private void OnCloudConnectionLost(NetworkRunner runner, ShutdownReason reason, bool reconnecting)
    {
        Debug.Log($"Cloud Connection Lost: {reason} (Reconnecting: {reconnecting})");

        if (!reconnecting)
        {
            // Handle scenarios where reconnection is not possible
            // e.g., notify the user, attempt manual reconnection, etc.
        }
        else
        {
            // Wait for automatic reconnection
            StartCoroutine(WaitForReconnection(runner));
        }
    }

    private IEnumerator WaitForReconnection(NetworkRunner runner)
    {
        yield return new WaitUntil(() => runner.IsInSession);
        Debug.Log("Reconnected to the Cloud!");
    }
}