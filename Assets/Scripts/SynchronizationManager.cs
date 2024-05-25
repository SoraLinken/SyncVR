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

        List<double> synchronyScores = new List<double>();

        const int sampleSize = 20; // Example sample size
        const double timeInterval = 0.2; // Example time interval in seconds
        const double synchronyThreshold = 60.0; // Example synchrony threshold

        while (dataStreamsActive())
        {
            // Step 2.1: Collect data points
            var user1Samples = CollectSamples(networkPlayers[0], sampleSize, timeInterval);
            var user2Samples = CollectSamples(networkPlayers[1], sampleSize, timeInterval);

            // Step 2.2: Normalize data points
            NormalizeData(user1Samples);
            NormalizeData(user2Samples);

            // Step 2.3: Calculate Pearson correlation coefficient
            double pearsonCorrelation = CalculatePearsonCorrelation(user1Samples, user2Samples);

            // Step 2.4: Calculate Spearman's rank correlation
            double spearmanCorrelation = CalculateSpearmanCorrelation(user1Samples, user2Samples);

            // Step 2.5: Perform Frequency Analysis
            double frequencyAnalysisScore = PerformFrequencyAnalysis(user1Samples, user2Samples);

            // Step 2.6: Combine measurements into a composite synchrony score
            double compositeScore = CalculateCompositeScore(pearsonCorrelation, spearmanCorrelation, frequencyAnalysisScore);

            // Step 2.7: Check if users are synchronized for this segment
            if (compositeScore > synchronyThreshold)
            {
                synchronyScores.Add(compositeScore);
            }
        }

        // Calculate the overall synchronization score
        double overallSyncScore = synchronyScores.Any() ? synchronyScores.Average() : 0;
        UpdateSyncPercentageUI(Math.Max(0, overallSyncScore));
    }

    bool dataStreamsActive()
    {
        // Implement logic to check if data streams are active
        return true; // Placeholder
    }

    List<Vector3> CollectSamples(NetworkPlayer player, int sampleSize, double timeInterval)
    {
        // Implement logic to collect 'sampleSize' data points from the player's data stream at 'timeInterval' intervals
        return new List<Vector3>(); // Placeholder
    }

    void NormalizeData(List<Vector3> samples)
    {
        // Implement logic to normalize the data points in the sample
    }

    double CalculatePearsonCorrelation(List<Vector3> samples1, List<Vector3> samples2)
    {
        // Implement Pearson correlation calculation
        return 0.0; // Placeholder
    }

    double CalculateSpearmanCorrelation(List<Vector3> samples1, List<Vector3> samples2)
    {
        // Implement Spearman's rank correlation calculation
        return 0.0; // Placeholder
    }

    double PerformFrequencyAnalysis(List<Vector3> samples1, List<Vector3> samples2)
    {
        // Implement frequency analysis using Fourier Transform and compare dominant frequencies
        return 0.0; // Placeholder
    }

    double CalculateCompositeScore(double pearson, double spearman, double frequencyAnalysis)
    {
        // Assign weights and calculate the weighted average of the scores
        const double pearsonWeight = 0.4;
        const double spearmanWeight = 0.4;
        const double frequencyWeight = 0.2;
        
        return (pearson * pearsonWeight) + (spearman * spearmanWeight) + (frequencyAnalysis * frequencyWeight);
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

}