using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Netcode;

[System.Serializable]
public class NetworkAnimationInput
{
    public string animationPropertyName;
    public InputActionProperty action;
}

public class NetworkAnimateOnInput : NetworkBehaviour
{
    public List<AnimationInput> animationInputs;
    public Animator animator;

    // Update is called once per frame
    void Update()
    {
        if (IsOwner)
        {
            foreach (var item in animationInputs)
            {
                float actionValue = item.action.action.ReadValue<float>();
                animator.SetFloat(item.animationPropertyName, actionValue);
            }
        }

    }
}
