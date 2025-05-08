using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class pauseMenu : MonoBehaviour
{
    public static pauseMenu Instance { get; private set; }
    public CanvasGroup pauseCanvas;
    private bool isPaused = false;
    public List<AnimatedUIElement> animatedElements;
    public SandwichTransition transition;
    public Button pause;
    public Button home;
    public Button resume;
    public Button re;
    public Button cha;

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
        foreach (var el in animatedElements)
        {
            if (el.rect != null)
                el.originalAnchoredPosition = el.rect.anchoredPosition;
        }

        SetupButtons();
        HidePause();
    }

    void Update()
    {
        // Toggle pause menu when pressing Escape
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (!isPaused)
                PauseGame();
            else
                ResumeGame();
        }
    }

    void SetupButtons()
    {
        ButtonAnimatorUtility.SetupButton(pause, 10f, PauseGame);
        ButtonAnimatorUtility.SetupButton(home, 10f, OnReturnToMainMenuPressed);
        ButtonAnimatorUtility.SetupButton(resume, 10f, ResumeGame);
        ButtonAnimatorUtility.SetupButton(re, 10f, OnRestartPressed);
        ButtonAnimatorUtility.SetupButton(cha, 10f, OnSelectCharacterPressed);
    }
    private void ShowPause()
    {
        pauseCanvas.alpha = 1;
        pauseCanvas.interactable = true;
        pauseCanvas.blocksRaycasts = true;

        foreach (var el in animatedElements)
        {
            if (el.rect == null) continue;

            LeanTween.cancel(el.rect.gameObject);

            el.rect.localScale = Vector3.one;
            el.rect.anchoredPosition = el.originalAnchoredPosition;

            UITween.SlideIn(el.rect, el.slideFrom, el.slideDistance, 2f, el.slideDelay);
            UITween.ScalePulse(el.rect.gameObject, el.pulseScale, el.pulseDuration);
        }
    }

    void HidePause()
    {
        pauseCanvas.alpha = 0;
        pauseCanvas.interactable = false;
        pauseCanvas.blocksRaycasts = false;
    }


    public void PauseGame()
    {
        isPaused = true;
        ShowPause();
        pause.gameObject.SetActive(false); // hide the pause button itself
        Time.timeScale = 0f; // Freeze game
        
    }

    public void ResumeGame()
    {
        isPaused = false;
        HidePause();

        Time.timeScale = 1f; // Resume game
        pause.gameObject.SetActive(true);
        LayoutRebuilder.ForceRebuildLayoutImmediate(pauseCanvas.GetComponent<RectTransform>());

    }
    public void OnReturnToMainMenuPressed()
    {
        ConfirmationDialog.Instance.Show(() =>
        {
            Time.timeScale = 1f;
            // Load your main menu scene
            ChangeScene("MainMenu");
        });
    }

    public void OnRestartPressed()
    {
        ConfirmationDialog.Instance.Show(() =>
       {
           Time.timeScale = 1f;
           LeanTween.moveX(pauseCanvas.gameObject, -(Screen.width + Screen.width), 0.6f) // faster
            .setEaseInBack()
            .setUseEstimatedTime(true)
            .setOnComplete(() =>
            {
                SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
            });
       });
    }

    public void OnSelectCharacterPressed()
    {
        ConfirmationDialog.Instance.Show(() =>
       {
           // Resume time before switching scenes
           Time.timeScale = 1f;
           // Load your character selection scene
           ChangeScene("CharacterSelection");
       });
    }

    public void ChangeScene(string Scene)
    {

        LeanTween.moveX(pauseCanvas.gameObject, -(Screen.width + Screen.width), 0.6f) // faster
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
