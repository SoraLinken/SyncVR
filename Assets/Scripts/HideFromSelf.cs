using UnityEngine;
using Unity.Netcode;


// Hides the attached GameObject if it is owned by the local player
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