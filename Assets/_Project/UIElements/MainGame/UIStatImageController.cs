using System;
using UnityEngine;
using UnityEngine.UI;

public class UIStatImageController : MonoBehaviour
{
    public enum StatType { Health, Stamina }
    public StatType statType;

    private Image uiImage;
    private Material materialInstance;
    private StatModule statModule;

    private bool lowHPTriggered = false;
    private AudioSource heartbeatSource;
    public Vector3 original;

    [SerializeField] private string shaderPropertyName = "_FillAmount";

    private void Awake()
    {
        uiImage = GetComponent<Image>();

        materialInstance = uiImage.material;
        original = uiImage.rectTransform.localScale;
    }

    private void OnEnable()
    {
        if (statModule != null)
            statModule.OnStatChanged += UpdateUI;
    }

    private void OnDisable()
    {
        if (statModule != null)
            statModule.OnStatChanged -= UpdateUI;
    }

    public void SetStatModule(StatModule module)
    {
       
        if (statModule != null)
            statModule.OnStatChanged -= UpdateUI; 
            
        statModule = module;

        if (statModule != null)
            statModule.OnStatChanged += UpdateUI;

    }

    private void UpdateUI(string statName, float value)
    {

        if (materialInstance == null || statModule == null) return;

        float fill = 0f;

        if (statType == StatType.Health && statName.Equals("health", StringComparison.OrdinalIgnoreCase))
        {
            float ratio = statModule.currentStats.currentHealth / statModule.currentStats.maxHealth;
            fill = 1f - ratio;
            materialInstance.SetFloat(shaderPropertyName, fill);

            if (ratio <= 0.25f && !lowHPTriggered)
            {
                lowHPTriggered = true;
                StartLowHealthWarning();
            }
            else if (ratio > 0.25f && lowHPTriggered)
            {
                lowHPTriggered = false;
                StopLowHealthWarning();
            }

            if (!lowHPTriggered)
            {
                JiggleBar(uiImage.rectTransform);
            }
        }

        else if (statType == StatType.Stamina && statName.Equals("stamina", StringComparison.OrdinalIgnoreCase))
        {
            fill = 1f - (statModule.currentStats.currentStamina / statModule.currentStats.maxStamina);
            materialInstance.SetFloat(shaderPropertyName, fill);
        }

        
    }

    void JiggleBar(RectTransform bar)
    {
        LeanTween.scale(bar, original * 1.05f, 0.15f)
            .setEasePunch()
            .setLoopOnce()
            .setOnComplete(() => bar.localScale = original); // reset scale
    }
    void StartLowHealthWarning()
    {
        // Cancel any tweens affecting the transform to avoid conflict
        LeanTween.cancel(uiImage.rectTransform);

        // Start the low health pulsing tween
        LeanTween.scale(uiImage.rectTransform, original * 1.05f, 0.4f)
            .setEaseInOutSine()
            .setLoopPingPong();

        LeanTween.delayedCall(0.8f, RepeatLowHealthSound);
    }


    void RepeatLowHealthSound()
    {
        UISoundManager.Instance?.PlayLowHP();
        // Repeat the sound after 0.8 seconds if necessary
        LeanTween.delayedCall(0.8f, RepeatLowHealthSound);
    }

    void StopLowHealthWarning()
    {
        if (heartbeatSource != null)
        {
            heartbeatSource.Stop();
            Destroy(heartbeatSource);
        }

        LeanTween.cancel(uiImage.rectTransform);
        uiImage.rectTransform.localScale = original;
    }

    public void ResetEffects()
    {
        lowHPTriggered = false;
        LeanTween.cancelAll();
        uiImage.rectTransform.localScale = original;
        materialInstance.SetFloat(shaderPropertyName, 0f);
    }

}
