using UnityEngine;
using System.Collections.Generic;
using System.Data;

public class AudioManager : MonoBehaviour
{
    // Singleton instance
    public static AudioManager Instance;

    // Dictionary to hold sound effects.
    public List<SoundClipMapping> soundMappings = new List<SoundClipMapping>();
    public Dictionary<string, SoundClipMapping> soundClips = new Dictionary<string, SoundClipMapping>();
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
                soundClips.Add(mapping.soundID, mapping);
            }
        }
    }

    public void PlaySound(string soundID)
    {
        if (soundClips.ContainsKey(soundID))
        {
            var mapping = soundClips[soundID];
            AudioSource tempSource = gameObject.AddComponent<AudioSource>();
            tempSource.ignoreListenerPause = true;
            tempSource.pitch = Random.Range(0.9f, 1.1f);

            tempSource.PlayOneShot(mapping.clip, mapping.volume);
            Destroy(tempSource, mapping.clip.length);
        }
        else
        {
            Debug.LogWarning("Sound ID not found: " + soundID);
        }
    }

}
