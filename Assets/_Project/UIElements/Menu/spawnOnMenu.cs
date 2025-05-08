using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class spawnOnMenu : MonoBehaviour
{
    public Transform player1Spawn;
    public Transform player2Spawn;
    public GameObject MainMenuprefab1;
    public GameObject MainMenuprefab2;
    public CameraController cameraController;
    private GameObject player1;
    private GameObject player2;
    private WireFrameEffectController wireframeEffect1;
    private WireFrameEffectController wireframeEffect2;
    // Start is called before the first frame update
    void Start()
    {
        player1 = Instantiate(MainMenuprefab1, player1Spawn.position, Quaternion.identity);
        player2 = Instantiate(MainMenuprefab2, player2Spawn.position, Quaternion.identity);

        FighterController controller1 = player1.GetComponent<FighterController>();
        FighterController controller2 = player2.GetComponent<FighterController>();

        // Assign Player IDs
        controller1.GetModule<PlayerInputModule>().playerID = PlayerID.Player1;
        controller2.GetModule<PlayerInputModule>().playerID = PlayerID.Player2;

        controller1.GetModule<MovementModule>().SetOpponent(player2.transform);
        controller2.GetModule<MovementModule>().SetOpponent(player1.transform);

        wireframeEffect1 = player1.GetComponent<WireFrameEffectController>();
        wireframeEffect2 = player2.GetComponent<WireFrameEffectController>();

        if (wireframeEffect1 != null)
            wireframeEffect1.PlaySpawnAnimation();
        if (wireframeEffect2 != null)
            wireframeEffect2.PlaySpawnAnimation();

        cameraController.SetTargets(player1.transform, player2.transform);
    }
}
