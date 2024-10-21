using System;
using System.Collections.Generic;
using FishNet.Connection;
using FishNet.Managing;
using FishNet.Managing.Server;
using FishNet.Object;
using NUnit.Framework;
using Unity.Collections;
using UnityEngine;

public class SessionManager : NetworkBehaviour
{
    //----Active Server Variables----//
    private NetworkManager currentNetworkManager; // The networkmanager attached to the connected game session.
    private Dictionary<ushort, NetworkManager> activeSessions = new Dictionary<ushort, NetworkManager>(); // Dictionary to store active game sessions and their ports
    
    //----Server Settings----//
    [Header("Server Settings")]
    [SerializeField] private int maxPlayers = 8;
    [SerializeField, Tooltip("The starting port of the first server, this increments for the next server to be spun up.")] 
    private ushort startingPort = 7778;
    [SerializeField, Tooltip("The port which the main lobby server is hosted on")]
    private ushort lobbyPort = 7777;
    
    //----References----//
    [Header("References")]
    [SerializeField, Tooltip("NetworkManager prefab for each game session")] 
    private GameObject networkManagerPrefab;
    [SerializeField, Tooltip("The main NetworkManager for the mainmenu scene, this connects players to other game sessions.")] 
    private NetworkManager mainNetworkManager;
    
    //-------------------------------//
    //    Start Initial Connection   //
    //-------------------------------//
    
    private void Awake() // Awake() determines whether the script is being run by the server or a client
    {
        if (IsHeadlessMode())
        {
            StartDedicatedServer();
        }
        else
        {
            StartClient();
        }
    }
    private bool IsHeadlessMode()
    {
        return SystemInfo.graphicsDeviceType == UnityEngine.Rendering.GraphicsDeviceType.Null;
    }
    private void StartDedicatedServer()
    {
        Debug.Log("Starting Dedicated Server...");
        mainNetworkManager.ServerManager.StartConnection(lobbyPort);
    }
    private void StartClient()
    {
        Debug.Log("Starting Client...");
        mainNetworkManager.ClientManager.StartConnection();
    }
   
    //-------------------------------//
    //  Management of game sessions  //
    //-------------------------------//
    
    public NetworkManager StartNewSession() // Create a new game session
    {
        // Instantiate a new NetworkManager for each session
        GameObject sessionObj = Instantiate(networkManagerPrefab, Vector3.zero, Quaternion.identity);
        NetworkManager sessionManager = sessionObj.GetComponent<NetworkManager>();
        
        // Assign a unique port for the new session and start the server
        sessionManager.ServerManager.StartConnection(startingPort);

        // Store the session with its port
        activeSessions.Add(startingPort, sessionManager);

        // Increment port for next session
        startingPort++;

        Debug.Log($"Started new session on port {sessionManager.TransportManager.Transport.GetPort()}");

        return sessionManager;
    }
    
    public bool StopSession(ushort port) // Stop and clean up a session
    {
        if (activeSessions.ContainsKey(port))
        {
            NetworkManager sessionManager = activeSessions[port];

            // Try to stop the server for this session
            if (!sessionManager.ServerManager.StopConnection(true))
            {
                Debug.LogError($"Failed to stop server on port {port}");
                return false;
            }

            // Clean up
            Destroy(sessionManager.gameObject);
            activeSessions.Remove(port);

            Debug.Log($"Stopped session on port {port}");
            return true;
        }
        else
        {
            Debug.LogWarning($"Session on port {port} does not exist.");
            return false;
        }
    }

    [Client] 
    public void FindSession()
    {
        FindSessionServer(mainNetworkManager.ClientManager.Connection);
    }

    [ServerRpc(RequireOwnership = false)]
    private void FindSessionServer(NetworkConnection client)
    {
        // Check dictionary for any available servers
        foreach (var server in activeSessions)
        {
            if (server.Value.ClientManager.Clients.Count < maxPlayers)
            {
                // Ensure the session manager is valid on the server side
                if (server.Value == null)
                {
                    Debug.LogError($"Session Manager for port {server.Key} not found on server!");
                    return;
                }

                Debug.Log($"Found available session on port {server.Key}, connecting client...");
                JoinSessionTarget(client, server.Key);
                return;
            }
        }

        // If no sessions are available, create a new one
        NetworkManager newServer = StartNewSession();
        Debug.Log($"No available sessions found, created new session on port {newServer.TransportManager.Transport.GetPort()}.");

        JoinSessionTarget(client, newServer.TransportManager.Transport.GetPort());
    }

    [TargetRpc]
    private void JoinSessionTarget(NetworkConnection client, ushort serverPort)
    {
        // Stop the current connection to the lobby
        if (mainNetworkManager.ClientManager.Connection.IsActive)
        {
            Debug.Log("Stopping current connection to the lobby before joining new game session...");
            mainNetworkManager.ClientManager.StopConnection();
        }

        // Start the connection to the game session
        Debug.Log($"Client is attempting to connect to game session on port {serverPort}");
        if (mainNetworkManager.ClientManager.StartConnection(serverPort))
        {
            Debug.Log($"Client successfully connected to game session on port {serverPort}");
            currentNetworkManager = activeSessions[serverPort];
        }
    }

    [Client]
    private void LeaveSession()
    {
        // Ensures that server is connected or exists
        if (currentNetworkManager == null)
        {
            Debug.LogError("Tried to leave server but currentNetworkManager is null");
            return;
        }
        
        // Stop connection to game session
        if (currentNetworkManager.ClientManager.Connection.IsActive)
        {
            Debug.Log("Stopping current connection to the game session before joining lobby...");
            
            // Ensures that the connection is stopped successfully
            if (!currentNetworkManager.ClientManager.StopConnection()) {
                Debug.LogError($"Client failed to disconnect to game session on port {currentNetworkManager.TransportManager.Transport.GetPort()}");
                return;
            }
            currentNetworkManager = null;
        }
        
        // Start the connection to the lobby
        Debug.Log($"Client is attempting to connect to lobby on port {lobbyPort}");
        if (mainNetworkManager.ClientManager.StartConnection(lobbyPort)) {
            Debug.Log($"Client successfully connected to game session on port {lobbyPort}");
        } else {
            Debug.LogError($"Client failed to connect to lobby on port {lobbyPort}");
        }
    }
}