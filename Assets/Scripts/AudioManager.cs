using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.Audio;

public class AudioManager : MonoBehaviour
{
    public Sounds[] sounds;

    public static AudioManager instance;
    public bool airAudioIsPlaying = false;

    private void Awake()
    {
        if (instance == null)
            instance = this;
        else
        {
            Destroy(gameObject);
            return;
        }

        DontDestroyOnLoad(gameObject);

        foreach (Sounds s in sounds)
        {
            s.source = gameObject.AddComponent<AudioSource>();
            s.source.clip = s.clip;

            s.source.volume = s.volume;
            s.source.pitch = s.pitch;
            s.source.loop = s.loop;
        }

    }

    private IEnumerator Start()
    {
        PlaySound("themeIntro");
        yield return new WaitForSecondsRealtime(.736f);
        PlaySound("theme");

    }

    public void PlaySound(string name)
    {
        Sounds s = Array.Find(sounds, sound => sound.name == name);
        s.source.Play();

        if(name.Equals("air"))
            airAudioIsPlaying = true;
    }
    public void StopSound(string name)
    {
        Sounds s = Array.Find(sounds, sound => sound.name == name);
        s.source.Stop();

        if (name.Equals("air"))
            airAudioIsPlaying = false;
    }
}
