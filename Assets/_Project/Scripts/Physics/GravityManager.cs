using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GravityManager : MonoBehaviour
{
    public static GravityManager Instance { get; private set; }

    [Header("Global Gravity Settings")]
    public float defaultGravity = -9.81f;
    private Dictionary<Rigidbody, float> gravityOverrides = new Dictionary<Rigidbody, float>();

    private float cleanupTimer;
    private float cleanupInterval = 1f;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public float GetGravity(Rigidbody rb)
    {
        if (rb == null) return defaultGravity;
        return gravityOverrides.TryGetValue(rb, out float customGravity) ? customGravity : defaultGravity;
    }

    public void SetGravityOverride(Rigidbody rb, float newGravity)
    {
        if (rb != null) gravityOverrides[rb] = newGravity;
    }

    public void ClearGravityOverride(Rigidbody rb)
    {
        gravityOverrides.Remove(rb);
    }
    private void Update()
    {
        cleanupTimer += Time.deltaTime;
        if (cleanupTimer >= cleanupInterval)
        {
            CleanupDestroyedRigidbodies();
            cleanupTimer = 0f;
        }
    }

    private void CleanupDestroyedRigidbodies()
    {
        foreach (var rigidbody in gravityOverrides.Keys.ToList())
        {
            if (rigidbody == null)
                gravityOverrides.Remove(rigidbody);
        }
    }
}
