using UnityEngine;

public class MainMenuManager : MonoBehaviour
{
    // UI Variables
    [SerializeField] private GameObject TitleScreen;
    [SerializeField] private GameObject GameSearchScreen;
    [SerializeField] private GameObject SearchingForGameText;
    [SerializeField] private GameObject GameFoundText;
    
    void Start()
    {
        OpenTitleScreen();
    }

    //------UI FUNCTIONS------//
    private void CloseAllScreens()
    {
        TitleScreen.SetActive(false);
        GameSearchScreen.SetActive(false);
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
    public void UIGameFound()
    {
        SearchingForGameText.SetActive(false);
        GameFoundText.SetActive(true);
    }
    
}
