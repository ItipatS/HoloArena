using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class KOUIManager : MonoBehaviour
{
    public static KOUIManager Instance;
    public RectTransform koImage;
    public float fadeDuration = 0.3f;
    public float holdDuration = 1f;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    public void ShowKO(System.Action onComplete = null)
    {
        UISoundManager.Instance?.PlayKO();
        koImage.gameObject.SetActive(true);
        koImage.localScale = Vector3.one * 2f;

        CanvasGroup cg = koImage.GetComponent<CanvasGroup>() ?? koImage.gameObject.AddComponent<CanvasGroup>();
        cg.alpha = 0;

        LeanTween.scale(koImage, Vector3.one, fadeDuration).setEaseOutBack().setIgnoreTimeScale(true);
        LeanTween.value(koImage.gameObject, 0f, 1f, fadeDuration)
            .setOnUpdate((float val) => { cg.alpha = val; })
            .setOnComplete(() =>
            {
                LeanTween.delayedCall(holdDuration, () =>
                {
                    HideKO(onComplete);
                });
            });
    }

    public void HideKO(System.Action onComplete = null)
    {
        CanvasGroup cg = koImage.GetComponent<CanvasGroup>();
        LeanTween.value(koImage.gameObject, 1f, 0f, fadeDuration)
            .setOnUpdate((float val) => { cg.alpha = val; })
            .setOnComplete(() =>
            {
                koImage.gameObject.SetActive(false);
                onComplete?.Invoke();
            });
    }
}
