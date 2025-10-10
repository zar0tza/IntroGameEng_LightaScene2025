using UnityEngine;
using UnityEngine.InputSystem;
using static PlayerController;

public class GameState_Paused : IState
{
    GameManager gameManager => GameManager.Instance;
    GameStateManager gameStateManager => GameManager.Instance.GameStateManager;
    PlayerController playerController => GameManager.Instance.PlayerController;
    UIManager uIManager => GameManager.Instance.UIManager;

    [Header("Debug")]
    public bool debugLogsEnabled = false;


    #region Singleton Instance
    // A single, readonly instance of the atate class is created.
    // The 'readonly' keyword ensures this instance cannot be modified after initialization.
    private static readonly GameState_Paused instance = new GameState_Paused();

    // Provides global access to the singleton instance of this state.
    // Uses an expression-bodied property to return the static _instance variable.
    public static GameState_Paused Instance = instance;
    #endregion



    public void EnterState()        
    {
        if(debugLogsEnabled) Debug.Log("Entered Paused State");

        Time.timeScale = 0f; // Pause the game

        Cursor.visible = true;

        uIManager.ShowPauseMenu();




    }

    public void FixedUpdateState()
    {
        
    }

    public void UpdateState()
    {

        if (Keyboard.current[Key.Escape].wasPressedThisFrame)
        {
            gameStateManager.Resume();  
        }
    }

    public void LateUpdateState()
    {
        
    }

    public void ExitState()
    {
        if (debugLogsEnabled) Debug.Log("Exiting Main Menu State");
    }

}
