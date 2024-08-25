using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode.Components;

// Custom Animator class for the client side
public class NetworkAnimatorClient : NetworkAnimator
{
    protected override bool OnIsServerAuthoritative()
    {
        return false;
    }
}
