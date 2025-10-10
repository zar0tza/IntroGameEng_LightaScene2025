using UnityEngine;
using UnityEngine.UIElements;

public class UIManager : MonoBehaviour
{
    public GameObject gameplayUI;
    public GameObject pauseUI;

    private void Awake()
    {
        HideAllUIMenus();
    }


    

    public void ShowPauseMenu()
    {
        HideAllUIMenus();
        pauseUI.SetActive(true);
    }

    public void ShowGameplayUI()
    {
        HideAllUIMenus();
        gameplayUI.SetActive(true);
    }

    public void HideAllUIMenus()
    {
        gameplayUI.SetActive(false);
        pauseUI.SetActive(false);
    }





}
