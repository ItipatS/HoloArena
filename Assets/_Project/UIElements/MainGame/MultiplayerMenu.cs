using System.Collections.Generic;
using Fusion;
using Fusion.Sockets;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System;

public class MultiplayerMenu : MonoBehaviour, INetworkRunnerCallbacks
{
    public static MultiplayerMenu Instance { get; private set; }
    public CanvasGroup multiplayercanvas;
    public List<AnimatedUIElement> animatedElements;
    public SandwichTransition transition;
    public Button multiplayer;
    public Button local;
    public Button host;
    public Button join;
    public Button back;
    public TMP_InputField joinRoomInput;
    public TMP_InputField roomNameInput;

    public TextMeshProUGUI statusText;
    public NetworkRunner Runner;
    private NetworkRunner _runner;
    public CharacterSelectManager characterSelectManagerPrefab;
    void Awake()
    {
        // Make sure only one exists
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }
    void Start()
    {
        foreach (var el in animatedElements)
        {
            if (el.rect != null)
                el.originalAnchoredPosition = el.rect.anchoredPosition;
        }

        SetupButtons();
        HideWindow();
    }

    void SetupButtons()
    {
        if (multiplayer != null)
            ButtonAnimatorUtility.SetupButton(multiplayer, 10f, OpenMenu);
        if (local != null)
            ButtonAnimatorUtility.SetupButton(local, 10f, OnSelectCharacterPressed);
        if (host != null)
            ButtonAnimatorUtility.SetupButton(host, 10f, StartHost);
        if (join != null)
            ButtonAnimatorUtility.SetupButton(join, 10f, StartClient);
        if (back != null)
            ButtonAnimatorUtility.SetupButton(back, 10f, CloseMenu);
    }
    private void ShowWindow()
    {
        multiplayercanvas.alpha = 1;
        multiplayercanvas.interactable = true;
        multiplayercanvas.blocksRaycasts = true;

        foreach (var el in animatedElements)
        {
            if (el.rect == null) continue;

            LeanTween.cancel(el.rect.gameObject);

            el.rect.localScale = Vector3.one;
            el.rect.anchoredPosition = el.originalAnchoredPosition;

            UITween.SlideIn(el.rect, el.slideFrom, el.slideDistance, 2f, el.slideDelay);
        }
    }

    void HideWindow()
    {
        multiplayercanvas.alpha = 0;
        multiplayercanvas.interactable = false;
        multiplayercanvas.blocksRaycasts = false;
    }


    public void OpenMenu()
    {
        ShowWindow();
        multiplayer.gameObject.SetActive(false); // hide the pause button itself

    }

    public void CloseMenu()
    {
        HideWindow();
        multiplayer.gameObject.SetActive(true);
        LayoutRebuilder.ForceRebuildLayoutImmediate(multiplayercanvas.GetComponent<RectTransform>());

    }
    async void StartHost()
    {
        string roomName = roomNameInput.text == "" ? "MyRoom" : roomNameInput.text;
        await LaunchGame(Fusion.GameMode.Host, roomName);
    }

    async void StartClient()
    {
        string roomName = roomNameInput.text == "" ? "MyRoom" : roomNameInput.text;
        await LaunchGame(Fusion.GameMode.Client, roomName);
    }

    public void OnSelectCharacterPressed()
    {
        GameSessionManager.Instance.gameMode = GameMode.Local;
        ChangeScene("CharacterSelection");
    }

    async Task LaunchGame(Fusion.GameMode mode, string roomName)
    {
        statusText.text = "Connecting as " + mode + "...";

        _runner = gameObject.AddComponent<NetworkRunner>();
        Debug.Log(_runner);
        _runner.ProvideInput = true;

        var sceneManager = _runner.gameObject.AddComponent<NetworkSceneManagerDefault>();

        var result = await _runner.StartGame(new StartGameArgs()
        {
            GameMode = mode,
            SessionName = roomName,
            Scene = SceneRef.FromIndex(1),
            SceneManager = sceneManager,
            PlayerCount = 2
        });

        if (result.Ok)
            statusText.text = "Connected to room: " + roomName;
        else
            statusText.text = "Failed to connect: " + result.ShutdownReason;

        if (mode == Fusion.GameMode.Host || mode == Fusion.GameMode.Client)
            GameSessionManager.Instance.gameMode = GameMode.Multiplayer;

    }

    public void ChangeScene(string Scene)
    {
        LeanTween.moveX(multiplayercanvas.gameObject, -(Screen.width + Screen.width), 0.6f) // faster
    .setEaseInBack()
    .setUseEstimatedTime(true)
    .setOnComplete(() =>
    {
        transition.PlayTransition(() =>
        {
            SceneManager.LoadScene(Scene);
        });
    });
    }

    public void OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player)
    {
        throw new NotImplementedException();
    }

    public void OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player)
    {
        throw new NotImplementedException();
    }

    public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
    {
        Debug.LogError("What");
        var uiRefs = FindObjectOfType<CharacterSelectUIRefs>();
        var characterSelect = _runner.Spawn(characterSelectManagerPrefab, Vector3.zero, Quaternion.identity, player);
        characterSelect.GetComponent<CharacterSelectManager>().InitWithUI(uiRefs);
        GameSessionManager.Instance.characterSelectManager = characterSelect;
    }

    public void OnPlayerLeft(NetworkRunner runner, PlayerRef player)
    {
        throw new NotImplementedException();
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
        Debug.Log("✅ Connected to server!");
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
