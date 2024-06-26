using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
public class NetworkPlayer : NetworkBehaviour
{
    public GameObject xAvatar;
    public GameObject yAvatar;
    public Transform root;
    public Transform head;
    public Transform leftHand;
    public Transform rightHand;

    private void Start()
    {
        xAvatar.SetActive(false);
        yAvatar.SetActive(false);
        if (IsOwner)
        {
            if (GameManager.avatarType == GameManager.AvatarType.XBot)
            {
                xAvatar.SetActive(true);
                yAvatar.SetActive(false);
            }
            else
            {
                xAvatar.SetActive(false);
                yAvatar.SetActive(true);
            }
        }
        else
        {
            HandleOtherPlayerAvatarType();
        }
    }

    void HandleOtherPlayerAvatarType()
    {
        StartCoroutine(APIClient.GetRequest($"/avatars/other?uniqueId={GameManager.uniqueId}&email={GameManager.email}", (data) =>
        {
            Debug.Log(data);
            var avatar = JsonUtility.FromJson<AvatarDeclaration>(data);
            if (avatar.avatarType == (int)GameManager.AvatarType.XBot)
            {
                xAvatar.SetActive(true);
                yAvatar.SetActive(false);
            }
            else
            {
                xAvatar.SetActive(false);
                yAvatar.SetActive(true);
            }
        }, (error) => Debug.Log("Error fetching avatar type")));
    }
    private void Update()
    {
        if (IsOwner && VRRigReferences.Singleton != null)
        {
            root.position = VRRigReferences.Singleton.root.position;
            root.rotation = VRRigReferences.Singleton.root.rotation;

            head.position = VRRigReferences.Singleton.head.position;
            head.rotation = VRRigReferences.Singleton.head.rotation;

            leftHand.position = VRRigReferences.Singleton.leftHand.position;
            leftHand.rotation = VRRigReferences.Singleton.leftHand.rotation;

            rightHand.position = VRRigReferences.Singleton.rightHand.position;
            rightHand.rotation = VRRigReferences.Singleton.rightHand.rotation;
        }
    }
}
