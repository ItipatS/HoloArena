using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using System.Collections;

public class BGMManager : MonoBehaviour
{
    public static BGMManager Instance;

    public AudioSource audioSource;
    public AudioClip menuBGM;
    public AudioClip gameBGM;

    // List of scenes that use the same BGM
    private HashSet<string> menuScenes = new HashSet<string> { "MainMenu", "CharacterSelection" };
    private string currentScene;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        string newScene = scene.name;
        currentScene = newScene;

        // If we’re in a menu scene
        if (menuScenes.Contains(newScene))
        {
            PlayBGM(menuBGM, 0.5f);
        }
        else if (newScene == "MainGame") // Your gameplay scene
        {
            PlayBGM(gameBGM, 0.5f); // Or StopBGM() if you want silence
        }
        else
        {
            StopBGM();
        }
    }

    public void PlayBGM(AudioClip clip, float volume = .5f)
    {
        if (audioSource.clip == clip && audioSource.isPlaying) return;
        StartCoroutine(FadeAndSwitch(clip, volume));
    }

    public void StopBGM()
    {
        audioSource.Stop();
    }

    private IEnumerator FadeAndSwitch(AudioClip newClip, float volume)
{
    // Fade out
    while (audioSource.volume > 0.01f)
    {
        audioSource.volume -= Time.unscaledDeltaTime;
        yield return null;
    }

    audioSource.Stop();
    audioSource.clip = newClip;
    audioSource.Play();
    audioSource.loop = true;

    // Fade in
    while (audioSource.volume < volume)
    {
        audioSource.volume += Time.unscaledDeltaTime;
        yield return null;
    }

    audioSource.volume = volume;
}
}
