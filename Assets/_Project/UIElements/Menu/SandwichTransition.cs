using System.Collections;
using UnityEngine;

public class SandwichTransition : MonoBehaviour
{
    public RectTransform topPanel;
    public RectTransform bottomPanel;
    public float duration = 0.5f;

    void Start()
    {
        StartCoroutine(PlayEntryTransition());
    }
    
    public void PlayTransition(System.Action onComplete)
    {
        // Slide panels in
        LeanTween.moveY(topPanel, 0, duration).setEaseInOutQuart();
        LeanTween.moveY(bottomPanel, 0, duration).setEaseInOutQuart().setOnComplete(() =>
        {
            UISoundManager.Instance?.PlayTransitionIn();
            onComplete?.Invoke();
        });
    }

    public void PlayReverse()
    {
        // Slide panels back out
        float screenH = Screen.height;
        UISoundManager.Instance?.PlayTransitionOut();
        LeanTween.moveY(topPanel, screenH, duration).setEaseInOutQuart();
        LeanTween.moveY(bottomPanel, -screenH, duration).setEaseInOutQuart();
    }

    IEnumerator PlayEntryTransition()
    {
        // Make sure panels are "closed" at start
        topPanel.anchoredPosition = Vector2.zero;
        bottomPanel.anchoredPosition = Vector2.zero;

        yield return new WaitForSecondsRealtime(0.05f); // allow UI to fully layout
        PlayReverse();
    }

    public void PlayEntry()
    {
        StartCoroutine(PlayEntryTransition());
    }

}
