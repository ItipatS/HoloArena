using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public class ConfirmationDialog : MonoBehaviour
{
    public static ConfirmationDialog Instance { get; private set; }
    public Button confirmButton;
    public Button cancelButton;
    public CanvasGroup canvasGroup;
    private UnityAction onConfirmAction;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        canvasGroup.alpha = 0;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;
        // Set up the cancel button to hide the dialog
        cancelButton.onClick.AddListener(Hide);
    }

    // Call this method to show the confirmation dialog
    public void Show(UnityAction onConfirm)
    {
        onConfirmAction = onConfirm;

        confirmButton.onClick.RemoveAllListeners();
        confirmButton.onClick.AddListener(() =>
        {
            onConfirmAction?.Invoke();
            Hide();
        });

        canvasGroup.alpha = 1;
        canvasGroup.interactable = true;
        canvasGroup.blocksRaycasts = true;
    }

    public void Hide()
    {
        canvasGroup.alpha = 0;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;
    }
}
