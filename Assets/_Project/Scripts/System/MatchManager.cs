using System;
using System.Collections;
using UnityEngine;

public class MatchManager : MonoBehaviour
{
    public static MatchManager Instance { get; private set; }
    public RoundManager roundManager; // Assign in Inspector
    public float roundResetDelay = 2f;
    public Transform player1Spawn;
    public Transform player2Spawn;

    public GameObject MainMenuprefab1;
    public GameObject MainMenuprefab2;
    public CameraController cameraController;

    private GameObject player1;
    private GameObject player2;

    public UIStatImageController p1HealthUI;
    public UIStatImageController p2HealthUI;
    public UIStatImageController p1StaminaUI;
    public UIStatImageController p2StaminaUI;
    public WireFrameEffectController wireframeEffect1 { get; private set; }
    public WireFrameEffectController wireframeEffect2 { get; private set; }
    private float roundTime = 99f;
    private float currentTime;
    public TMPro.TextMeshProUGUI timerText;
    private bool roundActive = false;
    public SandwichTransition transition;
    private StatModule p1Stat;
    private StatModule p2Stat;

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
        SpawnPlayers();
        SetupCamera();
        currentTime = roundTime;
        roundActive = true;
    }

    void Update()
    {
        if (!roundActive) return;

        currentTime -= Time.deltaTime;
        timerText.text = Mathf.CeilToInt(currentTime).ToString();

        if (currentTime <= 0f)
        {
            roundActive = false;
            HandleTimeout();
        }
    }

    void HandleTimeout()
    {
        // Determine who has more HP, or declare draw
        var p1HP = player1.GetComponent<FighterController>().GetModule<StatModule>().currentStats.currentHealth;
        var p2HP = player2.GetComponent<FighterController>().GetModule<StatModule>().currentStats.currentHealth;

        if (p1HP > p2HP)
            StartCoroutine(HandleTimeOut(PlayerID.Player1));
        if (Mathf.Approximately(p1HP, p2HP))
        {
            Debug.Log("Draw!");
            // Optionally call StartNextRound with delay
            StartCoroutine(StartNextRound());
            return;
        }
        else if (p2HP > p1HP)
            StartCoroutine(HandleTimeOut(PlayerID.Player2));
    }


    void SpawnPlayers()
    {
        GameObject prefab1 = GameSessionManager.Instance.selectedCharacterP1.characterPrefab;
        GameObject prefab2 = GameSessionManager.Instance.selectedCharacterP2.characterPrefab;

        player1 = Instantiate(prefab1, player1Spawn.position, Quaternion.identity);
        player2 = Instantiate(prefab2, player2Spawn.position, Quaternion.identity);

        FighterController controller1 = player1.GetComponent<FighterController>();
        FighterController controller2 = player2.GetComponent<FighterController>();

        // Assign Player IDs
        controller1.GetModule<PlayerInputModule>().playerID = PlayerID.Player1;
        controller2.GetModule<PlayerInputModule>().playerID = PlayerID.Player2;

        controller1.GetModule<MovementModule>().SetOpponent(player2.transform);
        controller2.GetModule<MovementModule>().SetOpponent(player1.transform);

        p1Stat = player1.GetComponent<FighterController>().GetModule<StatModule>();
        p2Stat = player2.GetComponent<FighterController>().GetModule<StatModule>();

        p1Stat.ResetStats();
        p2Stat.ResetStats();

        p1Stat.OnKO += () => StartCoroutine(HandleKO(PlayerID.Player2));
        p2Stat.OnKO += () => StartCoroutine(HandleKO(PlayerID.Player1));

        p1HealthUI.SetStatModule(p1Stat);
        p2HealthUI.SetStatModule(p2Stat);
        p1StaminaUI.SetStatModule(p1Stat);
        p2StaminaUI.SetStatModule(p2Stat);
        p1HealthUI.ResetEffects();
        p2HealthUI.ResetEffects();
        p1StaminaUI.ResetEffects();
        p2StaminaUI.ResetEffects();


        wireframeEffect1 = player1.GetComponent<WireFrameEffectController>();
        wireframeEffect2 = player2.GetComponent<WireFrameEffectController>();

        wireframeEffect1.PlaySpawnAnimation();
        wireframeEffect2.PlaySpawnAnimation();
    }

    private IEnumerator HandleKO(PlayerID winner)
    {
        p1HealthUI.ResetEffects();
        p2HealthUI.ResetEffects();
        if (winner == PlayerID.Player1)
        {
            wireframeEffect2.PlayDeathAnimation();
        }
        else
        {
            wireframeEffect1.PlayDeathAnimation();
        }
        Time.timeScale = 0.3f;
        ScreenFlash.Instance?.Flash();

        yield return new WaitForSecondsRealtime(1.0f); // Add suspense before KO shows
        KOUIManager.Instance?.ShowKO(() =>
        {
            Time.timeScale = 1f;
            roundManager.RegisterWin(winner);
        });
    }

    private IEnumerator HandleTimeOut(PlayerID winner)
    {
        yield return new WaitForSecondsRealtime(1.0f);
        
        p1HealthUI.ResetEffects();
        p2HealthUI.ResetEffects();
        roundManager.RegisterWin(winner);
    }
    void SetupCamera()
    {
        cameraController.SetTargets(player1.transform, player2.transform);
    }

    public IEnumerator StartNextRound()
    {
        roundActive = false;
        yield return new WaitForSeconds(0.5f);

        player1.GetComponent<FighterController>().GetModule<MovementModule>().LockMovement(true ,60);
        player2.GetComponent<FighterController>().GetModule<MovementModule>().LockMovement(true, 60);

        // Reset positions
        player1.transform.position = player1Spawn.position;
        player2.transform.position = player2Spawn.position;

        player1.transform.rotation = Quaternion.identity;
        player2.transform.rotation = Quaternion.identity;

        p1Stat.ResetStats();
        p2Stat.ResetStats();
        p1HealthUI.ResetEffects();
        p2HealthUI.ResetEffects();
        p1StaminaUI.ResetEffects();
        p2StaminaUI.ResetEffects();

        wireframeEffect1.PlaySpawnAnimation();
        wireframeEffect2.PlaySpawnAnimation();
        
        transition.PlayReverse();
        yield return new WaitForSeconds(0.6f);

        currentTime = roundTime;
        roundActive = true;

        Debug.Log("Next round started!");
    }
}
