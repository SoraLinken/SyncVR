using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Unity.Netcode;

public class GameManager : MonoBehaviour
{
    public static GameObject SwingingBall;
    public static Animator SwingingBallAnimator;
    public static GameObject yBot;
    public static GameObject xBot;
    public static GameObject yAvatar;
    public static GameObject xAvatar;
    public static GameObject leftHandModel;
    public static GameObject rightHandModel;

    public static GameObject glassRoomOne;

    public static GameObject glassRoomTwo;

    public enum AvatarType
    {
        Hands,
        YBot,
        XBot
    }

    public static AvatarType avatarType = AvatarType.Hands;

    public TextMeshProUGUI timerText;
    public static float[] phaseDurations = { 15f, 0f, 3f, 120f, 3f, 120f };
    public static int currentPhase = 0;
    public static float timeRemaining;
    public static List<Renderer> wallPaintRenderers = new List<Renderer>();

    public static Color colorSuccess = Color.white;
    public static Color colorBetween = Color.yellow;
    public static Color colorFail = Color.red;

    public static int successThreshold = 70;
    public static int failThreshold = 40;

    void Start()
    {
        SwingingBall = GameObject.Find("SwingingBall");
        SwingingBallAnimator = SwingingBall.GetComponent<Animator>();
        SwingingBallAnimator.enabled = false;

        glassRoomOne = GameObject.Find("GlassRoomOne");
        glassRoomTwo = GameObject.Find("GlassRoomTwo");

        glassRoomOne.SetActive(false);
        glassRoomTwo.SetActive(false);

        yBot = GameObject.Find("YBot");
        xBot = GameObject.Find("XBot");
        yAvatar = GameObject.Find("YAvatar");
        xAvatar = GameObject.Find("XAvatar");
        leftHandModel = GameObject.Find("LeftHandModel");
        rightHandModel = GameObject.Find("RightHandModel");

        yAvatar.SetActive(false);
        xAvatar.SetActive(false);

        timerText = GameObject.Find("TimerText").GetComponent<TextMeshProUGUI>();
        CollectWallPaintRenderers();
        StartPhase(currentPhase);
    }

    void Update()
    {
        if (timeRemaining > 0)
        {
            timeRemaining -= Time.deltaTime;
            UpdateTimerDisplay(timeRemaining);
        }
        else
        {
            timeRemaining = 0;
            UpdateTimerDisplay(timeRemaining);
            // Debug.Log("Phase: " + currentPhase);
            // Debug.Log("Count: " + SynchronizationManager.networkPlayers.Count);
            if (currentPhase == 1)
            {
                if (SynchronizationManager.networkPlayers.Count >= 2)
                {
                    currentPhase++;
                    StartPhase(currentPhase);
                }
            }
            else
            {
                currentPhase++;
                if (currentPhase < phaseDurations.Length)
                {
                    StartPhase(currentPhase);
                }
            }
        }
    }

    public static async void StartPhase(int phaseIndex)
    {
        timeRemaining = phaseDurations[phaseIndex];

        if (phaseIndex == 1)
        {
            Debug.Log("Phase 1 started");
            await NetworkConnect.Instance.JoinOrCreate();
            DisableAvatar(yBot);
            DisableAvatar(xBot);
            DisableAvatar(yAvatar);
            DisableAvatar(xAvatar);
            DisableHands();
        }
        if (phaseIndex == 2)
        {
            AudioController.Instance.PlaySound();
        }
        if (phaseIndex == 3)
        {
            Debug.Log("Phase 2 started");
            PrintAllPlayersAvatarType();
        }
        else if (phaseIndex == 4)
        {
            Debug.Log("Phase 3 started");
        }
        else if (phaseIndex == 5)
        {
            Debug.Log("Phase 4 started");
            SwingingBallAnimator.enabled = true;
            glassRoomOne.SetActive(true);
            glassRoomTwo.SetActive(true);
            // Calculate the center of each glass room
            Vector3 glassRoomOneCenter = CalculateCenter(glassRoomOne);
            Vector3 glassRoomTwoCenter = CalculateCenter(glassRoomTwo);

            int i = 0;
            foreach (var player in SynchronizationManager.networkPlayers)
            {
                // Alternate players between the two glass rooms
                Vector3 targetPosition = i % 2 == 0 ? glassRoomOneCenter : glassRoomTwoCenter;

                // Assuming each player has a GameObject or transform component to set its position
                player.transform.position = targetPosition;
                i++;
            }
        }
    }

    private static Vector3 CalculateCenter(GameObject room)
    {
        // Calculate the bounds of the room
        Renderer[] renderers = room.GetComponentsInChildren<Renderer>();
        Bounds bounds = new Bounds(renderers[0].bounds.center, Vector3.zero);
        foreach (Renderer renderer in renderers)
        {
            bounds.Encapsulate(renderer.bounds);
        }

        // Return the center of the bounds
        return bounds.center;
    }
    void UpdateTimerDisplay(float timeToDisplay)
    {
        timeToDisplay = Mathf.Max(0, timeToDisplay); // Ensure time is not negative
        float minutes = Mathf.FloorToInt(timeToDisplay / 60);
        float seconds = Mathf.FloorToInt(timeToDisplay % 60);
        timerText.text = string.Format("{0:0}:{1:00}", minutes, seconds);
    }

    void CollectWallPaintRenderers()
    {
        // Find all renderers in the scene
        Renderer[] renderers = FindObjectsOfType<Renderer>();

        foreach (Renderer renderer in renderers)
        {
            // Check if the material name is "WallPaint"
            if (renderer.material.name.Contains("WallPaint"))
            {
                wallPaintRenderers.Add(renderer);
            }
        }
    }

    public static void ChangeWallPaintColorFunction(Color newColor)
    {
        foreach (Renderer renderer in wallPaintRenderers)
        {
            // Change the color of the material
            renderer.material.color = newColor;
        }
        LightsController.Instance.changeLightColor(newColor);
    }

    public static void ChangeWallPaintColorBasedOnNumber(int number)
    {
        Color newColor;
        if (number > successThreshold)
        {
            newColor = colorSuccess;
        }
        else if (number < failThreshold)
        {
            float t = number / (float)failThreshold; // Normalize number to [0, 1]
            newColor = Color.Lerp(colorFail, colorBetween, t);
        }
        else
        {
            float t = (number - failThreshold) / (float)(successThreshold - failThreshold); // Normalize number to [0, 1]
            newColor = Color.Lerp(colorBetween, colorSuccess, t);
        }

        ChangeWallPaintColorFunction(newColor);
    }

    static public void EnableAvatar(GameObject avatar)
    {
        if (avatar != null)
        {
            avatar.SetActive(true);
        }
    }

    static public void DisableAvatar(GameObject avatar)
    {
        if (avatar != null)
        {
            avatar.SetActive(false);
        }
    }

    static public void DisableHands()
    {
        DisableAvatar(leftHandModel);
        DisableAvatar(rightHandModel);
    }

    public void SetAvatarType(AvatarType newAvatarType)
    {
        avatarType = newAvatarType;

        NetworkPlayerStats localPlayer = GetLocalNetworkPlayerStats();
        if (localPlayer != null && localPlayer.IsOwner)
        {
            localPlayer.SetAvatarType(newAvatarType);
        }
    }

    private NetworkPlayerStats GetLocalNetworkPlayerStats()
    {
        // Logic to get the local NetworkPlayerStats instance
        return FindObjectOfType<NetworkPlayerStats>();
    }

    static public void PrintAllPlayersAvatarType()
    {
        foreach (var player in FindObjectsOfType<NetworkPlayerStats>())
        {
            Debug.Log($"Player {player.OwnerClientId} has avatar type: {player.avatarType.Value}");
        }
    }
}
