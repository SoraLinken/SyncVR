using System.Collections;
using System.Collections.Generic;
using UnityEngine;


// Hold position and rotation of the VR rig components, acts as a global reference
public class VRRigReferences : MonoBehaviour
{
    public static VRRigReferences Singleton;
    public Transform root;
    public Transform head;
    public Transform leftHand;
    public Transform rightHand;

    private void Awake(){
        Singleton = this;
    }
}
