using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;


// Handle the player's avatar after connection
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
            }
            else
            {
                yAvatar.SetActive(true);
            }
        }
        else
        {
            HandleOtherPlayerAvatarType();
        }
    }

    // Fetch the other player's avatar type
    void HandleOtherPlayerAvatarType()
    {
        StartCoroutine(APIClient.GetRequest($"/avatars/other?uniqueId={GameManager.uniqueId}&email={GameManager.email}", (data) =>
        {
            Debug.Log(data);
            var avatar = JsonUtility.FromJson<AvatarDeclaration>(data);
            if (avatar.avatarType == (int)GameManager.AvatarType.XBot)
            {
                xAvatar.SetActive(true);
                Renderer renderer = xAvatar.transform.Find("Renderer_Outfit_Top").GetComponent<Renderer>();
                if (ColorUtility.TryParseHtmlString("#923e94", out Color newColor))
                {
                    renderer.material.color = newColor;
                }
            }
            else
            {
                yAvatar.SetActive(true);
                Renderer renderer = yAvatar.transform.Find("Renderer_Outfit_Top").GetComponent<Renderer>();
                if (ColorUtility.TryParseHtmlString("#923e94", out Color newColor))
                {
                    renderer.material.color = newColor;
                }
            }
        }, (error) => Debug.Log("Error fetching avatar type")));
    }

    // Update visual position and rotation every frame (of the rig)
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
