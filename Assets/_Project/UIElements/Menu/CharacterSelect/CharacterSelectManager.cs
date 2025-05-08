using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Fusion;
using Fusion.Sockets;

public enum CharacterSelectState
{
    Player1,
    Player2,
    Done
}

public class CharacterSelectManager : NetworkBehaviour, INetworkRunnerCallbacks
{
    [Networked] public PlayerRef HostRef { get; set; }
    [Networked] public PlayerRef JoinerRef { get; set; }

    [Networked] public int HostCharacterIndex { get; set; }
    [Networked] public int JoinerCharacterIndex { get; set; }

    [Networked] public bool HostConfirmed { get; set; }
    [Networked] public bool JoinerConfirmed { get; set; }

    [Networked, Capacity(2)]
    public NetworkDictionary<PlayerRef, int> CharacterSelections { get; }
    private int _localHostCharIndex = -1;
    private int _localJoinerCharIndex = -1;
    public GameObject characterButtonPrefab;
    public CharacterData[] allCharacters;
    public CharacterSelectState selectState = CharacterSelectState.Player1;
    public CharacterPreview leftPreview;
    public CharacterPreview rightPreview;
    private CharacterData selectedCharacter;
    public SandwichTransition transition;
    private List<Button> characterButtons = new();

    private CharacterSelectUIRefs _ui;

    public void InitWithUI(CharacterSelectUIRefs uiRefs)
    {
        _ui = uiRefs;
    }

    /// <summary>
    /// Awake is called when the script instance is being loaded.
    /// </summary>
    void Awake()
    {
        if (GameSessionManager.Instance.gameMode == GameMode.Multiplayer && (!Object || !Object.HasStateAuthority))
        {
            Destroy(gameObject);
            return;
        }
    }

    // This method is called when Player 1 is selected.
    void Start()
    {
        if (GameSessionManager.Instance.gameMode == GameMode.Local)
        {
            _ui = FindObjectOfType<CharacterSelectUIRefs>();
            Debug.Log(_ui);
            GameSessionManager.Instance.characterSelectManager = this;
        }

        PopulateCharacterButtons();

        if (Runner == null) // Local mode
        {
            _ui.confirmButton.gameObject.SetActive(true);
            _ui.confirmButton1.gameObject.SetActive(false);
            _ui.confirmButton2.gameObject.SetActive(false);
            ButtonAnimatorUtility.SetupButton(_ui.confirmButton, 10f, ConfirmCharacter);
        }
        else // Multiplayer
        {
            _ui.confirmButton.gameObject.SetActive(false);
            _ui.confirmButton1.gameObject.SetActive(true);
            _ui.confirmButton2.gameObject.SetActive(true);

            ButtonAnimatorUtility.SetupButton(_ui.confirmButton1, 10f, ConfirmCharacter);
            ButtonAnimatorUtility.SetupButton(_ui.confirmButton2, 10f, ConfirmCharacter);
        }

        ButtonAnimatorUtility.SetupButton(_ui.exitButton, 10f, Exit);
    }

    private void Exit()
    {
        _ui.exitButton.interactable = false;
        ExitToMainMenu();
    }

    void PopulateCharacterButtons()
    {
        foreach (var character in allCharacters)
        {
            var selectedCharacter = character;

            GameObject btnObj = Instantiate(characterButtonPrefab, _ui.buttonGrid);
            RectTransform btnRect = btnObj.GetComponent<RectTransform>();
            var ui = btnObj.GetComponent<CharacterButtonUI>();

            ui.Init(selectedCharacter, this);

            Button button = btnObj.GetComponent<Button>();
            characterButtons.Add(button);

            ButtonAnimatorUtility.SetupButton(button, 10f, () =>
            {
                SelectCharacter(selectedCharacter); // hook up click action
            });
            UITween.ScalePulse(btnObj, 1.05f, 1f);
        }
    }

    public void SelectCharacter(CharacterData data)
    {
        int index = Array.IndexOf(allCharacters, data);
        if (index < 0) return;

        if (Runner == null)
        {
            if (_localHostCharIndex == -1)
            {
                _localHostCharIndex = index;
                leftPreview.Display(data.characterPreview);
                selectState = CharacterSelectState.Player2;
                GameSessionManager.Instance.selectedCharacterP1 = allCharacters[_localHostCharIndex];

            }
            else
            {
                _localJoinerCharIndex = index;
                rightPreview.Display(data.characterPreview);
                GameSessionManager.Instance.selectedCharacterP2 = allCharacters[_localJoinerCharIndex];
            }
        }
        else // multiplayer
        {
            CharacterSelections.Set(Runner.LocalPlayer, index);

            if (Runner.LocalPlayer == HostRef)
            {
                leftPreview.Display(data.characterPreview);

            }
            else if (Runner.LocalPlayer == JoinerRef)
            {
                rightPreview.Display(data.characterPreview);
            }
        }

    }

    public void PreviewCharacter(CharacterData data)
    {
        if (selectState == CharacterSelectState.Player1)
        {
            leftPreview.Display(data.characterPreview);
        }
        else if (selectState == CharacterSelectState.Player2)
        {
            rightPreview.Display(data.characterPreview);
        }
    }

    public void ConfirmCharacter()
    {
        if (Runner == null)
        {
            if (_localHostCharIndex != -1 && _localJoinerCharIndex != -1)
            {
                ChangeScene("MainGame");
            }
            return;
        }

        if (Runner.LocalPlayer == HostRef)
            HostConfirmed = true;
        else if (Runner.LocalPlayer == JoinerRef)
            JoinerConfirmed = true;

        if (HostConfirmed && JoinerConfirmed && Object.HasStateAuthority)
        {
            Debug.Log("Both confirmed. Changing scene...");
            ChangeScene("MainGame");
        }
    }

    public void ChangeScene(string Scene)
    {

        foreach (var btn in characterButtons)
            btn.interactable = false;

        LeanTween.moveX(_ui.MainPanel, +(Screen.width + Screen.width), 0.6f) // faster
    .setEaseInBack()
    .setOnComplete(() =>
    {
        transition.PlayTransition(() =>
        {
            SceneManager.LoadScene(Scene);
        });
    });
    }

    public CharacterData GetCurrentSelectedCharacter()
    {
        return selectState == CharacterSelectState.Player1
            ? GameSessionManager.Instance.selectedCharacterP1
            : GameSessionManager.Instance.selectedCharacterP2;
    }

    void Update()
    {
        if (Runner == null)
        {
            _ui.confirmButton.interactable = _localHostCharIndex != -1 && _localJoinerCharIndex != -1;
        }
        else
        {
            if (Runner.LocalPlayer == HostRef)
                _ui.confirmButton1.interactable = HostCharacterIndex != -1;
            else if (Runner.LocalPlayer == JoinerRef)
                _ui.confirmButton2.interactable = JoinerCharacterIndex != -1;
        }


        if (Input.GetMouseButtonDown(1)) // right-click
        {
            if (Runner == null)
            {
                if (selectState == CharacterSelectState.Player2)
                {
                    _localHostCharIndex = -1;
                    rightPreview.Clear();
                    selectState = CharacterSelectState.Player1;
                }
                else
                {
                    _localJoinerCharIndex = -1;
                    leftPreview.Clear();
                }
            }
            else
            {
                if (Runner.LocalPlayer == HostRef)
                {
                    HostCharacterIndex = -1;
                    leftPreview.Clear();
                }
                else if (Runner.LocalPlayer == JoinerRef)
                {
                    JoinerCharacterIndex = -1;
                    rightPreview.Clear();
                }
            }


            UISoundManager.Instance?.PlayCancel();
        }
    }

    public void ExitToMainMenu()
    {
        GameSessionManager.Instance.SetGameMode(GameMode.None);
        SceneManager.LoadScene("MainMenu");
    }

    public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
    {
        if (!Object.HasStateAuthority) return; // Only the host sets these

        Debug.Log($"✅ Player {player} joined. Current Host: {HostRef}, Joiner: {JoinerRef}");

        if (HostRef == default)
        {
            HostRef = player;
            Debug.Log("Assigned HostRef to " + player);
        }
        else if (JoinerRef == default)
        {
            JoinerRef = player;
            Debug.Log("Assigned JoinerRef to " + player);
        }
    }


    public void OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player)
    {

    }

    public void OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player)
    {
    }



    public void OnPlayerLeft(NetworkRunner runner, PlayerRef player)
    {
        if (!Object.HasStateAuthority) return;

        Debug.Log($"❌ Player {player} left");

        CharacterSelections.Remove(player);

        if (player == HostRef)
            HostRef = default;
        else if (player == JoinerRef)
            JoinerRef = default;
    }


    public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason)
    {
    }

    public void OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason reason)
    {
    }

    public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token)
    {
    }

    public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason)
    {
    }

    public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message)
    {
    }

    public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ReliableKey key, ArraySegment<byte> data)
    {
    }

    public void OnReliableDataProgress(NetworkRunner runner, PlayerRef player, ReliableKey key, float progress)
    {
    }

    public void OnInput(NetworkRunner runner, NetworkInput input)
    {
    }

    public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input)
    {

    }

    public void OnConnectedToServer(NetworkRunner runner)
    {
    }

    public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList)
    {
    }

    public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data)
    {
    }

    public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken)
    {
    }

    public void OnSceneLoadDone(NetworkRunner runner)
    {
    }

    public void OnSceneLoadStart(NetworkRunner runner)
    {
    }
}
