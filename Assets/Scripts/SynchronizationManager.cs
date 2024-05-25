using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using System.Linq;
using System;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using TMPro;

public class SynchronizationManager : MonoBehaviour
{
    private Dictionary<ulong, Dictionary<string, Queue<(Vector3 position, Quaternion rotation)>>> history =
        new Dictionary<ulong, Dictionary<string, Queue<(Vector3 position, Quaternion rotation)>>>();


    public  Dictionary<string, Queue<(Vector3 position, Quaternion rotation)>> localPlayerHistory =
        new Dictionary<string, Queue<(Vector3 position, Quaternion rotation)>>();

    private List<NetworkObject> networkPlayers = new List<NetworkObject>();
    private float updateInterval = 0.1f; // Update interval in seconds
    private const int maxRecords = 5; // Maximum history records

    void Start()
    {
        InvokeRepeating("UpdateNetworkPlayers", 0f, updateInterval);
    }

    void UpdateNetworkPlayers()
    {
        GameObject[] allObjects = GameObject.FindGameObjectsWithTag("Player");
        networkPlayers.Clear();

        foreach (GameObject obj in allObjects)
        {
            NetworkObject netObject = obj.GetComponent<NetworkObject>();
            if (netObject != null)
            {
                networkPlayers.Add(netObject);
                if (!history.ContainsKey(netObject.NetworkObjectId))
                {
                    history[netObject.NetworkObjectId] = new Dictionary<string, Queue<(Vector3, Quaternion)>>();
                }
                // Update the general history for all network players
                UpdateHistory(netObject, history[netObject.NetworkObjectId], maxRecords);

                // Special handling for the local player with a separate history dictionary
                if (netObject.IsLocalPlayer)
                {
                    // Update the local player's history without a max records limit
                    UpdateHistory(netObject, localPlayerHistory, int.MaxValue);
                }
            }
        }

        if (networkPlayers.Count > 1)
        {
            CheckSynchronization();
        }
    }

    void CheckSynchronization()
    {
        if (networkPlayers.Count < 2)
        {
            UpdateSyncPercentageUI(0); // No comparison possible, set synchronization to 0%
            return;
        }

        double totalDifference = 0;
        int comparisons = 0;

        const double angleScale = 10.0;
        const double angleNormalizer = 0.5;
        const double epsilon = 1e-6;

        for (int i = 0; i < networkPlayers.Count; i++)
        {
            for (int j = i + 1; j < networkPlayers.Count; j++)
            {
                ulong id1 = networkPlayers[i].NetworkObjectId;
                ulong id2 = networkPlayers[j].NetworkObjectId;

                foreach (var tag in new[] { "LeftHandTarget", "RightHandTarget" })
                {
                    if (!history[id1].ContainsKey(tag) || !history[id2].ContainsKey(tag)) continue;

                    var queue1 = history[id1][tag];
                    var queue2 = history[id2][tag];
                    if (queue1.Count == 0 || queue2.Count == 0) continue;

                    double averageAngleDifference = 0;

                    var array1 = queue1.ToArray();
                    var array2 = queue2.ToArray();
                    int count = Math.Min(array1.Length, array2.Length);

                    for (int k = 0; k < count; k++)
                    {
                        double angleDiff = Quaternion.Angle(array1[k].rotation, array2[k].rotation) / 180.0;
                        averageAngleDifference += Math.Max(0, Math.Pow(angleNormalizer + angleDiff, 4) - angleNormalizer);
                    }

                    averageAngleDifference /= count;
                    totalDifference += (averageAngleDifference * angleScale);
                    comparisons++;
                }
            }
        }

        double syncScore = 100;
        if (comparisons > 0)
        {
            syncScore = 100 - Math.Sqrt(Math.Max(0, totalDifference / comparisons + epsilon));
        }
        else
        {
            syncScore = 0; // No valid comparisons
        }

        UpdateSyncPercentageUI(Math.Max(0, syncScore));
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
        Vector3 bestMatchForward = Vector3.zero;
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

    Vector3 CalculateVelocity(Queue<(Vector3 position, Quaternion rotation)> history)
    {
        if (history.Count < 2) return Vector3.zero;

        var positions = history.Select(h => h.position).ToArray();
        var times = Enumerable.Range(0, positions.Length).Select(i => i * updateInterval).ToArray();

        Vector3 velocity = (positions.Last() - positions.First()) / (times.Last() - times.First());
        return velocity;
    }

    string CalculateDirection(Vector3 velocity)
    {
        if (velocity.y > 0) return "Up";
        else if (velocity.y < 0) return "Down";
        else return "Neutral";
    }


}