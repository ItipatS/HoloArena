using UnityEngine;
using UnityEngine.UI;

public class ScreenFlash : MonoBehaviour
{
    public static ScreenFlash Instance;
    private Image flashImage;

    void Awake()
    {
        Instance = this;
        flashImage = GetComponent<Image>();
        flashImage.color = new Color(1, 1, 1, 0);
    }

    public void Flash(float duration = 0.4f)
    {
        flashImage.color = new Color(1, 1, 1, 1);
        LeanTween.alpha(flashImage.rectTransform, 0f, duration).setEaseOutQuad();
    }
}
