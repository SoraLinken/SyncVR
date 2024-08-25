using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Represents a mapping between an animation property and an input action.
/// </summary>
[System.Serializable]
public class AnimationInput
{
    public string animationPropertyName;
    public InputActionProperty action;
}
/// <summary>
/// Controls animations based on input actions, mapping input values to animator parameters.
/// </summary>
public class AnimateOnInput : MonoBehaviour
{
    public List<AnimationInput> animationInputs;
    public Animator animator;

    // Update is called once per frame
    void Update()
    {
        // Iterate through each animation input and update the corresponding animator parameter
        foreach (var item in animationInputs)
        {
            // Read the input action's value as a float
            float actionValue = item.action.action.ReadValue<float>();

            // Set the animator parameter with the value from the input action
            animator.SetFloat(item.animationPropertyName, actionValue);
        }
    }
}
