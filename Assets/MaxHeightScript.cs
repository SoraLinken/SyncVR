using System.Collections;
using System.Collections.Generic;
using UnityEngine;


// Help avoid the object to go above a certain height (fix problems with tall players floating)
public class MaxHeightScript : MonoBehaviour
{
    // The maximum height in the Y axis
    private float maxHeight = 1.68f;

    void Update()
    {
        Vector3 position = transform.position;

        // Clamp the Y position to the maximum height
        if (position.y > maxHeight)
        {
            position.y = maxHeight;
            transform.position = position;
        }
    }
}
