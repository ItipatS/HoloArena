using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class winMenu : MonoBehaviour
{
    public static winMenu Instance { get; private set; }
    public CanvasGroup winCanvas;
    public TMPro.TextMeshProUGUI Wintext;
    public List<AnimatedUIElement> animatedElements;
    public Button cha;
    public Button home;
    public Button replay;
    public SandwichTransition transition;
    void Awake()
    {
        // Make sure only one exists
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

    }
     void Start()
    {
        SetupButtons();
        HideWinMenu();
    }

    void SetupButtons()
    {
        ButtonAnimatorUtility.SetupButton(cha, 10f, OnSelectCharacterPressed);
        ButtonAnimatorUtility.SetupButton(home, 10f, OnReturnToMainMenuPressed);
        ButtonAnimatorUtility.SetupButton(replay, 10f, OnRestartPressed);
    }

    void ShowWinMenu()
    {
        winCanvas.alpha = 1;
        winCanvas.interactable = true;
        winCanvas.blocksRaycasts = true;

        foreach (var el in animatedElements)
        {
            if (el.rect == null) continue;
            UITween.SlideIn(el.rect, el.slideFrom, el.slideDistance, 2f, el.slideDelay);
            UITween.ScalePulse(el.rect.gameObject, el.pulseScale, el.pulseDuration);
        }
    }

    void HideWinMenu()
    {
        winCanvas.alpha = 0;
        winCanvas.interactable = false;
        winCanvas.blocksRaycasts = false;
    }
    // This method can be called from the win condition in your game
    public void ShowWin(string winnerName)
    {
        Wintext.text = $"{winnerName} Wins!";
        Time.timeScale = 0f;
        ShowWinMenu();
    }

     public void OnReturnToMainMenuPressed()
    {
        Time.timeScale = 1f;
        ChangeScene("MainMenu");
    }

    public void OnRestartPressed()
    {
        Time.timeScale = 1f;
        ChangeScene(SceneManager.GetActiveScene().name);
    }

    public void OnSelectCharacterPressed()
    {
        Time.timeScale = 1f;
        ChangeScene("CharacterSelection");
    }

    public void ChangeScene(string Scene)
    {

        LeanTween.moveX(winCanvas.gameObject, -(Screen.width + Screen.width), 0.6f) // faster
    .setEaseInBack()
    .setUseEstimatedTime(true) 
    .setOnComplete(() =>
    {
        transition.PlayTransition(() =>
        {
            SceneManager.LoadScene(Scene);
        });
    });
    }
}
