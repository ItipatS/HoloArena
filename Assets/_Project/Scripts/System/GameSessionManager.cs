using UnityEngine;

public enum GameMode { None, SinglePlayer,Local, Multiplayer, Client, Host }

public class GameSessionManager : MonoBehaviour
{
    public static GameSessionManager Instance;
    public CharacterSelectManager characterSelectManager;
    public GameMode gameMode = GameMode.None;
    public CharacterData selectedCharacterP1;
    public CharacterData selectedCharacterP2;

    void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void SetGameMode(GameMode mode)
    {
        gameMode = mode;
    }

    public void SetCharacter(int playerIndex, CharacterData data)
    {
        if (playerIndex == 1) selectedCharacterP1 = data;
        if (playerIndex == 2) selectedCharacterP2 = data;
    }
}
