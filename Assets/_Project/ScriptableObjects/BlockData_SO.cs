using UnityEngine;

[CreateAssetMenu(menuName = "Attack/Block Data")]
public class BlockData_SO : ScriptableObject
{
    [Header("Timing")]
    public int startupFrames;       // Time before block is fully active
    public int activeFrames;        // Duration of effective block
    public int recoveryFrames;      // Time after block before you can act again

    [Header("Block Quality")]
    // If the block key is released within this threshold during active frames, it is considered a perfect block.
    public float perfectBlockThreshold; 
    public float staminaPerSecond;
    public float perfectStaminaRefund;
    [Header("VFX Settings")]
    public Color normalColor = new Color(1f, 1f, 1f, 0.5f);
    public Color normalBlockColor = new Color(1f, 1f, 1f, 0.5f);
    public Color perfectBlockColor = new Color(0f, 1f, 1f, 0.7f);
}
