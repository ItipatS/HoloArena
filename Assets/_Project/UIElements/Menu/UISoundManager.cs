using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;

public class UISoundManager : MonoBehaviour
{
    public List<SoundClipMapping> uiSoundMappings = new List<SoundClipMapping>();
    public Dictionary<string, AudioClip> soundClips = new Dictionary<string, AudioClip>();
    public static UISoundManager Instance;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }


        // Populate the dictionary from the mapping list.
        foreach (var mapping in uiSoundMappings)
        {
            if (!soundClips.ContainsKey(mapping.soundID) && mapping.clip != null)
            {
                soundClips.Add(mapping.soundID, mapping.clip);
            }
        }
    }

    public void PlaySound(string soundID, float volume = .2f)
    {
        if (soundClips.ContainsKey(soundID))
        {
            AudioSource tempSource = gameObject.AddComponent<AudioSource>();
            tempSource.ignoreListenerPause = true;
            tempSource.volume = volume;
            tempSource.PlayOneShot(soundClips[soundID]);
            Destroy(tempSource, soundClips[soundID].length);
        }
        else
        {
            Debug.LogWarning("Sound ID not found: " + soundID);
        }
    }

    public void PlayHover()
    {
        PlaySound("Hover");
    }

    public void PlayConfirm()
    {
        PlaySound("Confirm");
    }

    public void PlayTransitionIn()
    {
        PlaySound("TransitionIn");
    }
    public void PlayTransitionOut()
    {
        PlaySound("TransitionOut");
    }

    public void PlayCancel()
    {
        PlaySound("Cancel");
    }

    public void PlayKO()
    {

        PlaySound("KO");
    }

    public void PlayLowHP()
    {
        PlaySound("LowHP");
    }
}
