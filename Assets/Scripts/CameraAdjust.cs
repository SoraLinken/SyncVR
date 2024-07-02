using UnityEngine;

public class CameraAdjust : MonoBehaviour
{
    void Start()
    {
        Camera cam = GetComponent<Camera>();

        if (cam != null)
        {
            // Calculate the new projection matrix
            Matrix4x4 mat = cam.projectionMatrix;

            // Modify the projection matrix to shift the lens
            mat.m11 = 1.0f / Mathf.Tan(Mathf.Deg2Rad * cam.fieldOfView / 2.0f);
            mat.m00 = mat.m11 / cam.aspect;

            // Apply the modified matrix to the camera
            cam.projectionMatrix = mat;
        }
    }
}