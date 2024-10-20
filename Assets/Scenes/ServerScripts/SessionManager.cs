using System.Collections.Generic;
using FishNet.Managing;
using FishNet.Managing.Server;
using UnityEngine;

public class SessionManager : MonoBehaviour
{
    public GameObject networkManagerPrefab;  // Prefab of the NetworkManager

    // Dictionary to store active game sessions
    private Dictionary<ushort, NetworkManager> activeSessions = new Dictionary<ushort, NetworkManager>();

    private ushort currentPort = 7777;  // Starting port number

    // Create a new game session
    public NetworkManager StartNewSession()
    {
        // Instantiate a new NetworkManager for each session
        GameObject sessionObj = Instantiate(networkManagerPrefab, Vector3.zero, Quaternion.identity);
        NetworkManager sessionManager = sessionObj.GetComponent<NetworkManager>();

        // Assign a unique port for the new session
        sessionManager.TransportManager.Transport.SetPort(currentPort);  // Set the port
        sessionManager.ServerManager.StartConnection();  // Start the server

        // Store the session with its port
        activeSessions.Add(currentPort, sessionManager);

        // Increment port for next session
        currentPort++;

        Debug.Log($"Started new session on port {sessionManager.TransportManager.Transport.GetPort()}");

        return sessionManager;
    }

    // Stop and clean up a session
    public void StopSession(ushort port)
    {
        if (activeSessions.ContainsKey(port))
        {
            NetworkManager sessionManager = activeSessions[port];

            // Stop the server for this session
            sessionManager.ServerManager.StopConnection(true);

            // Clean up
            Destroy(sessionManager.gameObject);
            activeSessions.Remove(port);

            Debug.Log($"Stopped session on port {port}");
        }
        else
        {
            Debug.LogWarning($"Session on port {port} does not exist.");
        }
    }
}