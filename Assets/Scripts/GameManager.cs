using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Text.RegularExpressions;
using UnityEngine.XR.Interaction.Toolkit;
using ReadyPlayerMe.Core.WebView;
using System.Collections;

public enum Phase
{
    SafetyPadding,
    Initialization,
    Preparation1,
    Synchronization,

    Preparation2,
    SwingingBall,
    Questionnaire
}


// Main component with most of the game logic and shared global variables.
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public static GameObject SwingingBall180;
    public static GameObject Pendulum180;
    public static GameObject SwingingBall360;
    public static GameObject Pendulum360;
    public static GameObject Pendulum; // Current pendulum
    public static GameObject SwingingBall; // Current swinging ball (Subcomponent of the pendulum)

    public static Animator SwingingBallAnimator180;
    public static Animator SwingingBallAnimator360;
    public static Animator SwingingBallAnimator; // Current swinging ball animator

    public static GameObject yBot; // Female Avatar on top of the portal
    public static GameObject xBot; // Male Avatar on top of the portal
    public static GameObject yAvatar; // Female player avatar before connection with other player
    public static GameObject xAvatar; // Male player avatar before connection with other player

    public static GameObject leftHand;
    public static GameObject rightHand;
    public static GameObject leftHandModel;
    public static GameObject rightHandModel;


    public static GameObject rugLeft;
    public static GameObject rugRight;

    public static GameObject glassRoomTwo; // Glass room on top of the right rug
    public static GameObject glassRoomOne; // Glass room on top of the left rug
    public static GameObject portalZone; // Area where the players select their avatars

    public static GameObject questionsCanvas;

    public static GameObject chooseSessionCanvas;

    public static GameObject choosePlayerCanvas;

    public static int pendulumRotation;

    public enum AvatarType
    {
        Hands,
        YBot,
        XBot
    }

    public static AvatarType avatarType = AvatarType.Hands; // Current avatar type

    public TextMeshProUGUI timerText;

    public static Renderer colorChangingWallRenderer; // Controls the color of the walls

    public static List<Participant> selectedParticipants; // Holds the details of both participants



    // All of the next global variables are overwritten by the API response
    public static float[] phaseDurations = { 0, 0, 12f, 60f, 12f, 60f, 0f }; // Duration of each phase in seconds
    public static int currentPhase = 0;
    public static float timeRemaining = 0; // Time remaining in the current phase

    public static Color highSyncColor = Color.white;
    public static Color midSyncColor = Color.white;
    public static Color lowSyncColor = Color.white;

    public static int highSync = 70; // Threshold for low synchronization level
    public static int lowSync = 40; // Threshold for low synchronization level

    public static float rateOfTesting = 0.03f; // Update interval in seconds
    public static int historyLength = 8; // Maximum history records

    public static string uniqueId = ""; // Unique ID of the current player

    public static string email = ""; // Email of the creator of the experience

    public static int sessionId = 1; // ID of the current experience session (1 - 100)

    public static string[] experienceType; // Mods enabled, Hands/Pendulum or both
    public static bool hasPendulum = false;
    public static bool hasHands = false;

    public static bool gameStarted = false;

    void Start()
    {
        Instance = this;

        // Setup swinging and pendulum objects, defaults to 360, overriden by API response
        SwingingBall180 = GameObject.Find("SwingingBall180");
        SwingingBallAnimator180 = SwingingBall180.GetComponent<Animator>();
        Pendulum180 = GameObject.Find("PendulumBlade180");
        SwingingBallAnimator180.enabled = false;
        SwingingBall180.SetActive(false);
        SwingingBall360 = GameObject.Find("SwingingBall360");
        SwingingBallAnimator360 = SwingingBall360.GetComponent<Animator>();
        Pendulum360 = GameObject.Find("PendulumBlade360");
        SwingingBallAnimator360.enabled = false;
        SwingingBall = SwingingBall360;
        Pendulum = Pendulum360;
        SwingingBallAnimator = SwingingBallAnimator360;


        // Setup rugs and glassrooms above rugs
        rugLeft = GameObject.Find("RugLeft");
        rugRight = GameObject.Find("RugRight");
        glassRoomOne = GameObject.Find("GlassRoomOne");
        glassRoomTwo = GameObject.Find("GlassRoomTwo");
        glassRoomOne.SetActive(false);
        glassRoomTwo.SetActive(false);


        // Setup avatars (player and static above portal)
        yBot = GameObject.Find("YBot");
        xBot = GameObject.Find("XBot");
        yAvatar = GameObject.Find("YAvatar");
        xAvatar = GameObject.Find("XAvatar");
        leftHandModel = GameObject.Find("LeftHandModel");
        rightHandModel = GameObject.Find("RightHandModel");
        leftHand = GameObject.Find("PlayerLeftHand");
        rightHand = GameObject.Find("PlayerRightHand");
        yAvatar.SetActive(false);
        xAvatar.SetActive(false);
        DisableLasers();


        portalZone = GameObject.Find("PortalZone");


        // Setup player input screens
        questionsCanvas = GameObject.Find("QuestionsCanvas");
        chooseSessionCanvas = GameObject.Find("ChooseSessionCanvas");
        choosePlayerCanvas = GameObject.Find("ChoosePlayerCanvas");
        questionsCanvas.SetActive(false);
        chooseSessionCanvas.SetActive(false);
        choosePlayerCanvas.SetActive(false);


        // Setup color changing walls
        colorChangingWallRenderer = GameObject.Find("ColorChangingWall").GetComponent<Renderer>();

      
        timerText = GameObject.Find("TimerText").GetComponent<TextMeshProUGUI>();
    }


    // Disable the laser pointers coming from the hands (help with input selection)
    static void DisableLasers()
    {
        leftHand.GetComponent<LineRenderer>().enabled = false;
        rightHand.GetComponent<LineRenderer>().enabled = false;

        leftHand.GetComponent<XRInteractorLineVisual>().enabled = false;
        rightHand.GetComponent<XRInteractorLineVisual>().enabled = false;
    }

    // Enable the laser pointers coming from the hands (help with input selection)
    static void EnableLasers()
    {
        leftHand.GetComponent<LineRenderer>().enabled = true;
        rightHand.GetComponent<LineRenderer>().enabled = true;

        leftHand.GetComponent<XRInteractorLineVisual>().enabled = true;
        rightHand.GetComponent<XRInteractorLineVisual>().enabled = true;
    }


    // Send a request with current avatar type selected by the player
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

    // Get experience details after the session Id is picked
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
                experienceType = gameStartData.experienceType;
                phaseDurations[3] = gameStartData.phaseDuration;
                phaseDurations[5] = gameStartData.phaseDuration;
                historyLength = gameStartData.historyLength;
                rateOfTesting = gameStartData.rateOfTesting;
                highSync = gameStartData.highSync;
                lowSync = gameStartData.lowSync;
                highSyncColor = ParseRGBColor(gameStartData.highSyncColor);
                midSyncColor = ParseRGBColor(gameStartData.midSyncColor);
                lowSyncColor = ParseRGBColor(gameStartData.lowSyncColor);
                pendulumRotation = gameStartData.pendulumRotation;
                // Store the selected participants
                selectedParticipants = gameStartData.selectedParticipants;
            },
            error =>
            {
                Debug.Log("Error getting game start data:" + error);
            })
        );

        foreach (string experience in experienceType)
        {
            if (experience == "hands")
            {
                hasHands = true;
            }
            else if (experience == "pendulum")
            {
                hasPendulum = true;
            }
        }

        if (pendulumRotation == 180)
        {
            SwingingBall180.SetActive(true);
            SwingingBall = SwingingBall180;
            Pendulum = Pendulum180;
            SwingingBallAnimator = SwingingBallAnimator180;
            SwingingBall360.SetActive(false);
        }
        chooseSessionCanvas.SetActive(false);
        choosePlayerCanvas.SetActive(true);
        ChoosePlayerController.Instance.initialize();
    }

    // Helper function to parse RGB color strings
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


    // Game logic for each phase
    public static async void StartPhase(int phaseIndex)
    {
        timeRemaining = phaseDurations[phaseIndex];

        if (phaseIndex == (int)Phase.Initialization)
        {
            declarePhaseStart(Phase.Initialization);
            portalZone.SetActive(false);
            await NetworkConnect.Instance.JoinOrCreate();
            DisableAvatar(yBot);
            DisableAvatar(xBot);
            DisableAvatar(yAvatar);
            DisableAvatar(xAvatar);
            DisableHands();
        }
        if (phaseIndex == (int)Phase.Preparation1)
        {
            if(!hasHands)
            {
                currentPhase++;
                StartPhase(currentPhase);
                return;
            }
            declarePhaseStart(Phase.Preparation1);
        }
        if (phaseIndex == (int)Phase.Synchronization)
        {
            if(!hasHands)
            {
                currentPhase++;
                StartPhase(currentPhase);
                return;
            }
            declarePhaseStart(Phase.Synchronization);
        }
        else if (phaseIndex == (int)Phase.Preparation2)
        {
            if(!hasPendulum)
            {
                currentPhase++;
                StartPhase(currentPhase);
                return;
            }
            declarePhaseStart(Phase.Preparation2);
        }
        else if (phaseIndex == (int)Phase.SwingingBall)
        {
            if(!hasPendulum)
            {
                currentPhase++;
                StartPhase(currentPhase);
                return;
            }
            declarePhaseStart(Phase.SwingingBall);
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
        else if (phaseIndex == (int)Phase.Questionnaire)
        {
            declarePhaseStart(Phase.Questionnaire);
            glassRoomOne.SetActive(false);
            glassRoomTwo.SetActive(false);
            SwingingBallAnimator.enabled = false;
            questionsCanvas.SetActive(true);
            EnableLasers();
        }
    }

    // Simple logger for phase start
    private static void declarePhaseStart(Phase phaseIndex)
    {
        Debug.Log("Phase " + phaseIndex + " started");
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


    // Format the timer
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

    // Change the color of the walls and lights based on the synchronization level
    public static void ChangeWallPaintColorFunction(Color newColor)
    {
        if (colorChangingWallRenderer.material.color == newColor) return;
        colorChangingWallRenderer.material.color = newColor;
        LightsController.Instance.changeLightColor(newColor);
    }

    // Create a new color based on the synchronization level
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

    // Game logic for selecting an avatar (before connection with other player)
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

    // Once game is over, exit the application
    public static IEnumerator QuitGame()
    {
        yield return new WaitForSeconds(3);
        Debug.Log("Exiting game...");

        Application.Quit();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}

[System.Serializable]
public class GameStartData
{
    public string[] experienceType;
    public int pendulumRotation;
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