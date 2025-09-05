using UnityEngine;

[System.Serializable]
public class SoundClipMapping
{
    public string soundID;
    public AudioClip clip;
    [Range(0f, 1f)] public float volume = 1f;
}
