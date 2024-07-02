using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StaticCameraRotation : MonoBehaviour
{
    public GameObject mainCamera; // Reference to the main camera
    public GameObject staticCamera; // Reference to the static camera (this camera)

    // Start is called before the first frame update
    void Start()
    {
        staticCamera.SetActive(false);
        StartCoroutine(SwitchCameraAfterDelay(40f)); // Start the coroutine to switch cameras after 30 seconds
    }

    // Coroutine to switch cameras after a delay
    IEnumerator SwitchCameraAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

        // Disable the main camera
        if (mainCamera != null)
        {
            mainCamera.GetComponent<Camera>().enabled = false;
        }

        // Enable the static camera
        if (staticCamera != null)
        {
            staticCamera.SetActive(true);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
