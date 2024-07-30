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
    public float value;
    public float time;

    public SynchronizationDatum(float value, float time)
    {
        this.value = value;
        this.time = time;
    }
}


public class SynchronizationManager : MonoBehaviour
{
    private const int TargetSampleSize = 1000;
    private Dictionary<ulong, Dictionary<string, Queue<(Vector3 position, Quaternion rotation)>>> history =
        new Dictionary<ulong, Dictionary<string, Queue<(Vector3 position, Quaternion rotation)>>>();

    public static List<NetworkObject> networkPlayers = new List<NetworkObject>();

    public Dictionary<string, Queue<(Vector3 position, Quaternion rotation)>> localPlayerHistory =
        new Dictionary<string, Queue<(Vector3 position, Quaternion rotation)>>();


    public static List<SynchronizationDatum> synchronizationHands = new List<SynchronizationDatum>();
    public static List<SynchronizationDatum> synchronizationPendulum = new List<SynchronizationDatum>();


    private bool headHit = false;
    void Start()
    {
        InvokeRepeating("UpdateNetworkPlayers", 0f, GameManager.rateOfTesting);
    }


    static public List<SynchronizationDatum> NormalizeListSize(List<SynchronizationDatum> originalList)
    {
        if(originalList.Count == 0)
        {
            return originalList;
        }
        
        if (originalList.Count == TargetSampleSize)
        {
            return originalList;
        }

        var normalizedList = new List<SynchronizationDatum>();

        if (originalList.Count < TargetSampleSize)
        {
            // Interpolation
            for (int i = 0; i < TargetSampleSize; i++)
            {
                float t = i / (float)(TargetSampleSize - 1);
                var interpolated = Interpolate(originalList, t);
                normalizedList.Add(interpolated);
            }
        }
        else
        {
            // Downsampling
            for (int i = 0; i < TargetSampleSize; i++)
            {
                float t = i / (float)(TargetSampleSize - 1);
                int index = Mathf.RoundToInt(t * (originalList.Count - 1));
                normalizedList.Add(originalList[index]);
            }
        }

        return normalizedList;
    }

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


    void UpdateNetworkPlayers()
    {
        GameObject[] allObjects = GameObject.FindGameObjectsWithTag("Player");

        if (networkPlayers.Count < 2)
        {
            // Debug.Log("Network players count less than 2");
            foreach (GameObject obj in allObjects)
            {
                NetworkObject netObject = obj.GetComponent<NetworkObject>();
                // Debug.Log("Network Id: " + netObject);
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

        foreach (var netObject in networkPlayers)
        {
            if (!history.ContainsKey(netObject.NetworkObjectId))
            {
                history[netObject.NetworkObjectId] = new Dictionary<string, Queue<(Vector3, Quaternion)>>();
            }

            // Update the general history for all network players
            UpdateHistory(netObject, history[netObject.NetworkObjectId], GameManager.historyLength);

            // Special handling for the local player with a separate history dictionary
            if (netObject.IsLocalPlayer)
            {
                // Update the local player's history without a max records limit
                UpdateHistory(netObject, localPlayerHistory, int.MaxValue);
            }
        }

        if (networkPlayers.Count > 1)
        {
            CheckSynchronization();
        }
    }

    void CheckSynchronization()
    {
        if ((GameManager.currentPhase != (int)Phase.Synchronization && GameManager.currentPhase != (int)Phase.SwingingBall) || networkPlayers.Count < 2)
        {
            // Debug.Log("Not in synchronization phase or not enough players");
            GameManager.ChangeWallPaintColorFunction(Color.white);
            return;
        }
        float pendulumRotationZ = GameManager.Pendulum.transform.rotation.eulerAngles.z;
        if (GameManager.currentPhase == (int)Phase.SwingingBall)
        {
            if (pendulumRotationZ > 180)
            {
                pendulumRotationZ -= 360;
            }

            if (pendulumRotationZ < 60 && pendulumRotationZ > -60)
            {
                GetPendulumSynchronization();
            }
            else
            {
                headHit = false;
            }
        }
        else
        {
            GetHandSynchronization();
            // UpdateSyncPercentageUI(Math.Max(0, syncScore));
        }
    }


    double GetPendulumSynchronization()
    {
        double syncScore = 0;

        PendulumCollisionDetection pendulumCollision = GameManager.Pendulum.GetComponent<PendulumCollisionDetection>();

        if (pendulumCollision.IsOverRug())
        {
            Debug.Log("Ball is above the rug");
            if (headHit)
            {
                syncScore = 0;
            }
            else if (pendulumCollision.IsCollidingWithHead())
            {
                headHit = true;
                StartVibration();
                syncScore = 0;
            }
            else
            {
                syncScore = 100;
            }

            synchronizationPendulum.Add(new SynchronizationDatum((float)syncScore, GameManager.timeRemaining));
            GameManager.ChangeWallPaintColorBasedOnNumber((int)syncScore);
            return syncScore;
        }
        return 0;
    }






    void StartVibration()
    {
        HapticCapabilities capabilities;
        InputDevice device = InputDevices.GetDeviceAtXRNode(XRNode.RightHand);
        if (device.TryGetHapticCapabilities(out capabilities) && capabilities.supportsImpulse)
        {
            uint channel = 0;
            device.SendHapticImpulse(channel, 1.0f, 1.0f); // Send vibration for 1 second
            Invoke("StopVibration", 1.0f); // Stop vibration after 1 second
        }
    }

    void StopVibration()
    {
        InputDevice device = InputDevices.GetDeviceAtXRNode(XRNode.RightHand);
        device.StopHaptics();
    }
    double GetHandSynchronization()
    {
        double syncScore = 0;
        double totalDifference = 0;
        int comparisons = 0;

        const double positionNormalizer = 0.97;
        const double epsilon = 1e-6;

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


        if (comparisons > 0)
        {
            syncScore = Math.Max(0, 100 - Math.Sqrt(Math.Max(0, totalDifference / comparisons + epsilon)));
        }
        else
        {
            syncScore = 0;
        }

        // Debug.Log(syncScore);

        synchronizationHands.Add(new SynchronizationDatum((float)syncScore, GameManager.timeRemaining));
        GameManager.ChangeWallPaintColorBasedOnNumber((int)syncScore);
        return syncScore;
    }


    Queue<(Vector3 position, Quaternion rotation)> NormalizeQueue(Queue<(Vector3 position, Quaternion rotation)> originalQueue)
    {
        var list = originalQueue.ToList();
        for (int i = 1; i < list.Count - 1; i++)
        {
            var prevMove = GetMovement(list[i - 1].position, list[i].position);
            var nextMove = GetMovement(list[i].position, list[i + 1].position);

            if (prevMove == nextMove)
            {
                // Correct the outlier
                list[i] = ((list[i - 1].position + list[i + 1].position) / 2, list[i].rotation);
            }
        }
        return new Queue<(Vector3 position, Quaternion rotation)>(list);
    }

    Movement GetMovement(Vector3 previous, Vector3 current)
    {
        return current.y > previous.y ? Movement.Up : Movement.Down;
    }
    enum Movement
    {
        Up,
        Down
    }


    void UpdateSyncPercentageUI(double score)
    {
        // Find the text GameObject with the tag "SyncPercentage"
        GameObject textObject = GameObject.FindWithTag("SyncPercentage");
        if (textObject != null)
        {
            // Update the text component with the new score
            TextMeshProUGUI syncText = textObject.GetComponent<TextMeshProUGUI>();
            if (syncText != null)
            {
                syncText.text = $"{score:0.00}%";
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

    void UpdateHistory(NetworkObject netObject, Dictionary<string, Queue<(Vector3, Quaternion)>> playerHistory, int limit)
    {
        string[] tags = { "LeftHandTarget", "RightHandTarget" };
        bool shouldMirror = ShouldMirror(netObject);

        Transform headTransform = FindChildWithTag(netObject.transform, "HeadTarget");
        Vector3 netObjectPosition = headTransform.position;
        Quaternion netObjectRotation = headTransform.rotation;

        // Debug.Log($"Player: {netObject.NetworkObjectId}, Mirror: {shouldMirror}, Position: {netObjectPosition}, Rotation: {netObjectRotation}");
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

                if (shouldMirror)
                {
                    worldPosition = new Vector3(-worldPosition.x, worldPosition.y, worldPosition.z);
                    worldRotation = new Quaternion(worldRotation.x, -worldRotation.y, -worldRotation.z, worldRotation.w);
                }

                Vector3 localPosition = Quaternion.Inverse(netObjectRotation) * (worldPosition - netObjectPosition);
                Quaternion localRotation = Quaternion.Inverse(netObjectRotation) * worldRotation;

                if (shouldMirror)
                {
                    localPosition = new Vector3(-localPosition.x, localPosition.y, localPosition.z);
                    localRotation = new Quaternion(-localRotation.x, localRotation.y, localRotation.z, -localRotation.w);
                }

                // Debug.Log($"Player: {netObject.NetworkObjectId}, Tag: {tag}, Local Position: {localPosition}, Local Rotation: {localRotation}"); 
                tempData[tag] = (localPosition, localRotation);
            }
        }

        foreach (var tag in tags)
        {
            if (!playerHistory.ContainsKey(tag))
            {
                playerHistory[tag] = new Queue<(Vector3, Quaternion)>();
            }

            if (tempData.ContainsKey(tag))
            {
                playerHistory[tag].Enqueue(tempData[tag]);

                // Maintain the queue size limit, except for local player with no limit
                if (playerHistory[tag].Count > limit)
                {
                    playerHistory[tag].Dequeue();
                }
            }
        }
    }


    bool ShouldMirror(NetworkObject netObject)
    {
        if (!netObject.IsLocalPlayer)  // Assuming IsLocalPlayer identifies the correct player to mirror
            return false;

        Vector3 localForward = netObject.transform.forward;
        Vector3 bestMatchForward;
        float smallestAngle = float.MaxValue;

        // Assuming access to other players' NetworkObject, refine the selection of the opposing player
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

        // More accurate check: Are they approximately facing each other?
        // Note: Vector3.Angle gives the smallest angle between vectors (0-180 degrees), so for directly facing each other, angle should be close to 180
        return smallestAngle > 90 && smallestAngle < 180;
    }

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