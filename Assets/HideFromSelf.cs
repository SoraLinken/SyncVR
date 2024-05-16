using UnityEngine;
using Unity.Netcode;

public class HideFromSelf : NetworkBehaviour
{
    void Start()
    {
        // Check if this NetworkObject is owned by the local player
        if (IsOwner)
        {
            gameObject.SetActive(false); // Hides the GameObject
        }
    }
}