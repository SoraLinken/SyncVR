using UnityEngine;
using Unity.Netcode;
using Unity.Collections;  // Ensure this namespace is included for Allocator and buffer usage

public class PingManager : NetworkBehaviour
{
    private float lastPingTime;

    private void Start()
    {
        // Check that the network manager is properly initialized and it's a client
        if (NetworkManager.Singleton != null && NetworkManager.Singleton.IsClient)
        {
            NetworkManager.Singleton.CustomMessagingManager.RegisterNamedMessageHandler("Pong", OnPongReceived);
            InvokeRepeating(nameof(SendPing), 1.0f, 2.0f);  // Ping every 2 seconds after a 1-second delay
        }
    }

    private void SendPing()
    {
        // Check the singleton instance and if it's a client
        if (NetworkManager.Singleton != null && NetworkManager.Singleton.IsClient)
        {
            lastPingTime = Time.realtimeSinceStartup;
            using (var writer = new FastBufferWriter(1, Allocator.Temp))  // Use a using statement to ensure disposal
            {
                writer.WriteValueSafe((byte)0);  // Just write a dummy byte
                // Corrected static member access
                NetworkManager.Singleton.CustomMessagingManager.SendNamedMessage("Ping", NetworkManager.ServerClientId, writer);
            }
        }
    }

    private void OnPongReceived(ulong senderClientId, FastBufferReader reader)
    {
        // Check the singleton instance and if the sender is the local client
        if (NetworkManager.Singleton != null && senderClientId == NetworkManager.Singleton.LocalClientId)
        {
            float rtt = (Time.realtimeSinceStartup - lastPingTime) * 1000f;  // Calculate RTT in milliseconds
            Debug.Log($"RTT: {rtt} ms");
        }
    }
}
