using System;
using System.Collections.Generic;
using FishNet.Connection;
using FishNet.Managing;
using FishNet.Object;
using UnityEngine;

public class SessionNetworkManager : NetworkBehaviour
{
    private SessionManager SM; // Reference to Session Manager
    private void Awake() // Sets reference to Session Manager
    {
        SM = FindFirstObjectByType<SessionManager>();
    }

    //-------------------------------//
    //  Management of game sessions  //
    //        Over The Network       //
    //-------------------------------//
    
    [ServerRpc(RequireOwnership = false)]
    public void FindSessionServer(NetworkConnection client, int maxPlayers)
    {
        
        
        // Check dictionary for any available servers
        foreach (var server in SM.activeSessions)
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
        NetworkManager newServer = SM.StartNewSession();
        Debug.Log($"No available sessions found, created new session on port {newServer.TransportManager.Transport.GetPort()}.");

        JoinSessionTarget(client, newServer.TransportManager.Transport.GetPort());
    }

    [TargetRpc]
    private void JoinSessionTarget(NetworkConnection target, ushort serverPort)
    {
        // Update UI to show a game has been found
        FindFirstObjectByType<MainMenuManager>().UIGameFound();
        
        // Stop the current connection to the lobby
        if (SM.mainNetworkManager.ClientManager.Connection.IsActive)
        {
            Debug.Log("Stopping current connection to the lobby before joining new game session...");
            SM.mainNetworkManager.ClientManager.StopConnection();
        }

        // Start the connection to the game session
        Debug.Log($"Client is attempting to connect to game session on port {serverPort}");
        if (SM.mainNetworkManager.ClientManager.StartConnection(serverPort))
        {
            SM.currentNetworkManagerPort = serverPort;
            Debug.Log($"Client successfully connected to game session on port {serverPort}");
        }
    }
}
