using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using System.Linq;
using System;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using TMPro;
using UnityEngine.XR;

[System.Serializable]
public struct SynchronizationDatum
{
    public float value;  // The synchronization score
    public float time;   // Time at which the score was recorded

    // Constructor to initialize SynchronizationDatum
    public SynchronizationDatum(float value, float time)
    {
        this.value = value;
        this.time = time;
    }
}

public class SynchronizationManager : MonoBehaviour
{
    private const int TargetSampleSize = 1000;  // Target size for sample normalization
    private Dictionary<ulong, Dictionary<string, Queue<(Vector3 position, Quaternion rotation)>>> history =
        new Dictionary<ulong, Dictionary<string, Queue<(Vector3 position, Quaternion rotation)>>>();  // History for each network player

    public static List<NetworkObject> networkPlayers = new List<NetworkObject>();  // List of all network players

    public Dictionary<string, Queue<(Vector3 position, Quaternion rotation)>> localPlayerHistory =
        new Dictionary<string, Queue<(Vector3 position, Quaternion rotation)>>();  // Local player's history

    public static List<SynchronizationDatum> synchronizationHands = new List<SynchronizationDatum>();  // Synchronization data for hands
    public static List<SynchronizationDatum> synchronizationPendulum = new List<SynchronizationDatum>();  // Synchronization data for pendulum

    private bool headHit = false;  // Track whether the pendulum hit the player's head

    void Start()
    {
        // Repeatedly update network players at a defined rate
        InvokeRepeating("UpdateNetworkPlayers", 0f, GameManager.rateOfTesting);
    }

    // Normalize list to match the target sample size through interpolation or downsampling
    static public List<SynchronizationDatum> NormalizeListSize(List<SynchronizationDatum> originalList)
    {
        if (originalList.Count == 0)
        {
            return originalList;
        }

        // Return the list if it already has the target size
        if (originalList.Count == TargetSampleSize)
        {
            return originalList;
        }

        var normalizedList = new List<SynchronizationDatum>();

        if (originalList.Count < TargetSampleSize)
        {
            // Interpolate data to increase list size to match target sample size
            for (int i = 0; i < TargetSampleSize; i++)
            {
                float t = i / (float)(TargetSampleSize - 1);
                var interpolated = Interpolate(originalList, t);
                normalizedList.Add(interpolated);
            }
        }
        else
        {
            // Downsample data by selecting elements to reduce the list size to target sample size
            for (int i = 0; i < TargetSampleSize; i++)
            {
                float t = i / (float)(TargetSampleSize - 1);
                int index = Mathf.RoundToInt(t * (originalList.Count - 1));
                normalizedList.Add(originalList[index]);
            }
        }

        return normalizedList;
    }

    // Interpolates between two points in the list to return a SynchronizationDatum at a given t (0-1 range)
    static SynchronizationDatum Interpolate(List<SynchronizationDatum> list, float t)
    {
        int count = list.Count - 1;
        float scaledT = t * count;
        int index = Mathf.FloorToInt(scaledT);
        float fraction = scaledT - index;

        if (index >= count)
        {
            return list[count];
        }

        var start = list[index];
        var end = list[index + 1];

        var interpolatedValue = Mathf.Lerp(start.value, end.value, fraction);
        var interpolatedTime = Mathf.Lerp(start.time, end.time, fraction);

        return new SynchronizationDatum(interpolatedValue, interpolatedTime);
    }

    // Update the list of network players by finding all game objects tagged as "Player"
    void UpdateNetworkPlayers()
    {
        GameObject[] allObjects = GameObject.FindGameObjectsWithTag("Player");

        // Ensure that the networkPlayers list contains at least 2 players
        if (networkPlayers.Count < 2)
        {
            foreach (GameObject obj in allObjects)
            {
                NetworkObject netObject = obj.GetComponent<NetworkObject>();

                // Add the player to the list if it's not already there
                if (netObject != null && !networkPlayers.Any(p => p.NetworkObjectId == netObject.NetworkObjectId))
                {
                    networkPlayers.Add(netObject);
                    if (!history.ContainsKey(netObject.NetworkObjectId))
                    {
                        history[netObject.NetworkObjectId] = new Dictionary<string, Queue<(Vector3, Quaternion)>>();
                    }
                }
            }
        }

        // Update the history for each network player
        foreach (var netObject in networkPlayers)
        {
            if (!history.ContainsKey(netObject.NetworkObjectId))
            {
                history[netObject.NetworkObjectId] = new Dictionary<string, Queue<(Vector3, Quaternion)>>();
            }

            // Update history for the network player
            UpdateHistory(netObject, history[netObject.NetworkObjectId], GameManager.historyLength);

            // Handle history for the local player without a max records limit
            if (netObject.IsLocalPlayer)
            {
                UpdateHistory(netObject, localPlayerHistory, int.MaxValue);
            }
        }

        // Check synchronization when more than one player is present
        if (networkPlayers.Count > 1)
        {
            CheckSynchronization();
        }
    }

    // Function to check synchronization based on the current phase of the game
    void CheckSynchronization()
    {
        if ((GameManager.currentPhase != (int)Phase.Synchronization && GameManager.currentPhase != (int)Phase.SwingingBall) || networkPlayers.Count < 2)
        {
            GameManager.ChangeWallPaintColorFunction(Color.white);  // Default color when not in sync phase
            return;
        }

        float pendulumRotationZ = GameManager.Pendulum.transform.rotation.eulerAngles.z;
        
        if (GameManager.currentPhase == (int)Phase.SwingingBall)
        {
            // Adjust rotation values for synchronization checks
            if (pendulumRotationZ > 180)
            {
                pendulumRotationZ -= 360;
            }

            if (pendulumRotationZ < 60 && pendulumRotationZ > -60)
            {
                GetPendulumSynchronization();  // Check pendulum synchronization if within bounds
            }
            else
            {
                headHit = false;  // Reset head hit state
            }
        }
        else
        {
            GetHandSynchronization();  // Check hand synchronization
        }
    }

    // Calculate pendulum synchronization score and update game visuals accordingly
    double GetPendulumSynchronization()
    {
        double syncScore = 0;

        PendulumCollisionDetection pendulumCollision = GameManager.Pendulum.GetComponent<PendulumCollisionDetection>();

        if (pendulumCollision.IsOverRug())  // Check if pendulum is above the rug area
        {
            if (headHit)
            {
                syncScore = 0;  // Reset score if head was already hit
            }
            else if (pendulumCollision.IsCollidingWithHead())
            {
                headHit = true;  // Mark head hit and start vibration feedback
                StartVibration();
                syncScore = 0;
            }
            else
            {
                syncScore = 100;  // Assign maximum score if head is not hit
            }

            synchronizationPendulum.Add(new SynchronizationDatum((float)syncScore, GameManager.timeRemaining));  // Record score
            GameManager.ChangeWallPaintColorBasedOnNumber((int)syncScore);  // Update visual feedback
            return syncScore;
        }
        return 0;
    }

    // Trigger haptic feedback for VR devices when head is hit
    void StartVibration()
    {
        HapticCapabilities capabilities;
        InputDevice device = InputDevices.GetDeviceAtXRNode(XRNode.RightHand);

        if (device.TryGetHapticCapabilities(out capabilities) && capabilities.supportsImpulse)
        {
            uint channel = 0;
            device.SendHapticImpulse(channel, 1.0f, 1.0f);  // Send vibration for 1 second
            Invoke("StopVibration", 1.0f);  // Schedule vibration stop
        }
    }

    // Stop the vibration for the VR device
    void StopVibration()
    {
        InputDevice device = InputDevices.GetDeviceAtXRNode(XRNode.RightHand);
        device.StopHaptics();
    }

    // Calculate hand synchronization score based on position differences between players
    double GetHandSynchronization()
    {
        double syncScore = 0;
        double totalDifference = 0;
        int comparisons = 0;

        const double positionNormalizer = 0.97;
        const double epsilon = 1e-6;

        // Compare positions between each pair of players
        for (int i = 0; i < networkPlayers.Count; i++)
        {
            for (int j = i + 1; j < networkPlayers.Count; j++)
            {
                ulong id1 = networkPlayers[i].NetworkObjectId;
                ulong id2 = networkPlayers[j].NetworkObjectId;

                foreach (var tag in history[id1].Keys)
                {
                    if (!history[id1].ContainsKey(tag) || !history[id2].ContainsKey(tag)) continue;

                    var queue1 = NormalizeQueue(history[id1][tag]);
                    var queue2 = NormalizeQueue(history[id2][tag]);
                    if (queue1.Count == 0 || queue2.Count == 0) continue;

                    double averagePositionDifference = 0;

                    var array1 = queue1.ToArray();
                    var array2 = queue2.ToArray();
                    int count = Math.Min(array1.Length, array2.Length);

                    // Compute the average difference in position between two players
                    for (int k = 0; k < count; k++)
                    {
                        double positionDiff = Vector3.Distance(array1[k].position, array2[k].position);
                        averagePositionDifference += Math.Max(0, Math.Pow(positionNormalizer + positionDiff, 12) - positionNormalizer);
                    }

                    averagePositionDifference /= count;
                    totalDifference += averagePositionDifference;
                }
                comparisons++;
            }
        }

        // Calculate the final synchronization score
        if (comparisons > 0)
        {
            syncScore = Math.Max(0, 100 - Math.Sqrt(Math.Max(0, totalDifference / comparisons + epsilon)));
        }
        else
        {
            syncScore = 0;
        }

        synchronizationHands.Add(new SynchronizationDatum((float)syncScore, GameManager.timeRemaining));  // Record the hand sync score
        GameManager.ChangeWallPaintColorBasedOnNumber((int)syncScore);  // Update game visuals based on score
        return syncScore;
    }

    // Normalize the movement data in a queue by correcting outliers
    Queue<(Vector3 position, Quaternion rotation)> NormalizeQueue(Queue<(Vector3 position, Quaternion rotation)> originalQueue)
    {
        var list = originalQueue.ToList();
        for (int i = 1; i < list.Count - 1; i++)
        {
            var prevMove = GetMovement(list[i - 1].position, list[i].position);
            var nextMove = GetMovement(list[i].position, list[i + 1].position);

            // Correct outliers in movement direction by averaging surrounding points
            if (prevMove == nextMove)
            {
                list[i] = ((list[i - 1].position + list[i + 1].position) / 2, list[i].rotation);
            }
        }
        return new Queue<(Vector3 position, Quaternion rotation)>(list);
    }

    // Determine whether the movement is upwards or downwards
    Movement GetMovement(Vector3 previous, Vector3 current)
    {
        return current.y > previous.y ? Movement.Up : Movement.Down;
    }

    enum Movement
    {
        Up,
        Down
    }

    // Update the synchronization percentage UI on screen
    void UpdateSyncPercentageUI(double score)
    {
        GameObject textObject = GameObject.FindWithTag("SyncPercentage");
        if (textObject != null)
        {
            TextMeshProUGUI syncText = textObject.GetComponent<TextMeshProUGUI>();
            if (syncText != null)
            {
                syncText.text = $"{score:0.00}%";  // Display the score with two decimal places
            }
            else
            {
                Debug.LogError("TextMeshProUGUI component not found on the object with tag 'SyncPercentage'");
            }
        }
        else
        {
            Debug.LogError("No GameObject found with the tag 'SyncPercentage'");
        }
    }

    // Update position and rotation history for a network player
    void UpdateHistory(NetworkObject netObject, Dictionary<string, Queue<(Vector3, Quaternion)>> playerHistory, int limit)
    {
        string[] tags = { "LeftHandTarget", "RightHandTarget" };  // Tags to track in history
        bool shouldMirror = ShouldMirror(netObject);  // Determine if the player's data should be mirrored

        Transform headTransform = FindChildWithTag(netObject.transform, "HeadTarget");
        Vector3 netObjectPosition = headTransform.position;
        Quaternion netObjectRotation = headTransform.rotation;

        // If mirroring is needed, apply necessary transformations
        if (shouldMirror)
        {
            netObjectPosition = new Vector3(-netObjectPosition.x, netObjectPosition.y, netObjectPosition.z);
            netObjectRotation = new Quaternion(netObjectRotation.x, -netObjectRotation.y, -netObjectRotation.z, netObjectRotation.w);
        }

        Dictionary<string, (Vector3, Quaternion)> tempData = new Dictionary<string, (Vector3, Quaternion)>();

        foreach (string tag in tags)
        {
            Transform targetTransform = FindChildWithTag(netObject.transform, tag);
            if (targetTransform != null)
            {
                Vector3 worldPosition = targetTransform.position;
                Quaternion worldRotation = targetTransform.rotation;

                // Apply mirroring to world coordinates if necessary
                if (shouldMirror)
                {
                    worldPosition = new Vector3(-worldPosition.x, worldPosition.y, worldPosition.z);
                    worldRotation = new Quaternion(worldRotation.x, -worldRotation.y, -worldRotation.z, worldRotation.w);
                }

                // Convert to local space
                Vector3 localPosition = Quaternion.Inverse(netObjectRotation) * (worldPosition - netObjectPosition);
                Quaternion localRotation = Quaternion.Inverse(netObjectRotation) * worldRotation;

                if (shouldMirror)
                {
                    localPosition = new Vector3(-localPosition.x, localPosition.y, localPosition.z);
                    localRotation = new Quaternion(-localRotation.x, localRotation.y, localRotation.z, -localRotation.w);
                }

                tempData[tag] = (localPosition, localRotation);  // Store the transformed data
            }
        }

        // Add the data to the player's history and maintain the size limit
        foreach (var tag in tags)
        {
            if (!playerHistory.ContainsKey(tag))
            {
                playerHistory[tag] = new Queue<(Vector3, Quaternion)>();
            }

            if (tempData.ContainsKey(tag))
            {
                playerHistory[tag].Enqueue(tempData[tag]);

                if (playerHistory[tag].Count > limit)
                {
                    playerHistory[tag].Dequeue();
                }
            }
        }
    }

    // Determines if the player's view should be mirrored based on their relative facing angle
    bool ShouldMirror(NetworkObject netObject)
    {
        if (!netObject.IsLocalPlayer)  
            return false;

        Vector3 localForward = netObject.transform.forward;
        Vector3 bestMatchForward;
        float smallestAngle = float.MaxValue;

        // Find the player who is directly facing the local player
        foreach (var player in networkPlayers)
        {
            if (player.NetworkObjectId != netObject.NetworkObjectId)
            {
                float angle = Vector3.Angle(localForward, player.transform.forward);
                if (angle < smallestAngle)
                {
                    smallestAngle = angle;
                    bestMatchForward = player.transform.forward;
                }
            }
        }

        // Mirror if the angle between players is between 90 and 180 degrees
        return smallestAngle > 90 && smallestAngle < 180;
    }

    // Helper function - Recursively search for a Transform child object with a specific tag
    Transform FindChildWithTag(Transform parent, string tag)
    {
        foreach (Transform child in parent)
        {
            if (child.CompareTag(tag))
            {
                return child;
            }

            Transform found = FindChildWithTag(child, tag);
            if (found != null)
            {
                return found;
            }
        }
        return null;
    }
}
