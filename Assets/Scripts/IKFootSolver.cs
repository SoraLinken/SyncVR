using System.Collections;
using System.Collections.Generic;
using UnityEngine;


// Responsible for solving the inverse kinematics of the feet of the player character.
public class IKFootSolver : MonoBehaviour
{
    public bool isMovingForward;

    [SerializeField] LayerMask terrainLayer = default;
    [SerializeField] Transform body = default;
    [SerializeField] IKFootSolver otherFoot = default;
    [SerializeField] float speed = 4; // Walking speed
    [SerializeField] float stepDistance = .2f; // Distance between steps
    [SerializeField] float stepLength = .2f; // How much time the foot is in the air when moving forward
    [SerializeField] float sideStepLength = .1f; // How much time the foot is in the air when moving sideways

    [SerializeField] float stepHeight = .3f; // How high the foot goes when walking
    [SerializeField] Vector3 footOffset = default; // Offset of the foot in all directions

    public Vector3 footRotOffset;
    public float footYPosOffset = 0.1f; // Offset of the foot from the ground

    public float rayStartYOffset = 0; // Offset of the ray (helps in solving direction) the foot
    public float rayLength = 1.5f; // Length of the ray
    
    float footSpacing;
    Vector3 oldPosition, currentPosition, newPosition;
    Vector3 oldNormal, currentNormal, newNormal;
    float lerp;


    // Setup the initial values based on player position
    private void Start()
    {
        footSpacing = transform.localPosition.x;
        currentPosition = newPosition = oldPosition = transform.position;
        currentNormal = newNormal = oldNormal = transform.up;
        lerp = 1;
    }

    // Update the foot position based on the raycast hit every frame
    void Update()
    {
        transform.position = currentPosition + Vector3.up * footYPosOffset;
        transform.localRotation = Quaternion.Euler(footRotOffset);

        Ray ray = new Ray(body.position + (body.right * footSpacing) + Vector3.up * rayStartYOffset, Vector3.down);

        Debug.DrawRay(body.position + (body.right * footSpacing) + Vector3.up * rayStartYOffset, Vector3.down);
            
        if (Physics.Raycast(ray, out RaycastHit info, rayLength, terrainLayer.value))
        {
            if (Vector3.Distance(newPosition, info.point) > stepDistance && !otherFoot.IsMoving() && lerp >= 1)
            {
                lerp = 0;
                Vector3 direction = Vector3.ProjectOnPlane(info.point - currentPosition,Vector3.up).normalized;

                float angle = Vector3.Angle(body.forward, body.InverseTransformDirection(direction));

                isMovingForward = angle < 50 || angle > 130;

                if(isMovingForward)
                {
                    newPosition = info.point + direction * stepLength + footOffset;
                    newNormal = info.normal;
                }
                else
                {
                    newPosition = info.point + direction * sideStepLength + footOffset;
                    newNormal = info.normal;
                }

            }
        }

        if (lerp < 1)
        {
            Vector3 tempPosition = Vector3.Lerp(oldPosition, newPosition, lerp);
            tempPosition.y += Mathf.Sin(lerp * Mathf.PI) * stepHeight;

            currentPosition = tempPosition;
            currentNormal = Vector3.Lerp(oldNormal, newNormal, lerp);
            lerp += Time.deltaTime * speed;
        }
        else
        {
            oldPosition = newPosition;
            oldNormal = newNormal;
        }
    }

    // Draw the sphere at the new position of the foot (for debugging)
    private void OnDrawGizmos()
    {

        Gizmos.color = Color.red;
        Gizmos.DrawSphere(newPosition, 0.1f);
    }



    public bool IsMoving()
    {
        return lerp < 1;
    }



}
