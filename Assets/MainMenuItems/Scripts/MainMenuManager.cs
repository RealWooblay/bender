using System;
using FishNet.Managing;
using FishNet.Object;
using UnityEngine;

public class MainMenuManager : MonoBehaviour
{
    //----UI Variables----//
    [SerializeField] private GameObject TitleScreen;
    [SerializeField] private GameObject GameSearchScreen;
    [SerializeField] private GameObject SearchingForGameText;
    [SerializeField] private GameObject GameFoundText;
    [SerializeField] private GameObject ConnectingToServerScreen;
    
    void Start()
    {
        SessionManager SM = FindFirstObjectByType<SessionManager>();
        if (!SM.isDDOLActive)
        {
            OpenConnectingToServerScreen();
            DontDestroyOnLoad(FindFirstObjectByType<NetworkManager>());
            DontDestroyOnLoad(FindFirstObjectByType<SessionManager>());
            return;
        }
        
        OpenTitleScreen();
    }
    
    //-------------------------------//
    //         UI FUNCTIONS          //
    //-------------------------------//
    private void CloseAllScreens()
    {
        TitleScreen.SetActive(false);
        GameSearchScreen.SetActive(false);
        ConnectingToServerScreen.SetActive(false);
    }
    public void OpenTitleScreen()
    {
        CloseAllScreens();
        TitleScreen.SetActive(true);
    }
    public void OpenGameSearchScreen()
    {
        CloseAllScreens();
        GameSearchScreen.SetActive(true);
        SearchingForGameText.SetActive(true);
        GameFoundText.SetActive(false);
    }

    public void OpenConnectingToServerScreen()
    {
        CloseAllScreens();
        ConnectingToServerScreen.SetActive(true);
    }
    public void UIGameFound()
    {
        SearchingForGameText.SetActive(false);
        GameFoundText.SetActive(true);
    }
    
}
