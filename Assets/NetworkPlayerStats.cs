using Unity.Netcode;
using UnityEngine;

public class NetworkPlayerStats : NetworkBehaviour
{
    public NetworkVariable<int> avatarType = new NetworkVariable<int>();
    public NetworkVariable<int> currentPhase = new NetworkVariable<int>(0);

    public GameObject yAvatarNetwork;
    public GameObject xAvatarNetwork;

    private void Awake()
    {
        avatarType.OnValueChanged += OnAvatarTypeChanged;
        currentPhase.OnValueChanged += OnPhaseChanged;
    }

    private void Start()
    {
        yAvatarNetwork.SetActive(false);
        xAvatarNetwork.SetActive(false);

        if (IsOwner)
        {
            avatarType.Value = (int)GameManager.avatarType;
        }

        UpdateAvatar(avatarType.Value);
    }

    private void OnAvatarTypeChanged(int oldValue, int newValue)
    {
        UpdateAvatar(newValue);
    }

    private void OnPhaseChanged(int oldValue, int newValue)
    {
        if (IsOwner)
        {
            GameManager.StartPhase(newValue);
        }
    }

    private void UpdateAvatar(int newAvatarType)
    {
        switch ((GameManager.AvatarType)newAvatarType)
        {
            case GameManager.AvatarType.Hands:
                yAvatarNetwork.SetActive(true);
                xAvatarNetwork.SetActive(false);
                break;
            case GameManager.AvatarType.YBot:
                yAvatarNetwork.SetActive(true);
                xAvatarNetwork.SetActive(false);
                break;
            case GameManager.AvatarType.XBot:
                yAvatarNetwork.SetActive(false);
                xAvatarNetwork.SetActive(true);
                break;
        }
    
        Debug.Log($"Player {OwnerClientId} updated to avatar type: {newAvatarType}");
    }

    [ServerRpc]
    public void SetAvatarTypeServerRpc(int newAvatarType)
    {
        avatarType.Value = newAvatarType;
    }

    [ServerRpc]
    public void SetPhaseServerRpc(int phase)
    {
        currentPhase.Value = phase;
    }

    public void SetAvatarType(GameManager.AvatarType newAvatarType)
    {
        if (IsOwner)
        {
            SetAvatarTypeServerRpc((int)newAvatarType);
        }
    }

    public void SetPhase(int phase)
    {
        if (IsOwner)
        {
            SetPhaseServerRpc(phase);
        }
    }
}
