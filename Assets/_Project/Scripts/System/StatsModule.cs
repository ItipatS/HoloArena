using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StatModule : MonoBehaviour, ICharacterModule
{
    [SerializeField] private CharacterData characterStatsConfig;
    public CharacterStat currentStats { get; private set; }
    public event Action<string, float> OnStatChanged;

    private Dictionary<string, Func<float>> statGetters;
    private Dictionary<string, Action<float, bool>> statModifiers;

    public event Action OnKO;

    public void Initialize(FighterController controller)
    {
        currentStats = new CharacterStat(characterStatsConfig); // Uses deep copy constructor

        InitializeStatDictionaries();
    }

    public void Tick(float deltaTime)
    {

    }

    public void FixedTick(float fixedDeltaTime)
    {
        if (currentStats.currentStamina < currentStats.maxStamina)
        {
            float regenAmount = 5f * Time.deltaTime;
            currentStats.currentStamina = Mathf.Min(currentStats.currentStamina + regenAmount, currentStats.maxStamina);
            OnStatChanged?.Invoke("stamina", currentStats.currentStamina);
        }
    }

    private void InitializeStatDictionaries()
    {
        statGetters = new Dictionary<string, Func<float>>(StringComparer.OrdinalIgnoreCase)
        {
            { "maxhealth",      () => currentStats.maxHealth },
            { "maxstamina",     () => currentStats.maxStamina },
            { "health",         () => currentStats.currentHealth },
            { "stamina",        () => currentStats.currentStamina },
            { "speed",          () => currentStats.moveSpeed },
            { "jumpforce",      () => currentStats.jumpHoldForce },
            { "jumpduration",   () => currentStats.jumpHoldDuration },
            { "dashspeed",      () => currentStats.dashSpeed },
            { "dashduration",   () => currentStats.dashDuration },
            { "acceleration",   () => currentStats.acceleration },
            { "deceleration",   () => currentStats.deceleration },
            { "baseattack",     () => currentStats.baseAtk },
            { "currentattack",  () => currentStats.currentAttack },
        };

        statModifiers = new Dictionary<string, Action<float, bool>>(StringComparer.OrdinalIgnoreCase)
        {
            { "maxhealth",    (value, relative) => ModifyMaxStat("maxhealth", ref currentStats.maxHealth, ref currentStats.currentHealth, value, relative) },
            { "maxstamina",   (value, relative) => ModifyMaxStat("maxstamina", ref currentStats.maxStamina, ref currentStats.currentStamina, value, relative) },
            { "baseattack",   (value, relative) => ModifyMaxStat("baseattack", ref currentStats.baseAtk, ref currentStats.currentAttack, value, relative) },
            { "health",       (value, relative) => ModifyStatDirect("health", ref currentStats.currentHealth, currentStats.maxHealth, value, relative) },
            { "stamina",      (value, relative) => ModifyStatDirect("stamina", ref currentStats.currentStamina, currentStats.maxStamina, value, relative) },
            { "speed",        (value, relative) => ModifyStatDirect("speed", ref currentStats.moveSpeed, float.MaxValue, value, relative) },
            { "jumpforce",    (value, relative) => ModifyStatDirect("jumpforce", ref currentStats.jumpHoldForce, float.MaxValue, value, relative) },
            { "jumpduration", (value, relative) => ModifyStatDirect("jumpduration", ref currentStats.jumpHoldDuration, float.MaxValue, value, relative) },
            { "dashspeed",    (value, relative) => ModifyStatDirect("dashspeed", ref currentStats.dashSpeed, float.MaxValue, value, relative) },
            { "dashduration", (value, relative) => ModifyStatDirect("dashduration", ref currentStats.dashDuration, float.MaxValue, value, relative) },
            { "acceleration", (value, relative) => ModifyStatDirect("acceleration", ref currentStats.acceleration, float.MaxValue, value, relative) },
            { "deceleration", (value, relative) => ModifyStatDirect("deceleration", ref currentStats.deceleration, float.MaxValue, value, relative) },
            { "currentattack",(value, relative) => ModifyStatDirect("currentattack", ref currentStats.currentAttack, currentStats.baseAtk, value, relative) },
        };
    }

    private void ModifyMaxStat(string statName, ref float maxStat, ref float currentStat, float value, bool relative)
    {
        float oldValue = maxStat;
        maxStat = relative ? oldValue + value : value;
        maxStat = Mathf.Max(maxStat, 0);

        currentStat = Mathf.Min(currentStat, maxStat);
        OnStatChanged?.Invoke(statName, maxStat);
    }

    private void ModifyStatDirect(string statName, ref float stat, float maxLimit, float value, bool relative)
    {
        float oldValue = stat;
        stat = relative ? oldValue + value : value;
        stat = Mathf.Clamp(stat, 0, maxLimit);
        OnStatChanged?.Invoke(statName, stat);
    }


    public void ModifyStat(string statName, float value, bool relative = true)
    {
        if (statModifiers.TryGetValue(statName, out var modify))
        {
            modify(value, relative);
        }
        else
        {
            Debug.LogWarning($"Stat '{statName}' not recognized in StatModule.");
        }
    }

    public void ResetStats()
    {
        Initialize(GetComponent<FighterController>()); // Re-initialization
        OnStatChanged?.Invoke("reset", 0f); // Notify listeners of full reset
    }

    public float GetStat(string statName)
    {
        return statGetters.TryGetValue(statName, out var getter) ? getter() : 0f;
    }

    public void ApplyTempBuff(string statName, float value, float duration)
    {
        ModifyStat(statName, value);
        StartCoroutine(RemoveBuff(statName, value, duration));
    }
    private IEnumerator RemoveBuff(string statName, float value, float duration)
    {
        yield return new WaitForSeconds(duration);
        ModifyStat(statName, -value);
    }

    //Temporary Stat Manipulation (TakeDamage, Consume Stamina)
    public void TakeDamage(float damage, Vector3 knockback)
    {
        currentStats.currentHealth = Mathf.Max(0, currentStats.currentHealth - damage);
        OnStatChanged?.Invoke("health", currentStats.currentHealth);

        // Optionally, apply knockback via the FighterController's Rigidbody.
        FighterController fighter = GetComponent<FighterController>();

        if (fighter != null)
        {
            Debug.LogWarning($"{damage} | Knockback: {knockback} | Target: {fighter.name}");
            fighter.Rigidbody.AddForce(knockback, ForceMode.Impulse);
        }
        if (currentStats.currentHealth <= 0f)
        {
            OnKO?.Invoke();
        }

    }

    public bool ConsumeStamina(float staminaNeed)
    {

        if (currentStats.currentStamina >= staminaNeed)
        {
            currentStats.currentStamina -= staminaNeed;
            OnStatChanged?.Invoke("stamina", currentStats.currentStamina);
            return true;
        }
        return false;
    }
}