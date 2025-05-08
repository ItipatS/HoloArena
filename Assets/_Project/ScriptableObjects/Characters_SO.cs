using UnityEngine;

[CreateAssetMenu(fileName = "NewCharacterData", menuName = "Character/Character Data")]
public class CharacterData : ScriptableObject
{
    public GameObject characterPreview;
    public GameObject characterPrefab;
    public Sprite characterIcon;
    public bool isUnlocked;
    public string characterName;
    public float maxHealth = 100f;
    public float maxStamina = 100f;
    public float baseAtk = 10f;
    public float jumpHoldForce = 15f;
    public float jumpHoldDuration = 0.5f;
    public float dashSpeed = 10f;
    public float dashDuration = 0.3f;
    public float moveSpeed = 5f;
    public float acceleration = 10f;
    public float deceleration = 8f;
}
