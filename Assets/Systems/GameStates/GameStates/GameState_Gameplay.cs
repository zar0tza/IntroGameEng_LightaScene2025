using UnityEngine;
using UnityEngine.InputSystem;

public class GameState_Gameplay : IState
{
    GameManager gameManager => GameManager.Instance;
    GameStateManager gameStateManager => GameManager.Instance.GameStateManager;
    PlayerController playerController => GameManager.Instance.PlayerController;
    UIManager uIManager => GameManager.Instance.UIManager;


    #region Singleton Instance
    // A single, readonly instance of the atate class is created.
    // The 'readonly' keyword ensures this instance cannot be modified after initialization.
    private static readonly GameState_Gameplay instance = new GameState_Gameplay();

    // Provides global access to the singleton instance of this state.
    // Uses an expression-bodied property to return the static _instance variable.
    public static GameState_Gameplay Instance = instance;
    #endregion



    public void EnterState()
    {


        Time.timeScale = 1f; // Resume  
        
        Cursor.visible = false;

        uIManager.ShowGameplayUI();



    }

    public void FixedUpdateState()
    {

    }

    public void UpdateState()
    {
        playerController.HandlePlayerMovement();

        if (Keyboard.current[Key.Escape].wasPressedThisFrame)
        {
            gameStateManager.Pause();
        }    }

    public void LateUpdateState()
    {
        playerController.HandlePlayerLook();
    }

    public void ExitState()
    {

    }

}
