using UnityEngine;

public class PendulumCollision : MonoBehaviour
{
    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Character")) // Ensure your character has the tag "Character"
        {
            Rigidbody characterRb = collision.gameObject.GetComponent<Rigidbody>();
            if (characterRb != null)
            {
                // Calculate the direction of the force
                Vector3 forceDirection = collision.transform.position - transform.position;
                forceDirection.y = 0; // Optional: to keep the force horizontal
                forceDirection.Normalize();

                // Apply force to the character
                float forceMagnitude = 10f; // Adjust the force magnitude as needed
                characterRb.AddForce(forceDirection * forceMagnitude, ForceMode.Impulse);
            }
        }
    }
}
