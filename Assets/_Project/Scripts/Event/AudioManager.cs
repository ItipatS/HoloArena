using UnityEngine;
using System.Collections.Generic;

public class AudioManager : MonoBehaviour
{
    // Singleton instance
    public static AudioManager Instance;

    // Dictionary to hold sound effects.
    public List<SoundClipMapping> soundMappings = new List<SoundClipMapping>();
    public Dictionary<string, AudioClip> soundClips = new Dictionary<string, AudioClip>();
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        // Populate the dictionary from the mapping list.
        foreach (var mapping in soundMappings)
        {
            if (!soundClips.ContainsKey(mapping.soundID) && mapping.clip != null)
            {
                soundClips.Add(mapping.soundID, mapping.clip);
            }
        }
    }

    public void PlaySound(string soundID)
    {
        if (soundClips.ContainsKey(soundID))
        {
            AudioSource tempSource = gameObject.AddComponent<AudioSource>();
            tempSource.ignoreListenerPause = true;
            tempSource.pitch = Random.Range(0.9f, 1.1f);
            tempSource.PlayOneShot(soundClips[soundID]);
            Destroy(tempSource, soundClips[soundID].length);
        }
        else
        {
            Debug.LogWarning("Sound ID not found: " + soundID);
        }
    }

}
