using UnityEngine;
using static PlayerController;

public class GameStateManager : MonoBehaviour
{
    [Header("Debug (read only)")]
    public bool debugLogsEnabled = false;
    [SerializeField] private string currentActiveState;
    [SerializeField] private string lastActiveState;

    // Private variables to store state information
    private IState currentState;  // Current active state
    private IState lastState;     // Last active state (kept private for encapsulation)

    // Instantiate GameStates
    public GameState_Gameplay gameState_Gameplay = GameState_Gameplay.Instance;
    public GameState_Paused gameState_Paused = GameState_Paused.Instance;

    private void Start()
    {
        currentState = gameState_Gameplay; // Set initial state to Gameplay
        currentActiveState = currentState.ToString(); // Update debug info in inspector
        currentState.EnterState();
    }

    public void SwitchToState(IState newState)
    {
        lastState = currentState; // Store the current state as the last state
        lastActiveState = lastState.ToString(); // Update debug info in inspector
        currentState?.ExitState(); // Exit the current state

        currentState = newState; // Switch to the new state
        currentActiveState = currentState.ToString(); // Update debug info in inspector
        currentState.EnterState(); // Enter the new state
    }






    #region State Machine Update Calls

    private void FixedUpdate()
    {
        // Handle physics updates in the current active state
        currentState.FixedUpdateState();

    }


    private void Update()
    {
        // Handle regular frame updates in the current active state
        currentState.UpdateState();
    }


    private void LateUpdate()
    {
        // Handle late frame updates in the current active state
        currentState.LateUpdateState();
    }
    #endregion

    #region Button Call Methods

    public void Pause()
    {
        if (currentState != gameState_Gameplay)
            return;

        if(currentState == gameState_Gameplay)
        {
            SwitchToState(gameState_Paused);
            return;
        }
    }

    public void Resume()
    {
        if (currentState != gameState_Paused)
            return;

        if (currentState == gameState_Paused)
        {
            SwitchToState(gameState_Gameplay);
            return;
        }
    }

    public void Play()
    {
        SwitchToState(gameState_Gameplay);

    }



    public void Quit()
    {
        Application.Quit();

    }




    #endregion


}
