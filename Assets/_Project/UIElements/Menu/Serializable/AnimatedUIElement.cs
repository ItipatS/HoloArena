using UnityEngine;

[System.Serializable]
public class AnimatedUIElement
{
    public RectTransform rect;
    public SlideDirection slideFrom = SlideDirection.Bottom;
    [HideInInspector] public Vector2 originalAnchoredPosition;
    public float slideDistance = 500f;
    public float slideDelay = 0f;
    public float pulseScale = 1.01f;
    public float pulseDuration = 1f;
}
