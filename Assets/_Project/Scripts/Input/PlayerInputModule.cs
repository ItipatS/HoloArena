using UnityEngine;
using System.Collections.Generic;

public enum PlayerID { Player1, Player2 }

public class PlayerInputModule : MonoBehaviour, ICharacterModule
{
    [Header("Player Settings")]
    public PlayerID _playerID;
    public PlayerID playerID
    {
        get => _playerID;
        set
        {
            _playerID = value;
            keyMappings = (_playerID == PlayerID.Player1) ? keyMappingsPlayer1 : keyMappingsPlayer2;
        }
    }

    // Define separate key mappings for each player.
    private static readonly Dictionary<string, KeyCode> keyMappingsPlayer1 = new Dictionary<string, KeyCode>
    {
        { "Jump", KeyCode.J },
        { "Right", KeyCode.D },
        { "Left", KeyCode.A },
        { "Forward", KeyCode.W },
        { "Backward", KeyCode.S },
        { "LightAttack1", KeyCode.T },
        { "LightAttack2", KeyCode.Y },
        { "HeavyAttack1", KeyCode.G },
        { "HeavyAttack2", KeyCode.H },
        { "Block", KeyCode.U },
    };

    private static readonly Dictionary<string, KeyCode> keyMappingsPlayer2 = new Dictionary<string, KeyCode>
    {
        { "Jump", KeyCode.Keypad3 },
        { "Right", KeyCode.RightArrow },
        { "Left", KeyCode.LeftArrow },
        { "Forward", KeyCode.UpArrow },
        { "Backward", KeyCode.DownArrow },
        { "LightAttack1", KeyCode.Keypad4 },
        { "LightAttack2", KeyCode.Keypad5 },
        { "HeavyAttack1", KeyCode.Keypad1 },
        { "HeavyAttack2", KeyCode.Keypad2 },
        { "Block", KeyCode.Keypad6 },
    };

    private Dictionary<string, KeyCode> keyMappings; // The mapping used by this instance.
    private Dictionary<string, bool> keyStates = new Dictionary<string, bool>();

    private InputBufferModule inputBuffer;
    private InputWindowCheckerModule inputWindowChecker = new InputWindowCheckerModule();

    public void Initialize(FighterController controller)
    {
        // Choose key mapping based on playerID.
        keyMappings = (playerID == PlayerID.Player1) ? keyMappingsPlayer1 : keyMappingsPlayer2;

        // Get the shared input buffer from the controller.
        inputBuffer = controller.InputBuffer;

        // Initialize keyStates for each key.
        foreach (var key in keyMappings.Keys)
        {
            keyStates[key] = false;
        }
    }

    public void Tick()
    {
        inputBuffer.Tick();

        List<string> pressedThisFrame = new List<string>();

        //Detect new key presses.
        foreach (var key in keyMappings.Keys)
        {
            bool wasPressed = keyStates[key];
            bool isPressed = Input.GetKey(keyMappings[key]);
            keyStates[key] = isPressed;

            if (isPressed && !wasPressed)
            {
                pressedThisFrame.Add(key);
            }
        }

        // Second pass: add new key presses to buffer and window checker.
        foreach (string action in pressedThisFrame)
        {
            inputBuffer.AddInput(action);
            inputWindowChecker.RecordInput(action);
        }
    }

    public bool IsKeyPressed(string action)
    {
        return keyMappings.ContainsKey(action) && Input.GetKeyDown(keyMappings[action]);
    }

    public bool IsKeyHeld(string action)
    {
        return keyMappings.ContainsKey(action) && Input.GetKey(keyMappings[action]);
    }

    public bool IsKeyReleased(string action)
    {
        return keyMappings.ContainsKey(action) && Input.GetKeyUp(keyMappings[action]);
    }

    public bool ConsumeBufferedInput(string action) => inputBuffer.ConsumeInput(action);

    public bool CheckInputInWindow(string action, float timeWindow) => inputWindowChecker.CheckInputInWindow(action, timeWindow);

    public void FixedTick()
    {
        inputWindowChecker.Cleanup();
    }
}
