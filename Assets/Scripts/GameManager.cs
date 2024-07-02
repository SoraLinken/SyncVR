using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Text.RegularExpressions;
using UnityEngine.XR.Interaction.Toolkit;


public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    public static GameObject SwingingBall;
    public static Animator SwingingBallAnimator;
    public static GameObject yBot;
    public static GameObject xBot;
    public static GameObject yAvatar;
    public static GameObject xAvatar;

    public static GameObject leftHand;
    public static GameObject rightHand;
    public static GameObject leftHandModel;
    public static GameObject rightHandModel;

    public static GameObject glassRoomOne;

    public static GameObject questionsCanvas;
    public static GameObject chooseSessionCanvas;

    public static GameObject choosePlayerCanvas;

    public static GameObject glassRoomTwo;

    public enum AvatarType
    {
        Hands,
        YBot,
        XBot
    }

    public static AvatarType avatarType = AvatarType.Hands;

    public TextMeshProUGUI timerText;
    public static float[] phaseDurations = { 5f, 0f, 4f, 60f, 4f, 60f, 0f };
    public static int currentPhase = 0;
    public static float timeRemaining = 0;
    public static List<Renderer> wallPaintRenderers = new List<Renderer>();

    public static Color highSyncColor = Color.white;
    public static Color midSyncColor = Color.white;
    public static Color lowSyncColor = Color.white;

    public static int highSync = 70;
    public static int lowSync = 40;

    public static float rateOfTesting = 0.15f; // Update interval in seconds
    public static int historyLength = 8; // Maximum history records

    public static string uniqueId = "";

    public static string email = "";

    public static int sessionId = 1;
    public static bool gameStarted = false;

    public static List<Participant> selectedParticipants;

    void Start()
    {
        Instance = this;
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
        leftHand = GameObject.Find("LeftHand");
        rightHand = GameObject.Find("RightHand");

        questionsCanvas = GameObject.Find("QuestionsCanvas");
        questionsCanvas.SetActive(false);

        chooseSessionCanvas = GameObject.Find("ChooseSessionCanvas");
        chooseSessionCanvas.SetActive(false);

        choosePlayerCanvas = GameObject.Find("ChoosePlayerCanvas");
        choosePlayerCanvas.SetActive(false);


        yAvatar.SetActive(false);
        xAvatar.SetActive(false);
        DisableLasers();

        timerText = GameObject.Find("TimerText").GetComponent<TextMeshProUGUI>();
        CollectWallPaintRenderers();
    }

    static void DisableLasers()
    {
        // leftHand.GetComponent<LineRenderer>().enabled = false;
        // rightHand.GetComponent<LineRenderer>().enabled = false;

        // leftHand.GetComponent<XRInteractorLineVisual>().enabled = false;
        // rightHand.GetComponent<XRInteractorLineVisual>().enabled = false;
    }

    static void EnableLasers()
    {
        // leftHand.GetComponent<LineRenderer>().enabled = true;
        // rightHand.GetComponent<LineRenderer>().enabled = true;

        leftHand.GetComponent<XRInteractorLineVisual>().enabled = true;
        rightHand.GetComponent<XRInteractorLineVisual>().enabled = true;
    }

    public void OnPlayerChosen()
    {
        choosePlayerCanvas.SetActive(false);
        StartPhase(currentPhase);
        DisableLasers();
        gameStarted = true;

        AvatarDeclaration data = new AvatarDeclaration(uniqueId, (int)avatarType, email);
        string jsonData = JsonUtility.ToJson(data);

        StartCoroutine(APIClient.PutRequest("/avatars", jsonData, (response) =>
        {
            Debug.Log("Avatar set succesfully");
        }, (error) =>
        {
            Debug.LogError("Error submitting questionnaire: " + error);
        }));
    }

    public System.Collections.IEnumerator OnSessionPicked(int newSessionId)
    {
        sessionId = newSessionId;
        Debug.Log("Session ID: " + sessionId);
        yield return StartCoroutine(
            APIClient.GetRequest($"/on-game-start?sessionId={sessionId}",
            data =>
            {
                Debug.Log("Game start data: " + data);
                var gameStartData = JsonUtility.FromJson<GameStartData>(data);
                uniqueId = gameStartData.uniqueId;
                phaseDurations[3] = gameStartData.phaseDuration;
                phaseDurations[5] = gameStartData.phaseDuration;
                historyLength = gameStartData.historyLength;
                rateOfTesting = gameStartData.rateOfTesting;
                highSync = gameStartData.highSync;
                lowSync = gameStartData.lowSync;
                highSyncColor = ParseRGBColor(gameStartData.highSyncColor);
                midSyncColor = ParseRGBColor(gameStartData.midSyncColor);
                lowSyncColor = ParseRGBColor(gameStartData.lowSyncColor);

                // Store the selected participants
                selectedParticipants = gameStartData.selectedParticipants;
            },
            error =>
            {
                Debug.Log("Error getting game start data:" + error);
            })
        );
       
        chooseSessionCanvas.SetActive(false);
        choosePlayerCanvas.SetActive(true);
        ChoosePlayerController.Instance.initialize();
    }

    private Color ParseRGBColor(string rgbColor)
    {
        Regex regex = new Regex(@"rgb\((\d+),\s*(\d+),\s*(\d+)\)");
        Match match = regex.Match(rgbColor);

        if (match.Success)
        {
            int r = int.Parse(match.Groups[1].Value);
            int g = int.Parse(match.Groups[2].Value);
            int b = int.Parse(match.Groups[3].Value);

            return new Color(r / 255f, g / 255f, b / 255f);
        }
        else
        {
            Debug.LogError("Invalid color string: " + rgbColor);
            return Color.white; // Default color in case of error
        }
    }



    void Update()
    {
        if (gameStarted)
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
            Debug.Log("Phase 2 started");
            try
            {
                // var localPlayer = GetLocalNetworkPlayerStats();
                // if (localPlayer != null)
                // {
                //     int playerId = (int)localPlayer.OwnerClientId - 1;
                //     if (playerId >= 0 && playerId < selectedParticipants.Count)
                //     {
                //         email = selectedParticipants[playerId].email;
                //         Debug.Log("Player email: " + email);
                //     }
                //     else
                //     {
                //         Debug.LogError("Player ID out of range for selected participants.");
                //     }
                // }
                // else
                // {
                //     Debug.LogError("Local player not found.");
                // }
            }
            catch (System.Exception e)
            {
                Debug.Log(e);
            }


            AudioController.Instance.PlaySound();
        }
        if (phaseIndex == 3)
        {
            Debug.Log("Phase 2 started");
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
        else if (phaseIndex == 6)
        {
            glassRoomOne.SetActive(false);
            glassRoomTwo.SetActive(false);
            SwingingBallAnimator.enabled = false;
            questionsCanvas.SetActive(true);
            EnableLasers();
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
        if (timerText)
        {
            timeToDisplay = Mathf.Max(0, timeToDisplay); // Ensure time is not negative
            float minutes = Mathf.FloorToInt(timeToDisplay / 60);
            float seconds = Mathf.FloorToInt(timeToDisplay % 60);
            timerText.text = string.Format("{0:0}:{1:00}", minutes, seconds);
        }
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
        if (number > highSync)
        {
            newColor = highSyncColor;
        }
        else if (number < lowSync)
        {
            float t = number / (float)lowSync; // Normalize number to [0, 1]
            newColor = Color.Lerp(lowSyncColor, midSyncColor, t);
        }
        else
        {
            float t = (number - lowSync) / (float)(highSync - lowSync); // Normalize number to [0, 1]
            newColor = Color.Lerp(midSyncColor, highSyncColor, t);
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

    public static void SetAvatarType(AvatarType newAvatarType)
    {
        avatarType = newAvatarType;
        chooseSessionCanvas.SetActive(true);
        EnableLasers();
        if (avatarType == AvatarType.YBot)
        {
            DisableAvatar(xBot);
            DisableAvatar(yBot);
            EnableAvatar(yAvatar);
            DisableAvatar(xAvatar);
            DisableAvatar(leftHandModel);
            DisableAvatar(rightHandModel);
        }
        else if (avatarType == AvatarType.XBot)
        {
            DisableAvatar(yBot);
            DisableAvatar(xBot);
            DisableAvatar(yAvatar);
            EnableAvatar(xAvatar);
            DisableAvatar(leftHandModel);
            DisableAvatar(rightHandModel);
        }
    }
}

[System.Serializable]
public class GameStartData
{
    public string uniqueId;
    public float phaseDuration;
    public int historyLength;
    public float rateOfTesting;
    public int highSync;
    public int lowSync;
    public string highSyncColor;
    public string midSyncColor;
    public string lowSyncColor;
    public List<Participant> selectedParticipants;
}

[System.Serializable]
public class Participant
{
    public string _id;
    public string name;
    public string email;
    public string sex;
    public string lastExperience;
}



[System.Serializable]
public class AvatarDeclaration
{
    public string uniqueId;
    public int avatarType;
    public string email;

    public AvatarDeclaration(string uniqueId, int avatarType, string email)
    {
        this.uniqueId = uniqueId;
        this.avatarType = avatarType;
        this.email = email;
    }
}