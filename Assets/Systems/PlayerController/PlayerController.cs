using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    // Manager References
    private InputManager inputManager => GameManager.Instance.InputManager;
    private CharacterController characterController => GetComponent<CharacterController>();

    [SerializeField] private Transform cameraRoot;
    public Transform CameraRoot => cameraRoot;

    public enum MovementState
    {
        Idle,
        Walking,
        Sprinting,
        Crouching,
        Jumping, // Or integrate into velocity checks
        Falling
    }

    
    

    [Header("Debug")]
    public bool debugLogsEnabled = false;
    public MovementState currentMovementState;
    public float characterVelocity;

    [Header("Enable/Disable Controls & Features")]
    public bool moveEnabled = true;
    public bool lookEnabled = true;
    

    [SerializeField] private bool jumpEnabled = true;

    [SerializeField] private bool sprintEnabled = true;
    [SerializeField] private bool holdToSprint = true;  // true = HOLD to sprint, false = TOGGLE to sprint

    [SerializeField] private bool crouchEnabled = true;
    [SerializeField] private bool holdToCrouch = true;  // true = HOLD to crouch, false = TOGGLE to crouch







    [Header("Move Settings")]
    //public float moveSpeed = 5;
    [SerializeField] private float crouchMoveSpeed = 1.25f;
    [SerializeField] private float walkMoveSpeed = 3.0f;
    [SerializeField] private float sprintMoveSpeed = 6.0f;

    private float speedTransitionDuration = 0.25f; // Time in seconds for speed transitions
    [SerializeField] private float currentMoveSpeed; // Tracks the current interpolated speed

    [SerializeField] private bool sprintInput = false;
    [SerializeField] private bool crouchInput = false;

    private Vector3 velocity; // Used for vertical movement (jumping/gravity)


    [Header("Look Settings")]
    public float horizontalLookSensitivity = 30;
    public float verticalLookSensitivity = 30;
    public float LowerLookLimit = -60;
    public float upperLookLimit = 60;
    public bool invertLookY { get; private set; } = false;


    [Header("Jump & Gravity Settings")]
    [SerializeField] private bool isGrounded;
    [SerializeField] private float gravity = 30.0f;
    [SerializeField] private float jumpHeight = 2.0f;
    private float jumpCooldownAmount = 0.2f; // Time before allowing another jump    
    private float jumpCooldownTimer = 0f;
    private bool jumpRequested = false;
    //private float groundCheckRadius = 0.1f; // Radius for ground check sphere


    [Header("Crouch Settings")]
    [SerializeField] private float crouchTransitionDuration = 0.5f; // Time in seconds for crouch/stand transition (approximate completion)
    [SerializeField] private float crouchingHeight = 1.0f;
    [SerializeField] private Vector3 crouchingCenter = new Vector3(0, 0.5f, 0);
    [SerializeField] private float crouchingCamY = 0.75f;
    private float standingHeight;
    private Vector3 standingCenter;
    private float standingCamY;
    private bool isObstructed = false;


    private float targetHeight;
    private Vector3 targetCenter;
    private float targetCamY; // Target Y position for camera root during crouch transition

    private int playerLayerMask;

    public Transform spawnPosition;

    // Input Variables
    private Vector2 moveInput;
    private Vector2 lookInput;

    private void Awake()
    {
        playerLayerMask = ~LayerMask.GetMask("Player");



        #region Initialize Default values
        currentMovementState = MovementState.Idle;

        // Initialize crouch variables
        standingHeight = characterController.height;
        standingCenter = characterController.center;
        standingCamY = cameraRoot.localPosition.y;

        targetHeight = standingHeight;
        targetCenter = standingCenter;
        targetCamY = cameraRoot.localPosition.y;

        // set default state of bools
        crouchInput = false;
        sprintInput = false;

        #endregion
    }

    private void Start()
    {
        MovePlayerToSpawnpoint();
    }


    public void HandlePlayerMovement()
    {
        characterVelocity = characterController.velocity.magnitude;

        if (moveEnabled == false) return; // Check if movement is enabled

        // DetermineMovementState
        DetermineMovemementState();

        // perform Ground Check
        GroundedCheck();

        // Handle Crouch Transition
        HandleCrouchTransition();

        // Apply Movement
        ApplyMovement();        
    }

    private void DetermineMovemementState()
    {
        // determine current movement state based on inputs and conditions

        // if the player is not grounded, they are either jumping or falling
        if (isGrounded == false)
        {
            // check if the player is moving upwards (jumping) or downwards (falling)
            if (velocity.y > 0.1f)
            {
                currentMovementState = MovementState.Jumping;
            }
            else if (velocity.y < 0)
            {
                currentMovementState = MovementState.Falling;
            }
        }

        else if (isGrounded == true)
        {
            if (crouchInput == true || isObstructed == true)
            {
                currentMovementState = MovementState.Crouching;
            }

            // sprint check
            else if (sprintInput == true && currentMovementState != MovementState.Crouching)
            {
                currentMovementState = MovementState.Sprinting;
            }

            // walk check
            else if (moveInput.magnitude > 0.1f && sprintInput == false && crouchInput == false)
            {
                currentMovementState = MovementState.Walking;
            }

            // Idle Check
            else if (moveInput.magnitude <= 0.1f)
            {
                currentMovementState = MovementState.Idle;
            }
        }
    }
    private void ApplyMovement()
    {
        // Step 1: Get input direction
        Vector3 moveInputDirection = new Vector3(moveInput.x, 0, moveInput.y);
        Vector3 worldMoveDirection = transform.TransformDirection(moveInputDirection);

        // Step 2: Determine movement speed
        float targetMoveSpeed;

        switch (currentMovementState)
        {
            case MovementState.Crouching: { targetMoveSpeed = crouchMoveSpeed; break; }
            case MovementState.Sprinting: { targetMoveSpeed = sprintMoveSpeed; break; }
            default: { targetMoveSpeed = walkMoveSpeed; break; }
        }

        // Step 3: Smoothly interpolate current speed towards target speed
        float lerpSpeed = 1f - Mathf.Pow(0.01f, Time.deltaTime / speedTransitionDuration);
        currentMoveSpeed = Mathf.Lerp(currentMoveSpeed, targetMoveSpeed, lerpSpeed);


        // Step 4: Handle horizontal movement
        Vector3 horizontalMovement = worldMoveDirection * currentMoveSpeed;

        // Step 5: Handle jumping and gravity
        ApplyJumpAndGravity();

        // Step 6: Combine horizontal and vertical movement
        Vector3 movement = horizontalMovement;
        movement.y = velocity.y;

        // Step 7: Apply final movement
        characterController.Move(movement * Time.deltaTime);
    }
    public void HandlePlayerLook()
    {
        if (lookEnabled == false) return; // Check if look is enabled 

        float lookX = lookInput.x * horizontalLookSensitivity * Time.deltaTime;
        float lookY = lookInput.y * verticalLookSensitivity * Time.deltaTime;

        // Invert vertical look if needed
        if (invertLookY)
        {
            lookY = -lookY;
        }

        // Rotate character on Y-axis (left/right look)
        transform.Rotate(Vector3.up * lookX);

        // Tilt cameraRoot on X-axis (up/down look)
        Vector3 currentAngles = cameraRoot.localEulerAngles;
        float newRotationX = currentAngles.x - lookY;

        // Convert to signed angle for proper clamping
        newRotationX = (newRotationX > 180) ? newRotationX - 360 : newRotationX;
        newRotationX = Mathf.Clamp(newRotationX, LowerLookLimit, upperLookLimit);

        CameraRoot.localEulerAngles = new Vector3(newRotationX, 0, 0);

    }
    private void ApplyJumpAndGravity()
    {

        // Process jump if...
        //  + Jump Requested (via input)
        //  + Player is currently grounded
        //  + Player is not crouching

        if (jumpRequested == true)
        {
            // Calculate the initial upward velocity needed to reach the desired jumpHeight.
            velocity.y = Mathf.Sqrt(2f * jumpHeight * gravity);

            // Reset the jump request flag so it only triggers once per button press.
            jumpRequested = false;

            // Start the jump cooldown timer to prevent immediate re-jumping.
            jumpCooldownTimer = jumpCooldownAmount;
        }


        // Apply gravity based on the player's current state (grounded or in air).
        if (isGrounded && velocity.y < 0)
        {
            // If grounded and moving downwards (due to accumulated gravity from previous frames),
            // snap velocity to a small negative value. This keeps the character firmly on the ground
            // without allowing gravity to build up indefinitely, preventing "bouncing" or
            // incorrect ground detection issues.

            velocity.y = -1f;
        }
        else  // If not grounded (in the air):
        {
            // apply standard gravity
            velocity.y -= gravity * Time.deltaTime;
        }


        // Update jump cooldown timer
        if (jumpCooldownTimer > 0)
        {
            jumpCooldownTimer -= Time.deltaTime;
        }

    }
    private void HandleCrouchTransition()
    {
        bool shouldCrouch = crouchInput == true && currentMovementState != MovementState.Jumping && currentMovementState != MovementState.Falling;

        // if airborne and was crouching, maintain crouch state (prevents standing up from crouch while walking off a ledge)
        bool wasAlreadyCrouching = characterController.height < (standingHeight - 0.05f);

        if (isGrounded == false && wasAlreadyCrouching)
        {
            shouldCrouch = true; // Maintain crouch state if airborne (walking off ledge while crouching)
        }

        if (shouldCrouch)
        {
            targetHeight = crouchingHeight;
            targetCenter = crouchingCenter;
            targetCamY = crouchingCamY;
            isObstructed = false; // No obstruction when intentionally crouching
        }
        else
        {
            float maxAllowedHeight = GetMaxAllowedHeight();

            if (maxAllowedHeight >= standingHeight - 0.05f)
            {
                // No obstruction, allow immediate transition to standing
                targetHeight = standingHeight;
                targetCenter = standingCenter;
                targetCamY = standingCamY;
                isObstructed = false;
            }

            else
            {
                // Obstruction detected, limit height and center
                targetHeight = Mathf.Min(standingHeight, maxAllowedHeight);
                float standRatio = Mathf.Clamp01((targetHeight - crouchingHeight) / (standingHeight - crouchingHeight));
                targetCenter = Vector3.Lerp(crouchingCenter, standingCenter, standRatio);
                targetCamY = Mathf.Lerp(crouchingCamY, standingCamY, standRatio);
                isObstructed = true;
            }
        }

        // Calculate lerp speed based on desired duration
        // This formula ensures the transition approximately reaches 99% of the target in 'crouchTransitionDuration' seconds.
        float lerpSpeed = 1f - Mathf.Pow(0.01f, Time.deltaTime / crouchTransitionDuration);

        // Smoothly transition to targets
        characterController.height = Mathf.Lerp(characterController.height, targetHeight, lerpSpeed);
        characterController.center = Vector3.Lerp(characterController.center, targetCenter, lerpSpeed);

        Vector3 currentCamPos = cameraRoot.localPosition;
        cameraRoot.localPosition = new Vector3(currentCamPos.x, Mathf.Lerp(currentCamPos.y, targetCamY, lerpSpeed), currentCamPos.z);

    }

    #region Helper Methods

    // Ground Check that requires LayerMask "Ground" to be set in the Unity Editor
    private void GroundedCheck()
    {
        isGrounded = characterController.isGrounded;

      
        /*

        bool previouslyGrounded = isGrounded; // Store previous grounded state

      
            // Start the ray slightly above the player's feet
            Vector3 rayOrigin = transform.position + Vector3.up * 0.1f;
            float groundCheckDistance = (characterController.height / 2f) + 0.2f;

            // Perform raycast
            isGrounded = Physics.Raycast(rayOrigin, Vector3.down, groundCheckDistance, ~0, QueryTriggerInteraction.Ignore);
    



        // Detect if the player just left the ground
        if (isGrounded == false && previouslyGrounded == true)
        {
            if(debugLogsEnabled) Debug.Log("Player just left ground");
        }

        // Detect if the player just landed
        if (isGrounded == true && previouslyGrounded == false)
        {
            if(debugLogsEnabled) Debug.Log($"Player just landed at Y position: {transform.position.y}");
        }

        */

       
    }

    private float GetMaxAllowedHeight()
    {
        // Cast a ray upwards from the character's position to check for obstructions.

        RaycastHit hit;
        float maxCheckDistance = standingHeight + 0.15f;

        // fire raycast
        if (Physics.Raycast(transform.position, Vector3.up, out hit, maxCheckDistance, playerLayerMask))
        {
            // We hit something, so calculate the maximum height the playe can stand

            // Subtract a small buffer to prevent clipping
            float maxHeight = hit.distance - 0.1f;

            // Ensure we don't go below crouching height
            maxHeight = Mathf.Max(maxHeight, crouchingHeight);

            if (debugLogsEnabled) Debug.Log($"Overhead obstruction detected. Max allowed height: {maxHeight:F2}");
            return maxHeight;
        }

        // No obstruction found, can stand at full height
        return standingHeight;
    }



    public void MovePlayerToSpawnPosition(Transform spawnPosition)
    {
        if(debugLogsEnabled) Debug.Log("Moving player to Spawn Position");

        characterController.enabled = false;
        transform.position = spawnPosition.position;
        transform.rotation = spawnPosition.rotation;
        characterController.enabled = true;
    }

    #endregion



    #region Input Methods

    void SetMoveInput(Vector2 inputVector)
    {
        moveInput = new Vector2(inputVector.x, inputVector.y);
    }

    void SetLookInput(Vector2 inputVector)
    {
        lookInput = new Vector2(inputVector.x, inputVector.y);
    }


    void HandleJumpInput(InputAction.CallbackContext context)
    {
        if (jumpEnabled == false) return; // if Jump is not enabled, do nothing and just return;

        if (context.started)
        {
            if (debugLogsEnabled) Debug.Log("Jump Input Started");

            if (isGrounded && jumpCooldownTimer <= 0f)
            {
                jumpRequested = true;

                // Immediately set a small "input buffer" cooldown to prevent spam
                jumpCooldownTimer = 0.1f;
            }
        }

    }

    void HandleCrouchInput(InputAction.CallbackContext context)
    {
        // if Crouch is not enabled, do nothing and just return;
        if (crouchEnabled == false) return;

        if (context.started)
        {
            if (holdToCrouch == true)
            {
                crouchInput = true;
            }

            // If holdToCrouch is false, Crouch will revert to toggle mode
            else if (holdToCrouch == false)
            {
                crouchInput = !crouchInput;
            }
        }

        else if (context.canceled)
        {
            // Only update crouchInput if holdToCrouch is enabled, otherwise in toggle mode we'll ignore the input canceled / button release
            if (holdToCrouch == true)
            {
                crouchInput = false;
            }
        }
    }

    void HandleSprintInput(InputAction.CallbackContext context)
    {
        // if Sprint is not enabled, do nothing and just return
        if (sprintEnabled == false) return;

        if (context.started)
        {
            if (holdToSprint == true)
            {
                sprintInput = true;
            }

            // If holdToSprint is false, Sprint will revert to toggle mode
            else if (holdToSprint == false)
            {
                sprintInput = !sprintInput;
            }
        }

        else if (context.canceled)
        {
            // Only update sprintInput if holdToSprint is enabled, otherwise in toggle mode we'll ignore the input canceled / button release
            if (holdToSprint == true)
            {
                sprintInput = false;
            }
        }

    }




    #endregion


    public void MovePlayerToSpawnpoint()
    {
        // find an object in the scene with the script "PlayerSpawnpoint"

        Transform targetSpawnpoint = FindFirstObjectByType<PlayerSpawnpoint>().transform;

        // this is very basic an only works if there is one spawnpoint in the scene, but works for now.

        // for a scene with multiple spawnpoints, we will need to find all the spawnpoints in the scene and store them in an array and then call the correct one based on ID.




        // Perform the actual move
        characterController.enabled = false;
        transform.position = targetSpawnpoint.position;
        transform.rotation = targetSpawnpoint.rotation;
        characterController.enabled = true;

        velocity = Vector3.zero;
        cameraRoot.localEulerAngles = Vector3.zero;

        if (debugLogsEnabled) Debug.Log($"Player repositioned to spawn point at {targetSpawnpoint.position}");
    }



    void OnEnable()
    {
        inputManager.MoveInputEvent += SetMoveInput;
        inputManager.LookInputEvent += SetLookInput;

        inputManager.JumpInputEvent += HandleJumpInput;
        inputManager.CrouchInputEvent += HandleCrouchInput;
        inputManager.SprintInputEvent += HandleSprintInput;

    }

    void OnDestroy()
    {
        inputManager.MoveInputEvent -= SetMoveInput;
        inputManager.LookInputEvent -= SetLookInput;

        inputManager.JumpInputEvent -= HandleJumpInput;
        inputManager.CrouchInputEvent += HandleCrouchInput;
        inputManager.SprintInputEvent += HandleSprintInput;


    }

}
