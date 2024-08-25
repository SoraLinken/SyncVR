using UnityEngine;


// Handle collision detection for the pendulum and the players head
public class PendulumCollisionDetection : MonoBehaviour
{
    private bool isOverRug = false;
    private int headCollisions = 0;
    public GameObject rightRugCollisionBox;
    public GameObject leftRugCollisionBox;

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject == rightRugCollisionBox || other.gameObject == leftRugCollisionBox)
        {
            isOverRug = true;
        }
        else if (other.gameObject.CompareTag("HeadCollider"))
        {
            headCollisions++;
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.gameObject == rightRugCollisionBox || other.gameObject == leftRugCollisionBox)
        {
            isOverRug = false;
        }
        else if (other.gameObject.CompareTag("HeadCollider"))
        {
            headCollisions--;
        }
    }

    public bool IsOverRug()
    {
        return isOverRug;
    }

    public bool IsCollidingWithHead()
    {
        return headCollisions > 0;
    }
}
