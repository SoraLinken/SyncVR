using UnityEngine;


// Class to make a UI element (Game object) follow the camera rotation (The view of the player)
public class UIFollowCamera : MonoBehaviour
{
    private Camera userCamera;

    void Start()
    {
        userCamera = Camera.main;
        if (userCamera == null)
        {
            Debug.LogError("UIFollowCamera: No main camera found in scene.");
            return;
        }
    }

    void Update()
    {
        if (userCamera != null)
        {
            // Directly face the camera by looking at it
            transform.LookAt(userCamera.transform);
            transform.rotation = Quaternion.LookRotation(transform.position - userCamera.transform.position);
        }
    }
}