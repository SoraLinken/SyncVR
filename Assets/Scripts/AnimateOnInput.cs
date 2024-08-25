using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;


[System.Serializable]
public class AnimationInput
{
    public string animationPropertyName;
    public InputActionProperty action;
}


// Responsible for animating the player character based on input from the controllers (besides hand gestures).
public class AnimateOnInput : MonoBehaviour
{
    public List<AnimationInput> animationInputs;
    public Animator animator;

    void Update()
    {
        foreach (var item in animationInputs)
        {
            float actionValue = item.action.action.ReadValue<float>();
            animator.SetFloat(item.animationPropertyName, actionValue);
        }
    }
}
