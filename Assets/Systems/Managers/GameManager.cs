using UnityEngine;


// GameManager must load first to initialize its references before sub-managers
[DefaultExecutionOrder(-100)]

public class GameManager : MonoBehaviour
{
    // Singleton instance of GameManager for global access
    public static GameManager Instance { get; private set; }

    [Header("Manager References (Auto-Assigned)")]
    [SerializeField] private InputManager inputManager;
    [SerializeField] private GameStateManager gameStateManager;
    [SerializeField] private PlayerController playerController;
    [SerializeField] private UIManager uIManager;

    // Public read-only accessors for other scripts to use the managers
    public InputManager InputManager => inputManager;
    public GameStateManager GameStateManager => gameStateManager;
    public PlayerController PlayerController => playerController;
    public UIManager UIManager => uIManager;


    private void Awake()
    {
        #region Singleton
        // Singleton pattern to ensure only one instance of GameManager exists

        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        { 
            Destroy(gameObject);
        }
        #endregion

        // Auto-assign manager references from child objects if not manually assigned
        inputManager ??= GetComponentInChildren<InputManager>();
        gameStateManager ??= GetComponentInChildren<GameStateManager>();
        playerController ??= GetComponentInChildren<PlayerController>();
        uIManager ??= GetComponentInChildren<UIManager>();
    }

  





}
