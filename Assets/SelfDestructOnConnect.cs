using UnityEngine;
using Unity.Netcode;

public class SelfDestructOnConnect : NetworkBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        // Subscribe to the OnClientConnectedCallback event
        NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
    }

    public  override void OnDestroy()
    {
        base.OnDestroy(); // Call base method to ensure proper cleanup

        // It's important to unsubscribe when the object is destroyed to prevent memory leaks
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
        }
    }

    private void OnClientConnected(ulong clientId)
    {
        // Check if the connected client is the local player
        if (NetworkManager.Singleton.LocalClientId == clientId)
        {
            // Destroy this object once the connection is established
            Destroy(gameObject);
        }
    }
}