using System;
using System.Collections.Generic;
using FishNet.Connection;
using FishNet.Managing;
using FishNet.Managing.Server;
using FishNet.Object;
using FishNet.Transporting;
using NUnit.Framework;
using Unity.Collections;
using UnityEngine;

public class SessionManager : MonoBehaviour
{
    //----Active Server Variables----//
    [HideInInspector] 
    public ushort currentNetworkManagerPort = 0; // The networkmanager's port attached to the connected game session.
    [HideInInspector] 
    public Dictionary<ushort, NetworkManager> activeSessions = new Dictionary<ushort, NetworkManager>(); // Dictionary to store active game sessions and their ports
    
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
    [Tooltip("The main NetworkManager for the mainmenu scene, this connects players to other game sessions.")] 
    public NetworkManager mainNetworkManager;
    [HideInInspector, Tooltip("Used to identify whether DoNotDestroyOnLoad has been called already or not within the MainMenuManager class for the NetworkManager")]
    public bool isDDOLActive = false;
    
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
        if (!mainNetworkManager.ClientManager.StartConnection(lobbyPort))
        {
            Debug.LogError($"Failed to connect to lobby server with the IP[{mainNetworkManager.TransportManager.Transport.GetServerBindAddress(IPAddressType.IPv4)}] and Port[{lobbyPort}]");
            return;
        }
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
    
    public void FindSession()
    {
        FindFirstObjectByType<SessionNetworkManager>().FindSessionServer(mainNetworkManager.ClientManager.Connection, maxPlayers);
    }
    
    private void LeaveSession()
    {
        // Ensures that server is connected or exists
        if (currentNetworkManagerPort == 0)
        {
            Debug.LogError("Tried to leave server but currentNetworkManager is not set/== 0");
            return;
        }
        
        // Stop connection to game session
        if (mainNetworkManager.ClientManager.Connection.IsActive)
        {
            Debug.Log("Stopping current connection to the game session before joining lobby...");
            
            // Ensures that the connection is stopped successfully
            if (!mainNetworkManager.ClientManager.StopConnection()) {
                Debug.LogError($"Client failed to disconnect to game session on port {currentNetworkManagerPort}");
                return;
            }
            currentNetworkManagerPort = 0;
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