using UnityEngine;

[System.Serializable]
public class VRMap
{
    public Transform vrTarget;
    public Transform ikTarget;
    public Vector3 trackingPositionOffset;
    public Vector3 trackingRotationOffset;

    // Hold VR target position and rotation, defaults to current player position and rotation on mount
    public void Map()
    {
        ikTarget.position = vrTarget.TransformPoint(trackingPositionOffset); 
        ikTarget.rotation = vrTarget.rotation * Quaternion.Euler(trackingRotationOffset);
    }
}

public class IKTargetFollowVRRig : MonoBehaviour
{
    [Range(0,1)]
    public float turnSmoothness = 0.1f; // Smoothen animation of turning around
    public VRMap head; // Postion and rotation of the head
    public VRMap leftHand;
    public VRMap rightHand;

    public Vector3 headBodyPositionOffset; // Offset of the head from the body (corrects head position)
    public float headBodyYawOffset; // Leftover, we don't use this

    

    // Update the position and rotation of the head, left hand and right hand every frame
    void LateUpdate()
    {
        transform.position = head.ikTarget.position + headBodyPositionOffset;
        float yaw = head.vrTarget.eulerAngles.y;
        transform.rotation = Quaternion.Lerp(transform.rotation,Quaternion.Euler(transform.eulerAngles.x, yaw, transform.eulerAngles.z),turnSmoothness);

        head.Map();
        leftHand.Map();
        rightHand.Map();
    }
}
