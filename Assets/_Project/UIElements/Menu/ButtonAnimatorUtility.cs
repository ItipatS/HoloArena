using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public static class ButtonAnimatorUtility
{
    public static void SetupButton(Button button, float tiltAmount, System.Action onClick = null)
    {
        if (button == null) return;

        GameObject btnObj = button.gameObject;

        // Tilt animation
        EventTrigger trigger = btnObj.GetComponent<EventTrigger>();
        if (trigger == null)
            trigger = btnObj.AddComponent<EventTrigger>();

        trigger.triggers.Clear(); // clean up old events if reused

        // PointerEnter
        AddEvent(trigger, EventTriggerType.PointerEnter, () =>
        {
            UISoundManager.Instance?.PlayHover();

            LeanTween.cancel(btnObj, false);

            float randomizedTilt = tiltAmount * (Random.value > 0.5f ? 1 : -1);
            LeanTween.rotateZ(btnObj, randomizedTilt, 0.2f).setEaseOutExpo().setUseEstimatedTime(true) ;
        });

        // PointerExit
        AddEvent(trigger, EventTriggerType.PointerExit, () =>
        {
            LeanTween.cancel(btnObj, false);
            
            LeanTween.rotateZ(btnObj, 0f, 0.2f).setEaseOutExpo().setUseEstimatedTime(true) ;
        });

        // OnClick
        if (onClick != null)
        {
            button.onClick.RemoveAllListeners(); // Prevent duplication
            button.onClick.AddListener(() =>
            {
                // Sound + action are separate, easy to expand
                UISoundManager.Instance?.PlayConfirm();
                onClick.Invoke();
            });
        }

    }

    private static void AddEvent(EventTrigger trigger, EventTriggerType type, System.Action action)
    {
        EventTrigger.Entry entry = new EventTrigger.Entry();
        entry.eventID = type;
        entry.callback.AddListener((_) => action());
        trigger.triggers.Add(entry);
    }
}
