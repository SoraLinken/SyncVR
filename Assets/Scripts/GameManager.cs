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

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public static string[] experienceType;
    public static bool hasPendulum = false;
    public static bool hasHands = false;

    public static GameObject SwingingBall180;
    public static GameObject Pendulum180;
    public static GameObject SwingingBall360;
    public static GameObject Pendulum360;

    public static Animator SwingingBallAnimator180;
    public static Animator SwingingBallAnimator360;
    public static GameObject SwingingBall;

    public static GameObject Pendulum;
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

    public static GameObject portalZone;
    public static GameObject chooseSessionCanvas;

    public static GameObject choosePlayerCanvas;

    public static GameObject glassRoomTwo;
    public static int pendulumRotation;

    public static GameObject rugLeft;
    public static GameObject rugRight;

    public enum AvatarType
    {
        Hands,
        YBot,
        XBot
    }

    public static AvatarType avatarType = AvatarType.Hands;

    public TextMeshProUGUI timerText;

    public static float[] phaseDurations = { 0, 0, 12f, 60f, 12f, 60f, 0f };
    public static int currentPhase = 0;
    public static float timeRemaining = 0;

    public static Color highSyncColor = Color.white;
    public static Color midSyncColor = Color.white;
    public static Color lowSyncColor = Color.white;

    public static int highSync = 70;
    public static int lowSync = 40;

    public static float rateOfTesting = 0.03f; // Update interval in seconds
    public static int historyLength = 8; // Maximum history records

    public static string uniqueId = "";

    public static string email = "";

    public static int sessionId = 1;
    public static bool gameStarted = false;

    public static Renderer colorChangingWallRenderer;

    public static List<Participant> selectedParticipants;

    void Start()
    {
        Instance = this;

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
        leftHand = GameObject.Find("PlayerLeftHand");
        rightHand = GameObject.Find("PlayerRightHand");

        questionsCanvas = GameObject.Find("QuestionsCanvas");
        questionsCanvas.SetActive(false);

        portalZone = GameObject.Find("PortalZone");

        rugLeft = GameObject.Find("RugLeft");
        rugRight = GameObject.Find("RugRight");

        chooseSessionCanvas = GameObject.Find("ChooseSessionCanvas");
        chooseSessionCanvas.SetActive(false);

        choosePlayerCanvas = GameObject.Find("ChoosePlayerCanvas");
        choosePlayerCanvas.SetActive(false);

        colorChangingWallRenderer = GameObject.Find("ColorChangingWall").GetComponent<Renderer>();

        yAvatar.SetActive(false);
        xAvatar.SetActive(false);
        DisableLasers();

        timerText = GameObject.Find("TimerText").GetComponent<TextMeshProUGUI>();
    }

    static void DisableLasers()
    {
        leftHand.GetComponent<LineRenderer>().enabled = false;
        rightHand.GetComponent<LineRenderer>().enabled = false;

        leftHand.GetComponent<XRInteractorLineVisual>().enabled = false;
        rightHand.GetComponent<XRInteractorLineVisual>().enabled = false;
    }

    static void EnableLasers()
    {
        leftHand.GetComponent<LineRenderer>().enabled = true;
        rightHand.GetComponent<LineRenderer>().enabled = true;

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
            // AudioController.Instance.PlaySound();
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
            // AudioController.Instance.PlaySound();
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

    public static void ChangeWallPaintColorFunction(Color newColor)
    {
        if (colorChangingWallRenderer.material.color == newColor) return;
        colorChangingWallRenderer.material.color = newColor;
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


    static void LogAllComponents(GameObject gameObject)
    {
        if (gameObject == null)
        {
            Debug.LogError("GameObject is not assigned.");
            return;
        }

        Debug.Log("Logging components for: " + gameObject.name);

        Component[] components = gameObject.GetComponents<Component>();
        Debug.Log(gameObject.name + " has " + components.Length + " components.");

        foreach (Component component in components)
        {
            Debug.Log(gameObject.name + " has component: " + component.GetType().ToString());
        }
    }

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