using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MaxHeightScript : MonoBehaviour
{
    // The maximum height in the Y axis
    private float maxHeight = 1.68f;

    // Update is called once per frame
    void Update()
    {
        // Get the current position of the GameObject
        Vector3 position = transform.position;

        // Clamp the Y position to the maximum height
        if (position.y > maxHeight)
        {
            position.y = maxHeight;
            transform.position = position;
        }
    }
}
