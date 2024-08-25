using UnityEngine;


// Class to handle player collision with the avatar selection objects (XBot and YBot on the portals)
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