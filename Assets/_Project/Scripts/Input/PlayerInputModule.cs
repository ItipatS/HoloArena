using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;
using System.Linq;

public enum PlayerID { Player1, Player2 }

public class PlayerInputModule : MonoBehaviour, ICharacterModule
{
    public PlayerInput playerInput; // Reference to the PlayerInput component.
    public InputActionAsset inputActions; // Reference to the Input Action Asset if needed.
    private InputActionMap playerActionMap;
    private Dictionary<string, InputAction> actionMap;
    private Vector2 moveInput;

    [Header("Player Settings")]
    public PlayerID playerID;
    private InputBufferModule inputBuffer;
    private InputWindowCheckerModule inputWindowChecker = new InputWindowCheckerModule();

    public void DetermineControlScheme()
    {
        var gamepads = Gamepad.all.ToList();

        switch (playerID)
        {
            case PlayerID.Player1:
                if (gamepads.Count >= 2)
                {
                    playerActionMap = inputActions.FindActionMap("Player1");
                    playerInput.SwitchCurrentControlScheme("Gamepad", gamepads[0]);
                }
                else
                {
                    playerActionMap = inputActions.FindActionMap("Player1");
                }
                break;

            case PlayerID.Player2:
                if (gamepads.Count >= 1)
                {
                    playerActionMap = inputActions.FindActionMap("Player1");
                    playerInput.SwitchCurrentControlScheme("Gamepad", gamepads[gamepads.Count - 1]);
                }
                else
                {
                    playerActionMap = inputActions.FindActionMap("Player2");
                }
                break;
        }
        actionMap = new Dictionary<string, InputAction>
        {
            { "Jump", playerActionMap.FindAction("Jump") },
            { "Move", playerActionMap.FindAction("Move") },
            { "LightAttack1", playerActionMap.FindAction("LightAttack1") },
            { "LightAttack2", playerActionMap.FindAction("LightAttack2") },
            { "HeavyAttack1", playerActionMap.FindAction("HeavyAttack1") },
            { "HeavyAttack2", playerActionMap.FindAction("HeavyAttack2") },
            { "Block", playerActionMap.FindAction("Block") }
        };

        if (actionMap.TryGetValue("Move", out InputAction moveAction))
        {
            moveAction.performed += OnMoveInput;
            moveAction.canceled += OnMoveInputCanceled;
        }

        foreach (var action in actionMap.Values.Where(a => a.name != "Move"))
        {
            action.started += OnInputActionTriggered;
            action.canceled += OnInputActionCanceled;
        }

        playerActionMap.Enable();
    }
    private void OnDisable()
    {
        if (actionMap != null)
        {
            // Unsubscribe from move events
            if (actionMap.TryGetValue("Move", out InputAction moveAction))
            {
                moveAction.performed -= OnMoveInput;
                moveAction.canceled -= OnMoveInputCanceled;
            }

            // Unsubscribe from all other actions
            foreach (var action in actionMap.Values)
            {
                action.started -= OnInputActionTriggered;
                action.canceled -= OnInputActionCanceled;
            }

            playerActionMap?.Disable();
        }
    }

    public void Initialize(FighterController controller)
    {
        playerInput = GetComponent<PlayerInput>();
        // Get the shared input buffer from the controller.
        inputBuffer = controller.InputBuffer;
    }
    private void OnMoveInput(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();

        // Always add to the buffer when the input crosses the threshold
        if (Mathf.Abs(moveInput.y) > 0.9f)
        {
            string direction = moveInput.y > 0 ? "Forward" : "Backward";
            inputBuffer.AddInput(direction);
        }
        if (Mathf.Abs(moveInput.x) > 0.9f)
        {
            string direction = moveInput.x > 0 ? "Right" : "Left";
            inputBuffer.AddInput(direction);
        }
    }

    private void OnInputActionTriggered(InputAction.CallbackContext context)
    {
        string actionName = context.action.name;
        inputBuffer.AddInput(actionName);
        inputWindowChecker.RecordInput(actionName);
    }

    private void OnMoveInputCanceled(InputAction.CallbackContext context)
    {
        moveInput = Vector2.zero;
    }

    private void OnInputActionCanceled(InputAction.CallbackContext context)
    {
        // Handle button release if needed
        string actionName = context.action.name;
        // You might want to record releases differently in your input buffer
    }

    public bool IsKeyPressed(string action)
    {
        if (actionMap.TryGetValue(action, out InputAction inputAction))
        {
            return inputAction.triggered;
        }
        return false;
    }

    public bool IsKeyHeld(string action)
    {
        switch (action)
        {
        case "Forward":
            return moveInput.y > 0.5f; // Check if the player is holding "Forward"
        case "Backward":
            return moveInput.y < -0.5f; // Check if the player is holding "Backward"
        case "Left":
            return moveInput.x < -0.5f; // Check if the player is holding "Left"
        case "Right":
            return moveInput.x > 0.5f; // Check if the player is holding "Right"
        default:
            if (actionMap.TryGetValue(action, out InputAction inputAction))
            {
                return inputAction.IsPressed(); // For non-movement actions
            }
            return false;
        }
    }

    public bool IsKeyReleased(string action)
    {
        if (actionMap.TryGetValue(action, out InputAction inputAction))
        {
            return inputAction.WasReleasedThisFrame();
        }
        return false;
    }

    public bool ConsumeBufferedInput(string action) => inputBuffer.ConsumeInput(action);

    public bool CheckInputInWindow(string action, float timeWindow) => inputWindowChecker.CheckInputInWindow(action, timeWindow);

    public void FixedTick(float fixedDeltaTime)
    {
        inputBuffer.Tick();
        inputWindowChecker.Cleanup();
    }
    
    private void OnDestroy()
    {
        OnDisable(); // Clean up subscriptions
    }

    public void Tick(float deltaTime) { }
}
