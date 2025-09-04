using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public enum SlideDirection
{
    Top,
    Bottom,
    Left,
    Right
}
public class MainMenuController : MonoBehaviour
{
    public RectTransform mainMenuPanel;
    public Button playButton;
    public Button exitButton;
    public SandwichTransition transition;
    public List<MenuButtonAction> menuButtons;
    public List<AnimatedUIElement> animatedElements;

    void Start()
    {
        foreach (var b in menuButtons)
        {
            ButtonAnimatorUtility.SetupButton(b.button, 10f, () =>
            {
                GameSessionManager.Instance.SetGameMode(b.modeToSet);
                DoTransition(() => SceneManager.LoadScene(b.sceneToLoad));
            });
        }

        foreach (var el in animatedElements)
        {
            if (el.rect == null) continue;

            UITween.SlideIn(el.rect, el.slideFrom, el.slideDistance, 2f, el.slideDelay);
            UITween.ScalePulse(el.rect.gameObject, el.pulseScale, el.pulseDuration);
        }

        ButtonAnimatorUtility.SetupButton(playButton, 10f, OnSelectCharacterPressed);
        ButtonAnimatorUtility.SetupButton(exitButton, 10f, OnExitPressed);
    }

    public void OnSelectCharacterPressed()
    {
        GameSessionManager.Instance.gameMode = GameMode.Local;
        DoTransition(() => SceneManager.LoadScene("CharacterSelection"));
    }

    public void OnExitPressed()
    {
        DoTransition(() =>
        {
            UISoundManager.Instance?.PlayTransitionOut();
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
        });
    }

    public void DoTransition(System.Action onComplete)
    {
        foreach (Button b in mainMenuPanel.GetComponentsInChildren<Button>())
            b.interactable = false;

        foreach (RectTransform R in mainMenuPanel.GetComponentInChildren<RectTransform>())
        {
            UITween.SlideOut(R, SlideDirection.Bottom, 500f, 2f, 0.2f);
        }

        // Slide UI off screen, then play the transition, then call onComplete
        LeanTween.moveX(mainMenuPanel, -(Screen.width + Screen.width), 0.6f)
            .setEaseInBack()
            .setOnComplete(() =>
            {
                transition.PlayTransition(() =>
                {
                    onComplete?.Invoke();
                });
            });
    }
}
