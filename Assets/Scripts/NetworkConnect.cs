using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using Unity.Netcode.Transports.UTP;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using System;
using System.Threading.Tasks;

public class NetworkConnect : MonoBehaviour
{
    public int maxConnection = 20;
    public UnityTransport transport;
    public bool activateMultiplayer;

    private Lobby currentLobby;
    private float heartBeatTimer;
    private int maxRetries = 10; // Maximum number of retries
    private float retryDelay = 2.0f; // Delay in seconds between retries

    private async void Awake()
    {
        if (!activateMultiplayer)
            return;

        try
        {
            await UnityServices.InitializeAsync();
            if (!AuthenticationService.Instance.IsSignedIn)
                await AuthenticationService.Instance.SignInAnonymouslyAsync();

            await JoinOrCreate();
        }
        catch (Exception e)
        {
            Debug.LogError($"Unity Services Initialization failed: {e.Message}");
        }
    }

    public async Task JoinOrCreate(int retryCount = 0)
    {
        string lobbyName = "SyncVR";
        while (retryCount < maxRetries)
        {
            try
            {
                QueryLobbiesOptions options = new QueryLobbiesOptions
                {
                    Count = 1,
                    Filters = new List<QueryFilter>()
                {
                    new QueryFilter(
                        field: QueryFilter.FieldOptions.Name,
                        op: QueryFilter.OpOptions.EQ,
                        value: lobbyName)
                }
                };

                QueryResponse lobbies = await Lobbies.Instance.QueryLobbiesAsync(options);
                if (lobbies.Results.Count > 0)
                {
                    currentLobby = lobbies.Results[0];
                    if (currentLobby.Data.ContainsKey("JOIN_CODE"))
                    {
                        string relayJoinCode = currentLobby.Data["JOIN_CODE"].Value;
                        JoinAllocation allocation = await RelayService.Instance.JoinAllocationAsync(relayJoinCode);
                        transport.SetClientRelayData(allocation.RelayServer.IpV4, (ushort)allocation.RelayServer.Port,
                            allocation.AllocationIdBytes, allocation.Key, allocation.ConnectionData, allocation.HostConnectionData);
                        NetworkManager.Singleton.StartClient();
                    }
                    return; // Exit loop on success
                }
                else
                {
                    await Create(lobbyName);
                    return; // Assume creation is successful and break
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"Attempt {retryCount + 1} failed: {e.Message}");
                retryCount++;
                await Task.Delay(TimeSpan.FromSeconds(retryDelay));
            }
        }

        try{
            await Create(lobbyName);
        }
        catch (Exception e)
        {
            Debug.LogError("Maximum retry attempts reached, failing...");
        }
    }


    public async Task Create(string lobbyName)
    {
        try
        {
            Allocation allocation = await RelayService.Instance.CreateAllocationAsync(maxConnection);
            string newJoinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);

            Debug.Log(newJoinCode);

            transport.SetHostRelayData(allocation.RelayServer.IpV4, (ushort)allocation.RelayServer.Port,
                allocation.AllocationIdBytes, allocation.Key, allocation.ConnectionData);

            CreateLobbyOptions lobbyOptions = new CreateLobbyOptions();
            lobbyOptions.IsPrivate = false;
            lobbyOptions.Data = new Dictionary<string, DataObject>();
            DataObject dataObject = new DataObject(DataObject.VisibilityOptions.Public, newJoinCode);
            lobbyOptions.Data.Add("JOIN_CODE", dataObject);

            currentLobby = await Lobbies.Instance.CreateLobbyAsync(lobbyName, maxConnection, lobbyOptions);

            NetworkManager.Singleton.StartHost();
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to create lobby {lobbyName}: {e.Message}");
            throw;  // Re-throw the exception to handle it further up the call stack if necessary
        }
    }


    private void Update()
    {
        if (!activateMultiplayer || currentLobby == null)
            return;

        heartBeatTimer += Time.deltaTime;
        if (heartBeatTimer > 5)
        {
            heartBeatTimer -= 5;
            if (currentLobby.HostId == AuthenticationService.Instance.PlayerId)
                LobbyService.Instance.SendHeartbeatPingAsync(currentLobby.Id);
        }
    }
}
