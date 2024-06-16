using UnityEngine;

public class PlayerCollisionHandler : MonoBehaviour
{
 
    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.name == "YBot")
        {
            GameManager.SetAvatarType(GameManager.AvatarType.YBot);
            
        }
        else if (other.gameObject.name == "XBot")
        {
           GameManager.SetAvatarType(GameManager.AvatarType.XBot);
        }
    }
}