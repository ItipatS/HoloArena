using UnityEngine;

public class RoundManager : MonoBehaviour
{
    public static RoundManager Instance { get; private set; }

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }
    public TMPro.TextMeshProUGUI p1WinText;
    public TMPro.TextMeshProUGUI p2WinText;
    public int maxRounds = 3;
    private int p1Wins = 0;
    private int p2Wins = 0;

    public SandwichTransition transition;


    public void RegisterWin(PlayerID winner)
    {
        if (winner == PlayerID.Player1)
            p1Wins++;
        else
            p2Wins++;
        UpdateWinUI();

        Debug.Log($"P1: {p1Wins} | P2: {p2Wins}");

        if (p1Wins >= 3)
        {
            EndMatch(PlayerID.Player1);
        }
        else if (p2Wins >= 3)
        {
            EndMatch(PlayerID.Player2);
        }
        else
        {
            transition.PlayTransition(() => {StartCoroutine(MatchManager.Instance.StartNextRound());});
        }
        // Reset for next round
    }


    private void EndMatch(PlayerID winner)
    {
        Debug.Log($"Player {winner} wins the match!");
        GameSessionManager.Instance.selectedCharacterP1 = null;
        GameSessionManager.Instance.selectedCharacterP2 = null;
        transition.PlayTransition(() => {
            winMenu.Instance.ShowWin(winner.ToString());
        });
        
    }

    void UpdateWinUI()
    {
        p1WinText.text = $"Wins: {p1Wins}";
        p2WinText.text = $"Wins: {p2Wins}";
    }
}
