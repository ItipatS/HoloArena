using System;
using UnityEngine;

public class WireFrameEffectController : MonoBehaviour
{
    private Material mat;

    void Awake()
    {
        Renderer[] renderers = GetComponentsInChildren<Renderer>();
        // For example, if the target renderer's GameObject is named "BodyMesh"
        foreach (Renderer rend in renderers)
        {
            if (rend.gameObject.name.Equals("body", StringComparison.OrdinalIgnoreCase))
            {
                mat = rend.material;
                break;
            }
        }
        if (mat == null)
        {
            Debug.LogError("Could not find target renderer by name!");
        }
    }


    // Called when the character spawns or respawns:
    public void PlaySpawnAnimation(float duration = 2f)
    {
        // Tween _MovingSlider from -1 to 5 over "duration" seconds.
        LeanTween.value(gameObject, -1f, 5f, duration)
            .setEase(LeanTweenType.easeInOutSine)
            .setOnUpdate((float val) =>
            {
                mat.SetFloat("_MovingSlider", val);
            });
    }

    // Called when HP drops below a threshold, for a ping-pong pulse effect.
    public void PlayLowHPPulse(float duration = 1f)
    {
        // Tween between two values near 5 for a subtle pulse effect.
        LeanTween.value(gameObject, 4.5f, 5f, duration)
            .setEaseInOutSine()
            .setLoopPingPong()
            .setOnUpdate((float val) =>
            {
                mat.SetFloat("_MovingSlider", val);
            });
    }

    // Called when the character dies:
    public void PlayDeathAnimation(float duration = 1f)
    {
        // Tween _MovingSlider from its current value to -1 (so the wireframe fades out).
        float currentVal = mat.GetFloat("_MovingSlider");
        LeanTween.value(gameObject, currentVal, -1f, duration)
            .setEase(LeanTweenType.easeInOutSine)
            .setOnUpdate((float val) =>
            {
                mat.SetFloat("_MovingSlider", val);
            });
    }
}
