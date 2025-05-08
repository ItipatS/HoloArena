using UnityEngine;

public class UITween : MonoBehaviour
{
    public static void SlideIn(RectTransform rect, SlideDirection from, float distance = 300f, float duration = 0.5f, float delay = 0f, LeanTweenType ease = LeanTweenType.easeOutBack)
    {
        Vector2 originalPos = rect.anchoredPosition;
        Vector2 startPos = originalPos;

        switch (from)
        {
            case SlideDirection.Top:
                startPos += new Vector2(0, distance);
                break;
            case SlideDirection.Bottom:
                startPos -= new Vector2(0, distance);
                break;
            case SlideDirection.Left:
                startPos -= new Vector2(distance, 0);
                break;
            case SlideDirection.Right:
                startPos += new Vector2(distance, 0);
                break;
        }

        rect.anchoredPosition = startPos;

        LeanTween.move(rect, originalPos, duration)
            .setEase(ease)
            .setUseEstimatedTime(true) 
            .setDelay(delay);
    }

    public static void SlideOut(RectTransform rect, SlideDirection to, float distance = 300f, float duration = 0.5f, float delay = 0f, LeanTweenType ease = LeanTweenType.easeInBack, bool deactivateAfter = false)
    {
        Vector2 originalPos = rect.anchoredPosition;
        Vector2 targetPos = originalPos;

        switch (to)
        {
            case SlideDirection.Top:
                targetPos += new Vector2(0, distance);
                break;
            case SlideDirection.Bottom:
                targetPos -= new Vector2(0, distance);
                break;
            case SlideDirection.Left:
                targetPos -= new Vector2(distance, 0);
                break;
            case SlideDirection.Right:
                targetPos += new Vector2(distance, 0);
                break;
        }

        LeanTween.move(rect, targetPos, duration)
            .setEase(ease)
            .setDelay(delay)
            .setUseEstimatedTime(true) 
            .setOnComplete(() =>
            {
                if (deactivateAfter)
                    rect.gameObject.SetActive(false);
            });
    }


    public static void ScalePulse(GameObject target, float scaleMultiplier = 1.05f, float duration = 0.5f)
    {
        Vector3 originalScale = target.transform.localScale;

        LeanTween.scale(target, originalScale * scaleMultiplier, duration)
            .setEaseInOutSine()
            .setUseEstimatedTime(true) 
            .setLoopPingPong();
    }
}
