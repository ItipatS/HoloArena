using UnityEngine;
using System.Collections;

public class Hitbox : MonoBehaviour, ICharacterModule
{
    public GameObject Owner { get; set; }
    public float damage;
    public Vector3 knockback;
    public float activeDuration; // How long the hitbox stays active
    public Color debugColor = Color.red; // Red for hitbox
    private bool isActive = true;
    public int hitStop; // This will be set by the FighterAttackModule.

    public void Initialize(FighterController controller)
    {}

    private void OnEnable()
    {
        // When enabled, automatically disable after a short duration.
        StartCoroutine(DisableAfterTime(activeDuration));
    }

    private IEnumerator DisableAfterTime(float time)
    {
        yield return new WaitForSeconds(time);
        gameObject.SetActive(false);
    }

    private void OnTriggerEnter(Collider other)
    {
        // Look for a Hurtbox on the collided object.
        Hurtbox hurtbox = other.GetComponent<Hurtbox>();    
        if (hurtbox != null && hurtbox.Owner != this.Owner)
        {
            hurtbox.OnHit(hitStop, damage, knockback);
        }
    }

    void OnDrawGizmos()
    {
        BoxCollider bc = GetComponent<BoxCollider>();
        if (!isActive)
            return; // Only draw if active

        if (bc != null)
        {
            Gizmos.color = debugColor;

            // Save the current Gizmos matrix
            Matrix4x4 oldMatrix = Gizmos.matrix;

            // Multiply our transform's matrix so the Gizmos are drawn in local space
            Gizmos.matrix = transform.localToWorldMatrix;

            // Now draw the cube at the BoxCollider's center, using local coordinates
            Gizmos.DrawWireCube(bc.center, bc.size);

            // Restore the old matrix
            Gizmos.matrix = oldMatrix;
        }
        else
        {
            // Otherwise, draw a default cube
            Gizmos.color = debugColor;
            Gizmos.DrawWireCube(transform.position, Vector3.one);
        }
    }

    public void Tick(float deltaTime)
    {
        throw new System.NotImplementedException();
    }

    public void FixedTick(float fixedDeltaTime)
    {
        throw new System.NotImplementedException();
    }
}
