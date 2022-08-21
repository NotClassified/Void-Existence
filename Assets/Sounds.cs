using UnityEngine;
using UnityEngine.Audio;

[System.Serializable]
public class Sounds
{
    public string name;

    [HideInInspector]
    public AudioSource source;
    public AudioClip clip;

    [Range(0f, 1f)]
    public float volume;
    [Range(.5f, 1.5f)]
    public float pitch;
    public bool loop;
}
