using UnityEngine;

public class PlayerCollisionHandler : MonoBehaviour
{
 
    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.name == "YBot")
        {
            GameManager.DisableHands();
            GameManager.avatarType = GameManager.AvatarType.YBot;
            GameManager.DisableAvatar(GameManager.xAvatar);
            GameManager.EnableAvatar(GameManager.yAvatar);
            GameManager.EnableAvatar(GameManager.xBot);
            GameManager.DisableAvatar(GameManager.yBot);
        }
        else if (other.gameObject.name == "XBot")
        {
            GameManager.DisableHands();
            GameManager.avatarType = GameManager.AvatarType.XBot;
            GameManager.DisableAvatar(GameManager.yAvatar);
            GameManager.EnableAvatar(GameManager.xAvatar);
            GameManager.DisableAvatar(GameManager.xBot);
            GameManager.EnableAvatar(GameManager.yBot);
        }
    }
}