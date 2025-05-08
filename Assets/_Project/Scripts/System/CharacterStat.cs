public class CharacterStat
{
    public string characterName;
    public float maxHealth, currentHealth;
    public float maxStamina, currentStamina;
    public float baseAtk, currentAttack;
    public float jumpHoldForce, jumpHoldDuration;
    public float dashSpeed, dashDuration;
    public float moveSpeed, acceleration, deceleration;

     // Constructor to create a runtime copy from the ScriptableObject
    public CharacterStat(CharacterData config)
    {
        characterName = config.characterName;
        maxHealth = config.maxHealth;
        currentHealth = config.maxHealth;
        maxStamina = config.maxStamina;
        currentStamina = config.maxStamina;
        baseAtk = config.baseAtk;
        currentAttack = config.baseAtk;
        jumpHoldForce = config.jumpHoldForce;
        jumpHoldDuration = config.jumpHoldDuration;
        dashSpeed = config.dashSpeed;
        dashDuration = config.dashDuration;
        moveSpeed = config.moveSpeed;
        acceleration = config.acceleration;
        deceleration = config.deceleration;
    }
}
