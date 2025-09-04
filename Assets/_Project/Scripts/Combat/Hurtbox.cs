using System.Collections;
using UnityEngine;

public class Hurtbox : MonoBehaviour, ICharacterModule
{
    public GameObject Owner { get; set; }
    private BlockData_SO blockData;
    private BlockVFXController blockVFX;
    public Color normalColor = Color.green;
    // Color when hit.
    public Color hitColor = Color.yellow;
    // Current color (for debugging via Gizmos).
    private Color currentColor;
    // Reference to the fighter's controller to apply stun/damage, if needed.
    private MovementModule movementModule;
    private Coroutine hitCoroutine;
    private FighterBlockModule blockModule;
    private FighterAttackModule attackModule;
    private StatModule Stats;

    public void Initialize(FighterController controller)
    {
        currentColor = normalColor;
        blockVFX = controller.GetModule<BlockVFXController>();
        movementModule = controller.GetModule<MovementModule>();
        Stats = controller.GetModule<StatModule>();
        blockModule = controller.GetModule<FighterBlockModule>();
        attackModule = controller.GetModule<FighterAttackModule>();

        if (blockModule != null)
        {
            blockData = blockModule.blockData;
        }
        else
        {
            Debug.LogWarning("No BlockModule found in parent!");
        }
    }
    protected bool IsBlocking => blockModule != null && blockModule.IsBlocking;

    private void OnDrawGizmos()
    {
        Gizmos.color = currentColor;

        // Try to get a CapsuleCollider first.
        CapsuleCollider capsule = GetComponent<CapsuleCollider>();
        if (capsule != null)
        {
            // Calculate the top and bottom sphere positions.
            Vector3 center = transform.position + capsule.center;
            float height = Mathf.Max(capsule.height, capsule.radius * 2);
            float offset = (height - capsule.radius * 2) / 2f;
            Vector3 up = Vector3.up;
            // Depending on capsule.direction, you might adjust 'up' accordingly:
            if (capsule.direction == 0) up = transform.right;
            else if (capsule.direction == 2) up = transform.forward;

            Vector3 topSphere = center + up * offset;
            Vector3 bottomSphere = center - up * offset;

            // Draw spheres at the ends.
            Gizmos.DrawWireSphere(topSphere, capsule.radius);
            Gizmos.DrawWireSphere(bottomSphere, capsule.radius);
            // Optionally, draw a line connecting the spheres.
            Gizmos.DrawLine(topSphere + Vector3.forward * capsule.radius, bottomSphere + Vector3.forward * capsule.radius);
            Gizmos.DrawLine(topSphere - Vector3.forward * capsule.radius, bottomSphere - Vector3.forward * capsule.radius);
            Gizmos.DrawLine(topSphere + Vector3.right * capsule.radius, bottomSphere + Vector3.right * capsule.radius);
            Gizmos.DrawLine(topSphere - Vector3.right * capsule.radius, bottomSphere - Vector3.right * capsule.radius);
        }
        else
        {
            // If there's a BoxCollider, draw its wire cube.
            BoxCollider bc = GetComponent<BoxCollider>();
            if (bc != null)
            {
                Gizmos.DrawWireCube(transform.position + bc.center, bc.size);
            }
            else
            {
                // Fallback: draw a default cube.
                Gizmos.DrawWireCube(transform.position, Vector3.one);
            }
        }
    }
    public void OnHit(int hitStop, float damage, Vector3 knockback)
    {
        Debug.Log($"{Owner.name} got hit for {damage} damage!");
        // Start visual hit effect.
        if (hitCoroutine != null)
            StopCoroutine(hitCoroutine);
        hitCoroutine = StartCoroutine(HitEffectDebugCoroutine(hitStop));

        bool isPerfectBlock = IsBlocking && blockModule.IsPerfectBlock();

        // Use the DamageHandler to adjust values.
        CalculateDamage(ref damage, ref knockback, ref hitStop, IsBlocking, isPerfectBlock);

        // Now apply damage using your StatModule.
        Stats.TakeDamage(damage, knockback);

        // Optionally lock movement to simulate hit stun.
        if (movementModule != null)
        {
            movementModule.LockMovement(true, hitStop);
        }
        // Fire the global hit event (using the parent character).
        HitEventManager.CharacterHit(transform.parent.gameObject, hitStop);
    }


    private IEnumerator HitEffectDebugCoroutine(float duration)
    {
        // Set the color to yellow.
        currentColor = hitColor;
        yield return new WaitForSeconds(duration);
        // After hitStop duration, revert back to normal color.
        currentColor = normalColor;
        blockVFX.blockMaterial.SetColor("_TintColor", blockData.normalColor);
    }

    private void CalculateDamage(ref float damage, ref Vector3 knockback, ref int hitStop,
                                       bool isBlocking, bool isPerfectBlock)
    {
        if (isBlocking)
        {
            if (isPerfectBlock)
            {
                blockVFX.blockMaterial.SetColor("_TintColor", blockData.perfectBlockColor);
                AudioManager.Instance.PlaySound("PerfectBlock");
                damage = 0f;
                knockback = Vector3.zero;
                hitStop = 0;
                Stats.ModifyStat("stamina", Stats.currentStats.currentStamina + blockData.perfectStaminaRefund , false);
            }
            else
            {
                blockVFX.blockMaterial.SetColor("_TintColor", blockData.normalBlockColor);
                AudioManager.Instance.PlaySound("Block");
                damage *= 0.5f;
                knockback *= 0.5f;
                if(hitStop > 2)
                {
                    hitStop = 1;
                }else
                {
                    hitStop = 0;
                }
            }
        }
        AudioManager.Instance.PlaySound("Hit");
    }

    public void Tick(float deltaTime)
    {
    }

    public void FixedTick(float fixedDeltaTime)
    {
    }
}
