using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode.Components;


// Custom NetworkTransform class for the client side
public class NetworkTransformClient : NetworkTransform
{
   protected override bool OnIsServerAuthoritative()
   {
       return false;
   }
}
