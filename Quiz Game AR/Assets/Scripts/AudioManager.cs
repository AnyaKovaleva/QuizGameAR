using UnityEngine.Audio;
using UnityEngine;
using System;

public class AudioManager : MonoBehaviour
{
    public Sound[] sounds;

    private void Awake()
    {
        foreach(Sound sound in sounds)
        {
            sound.source = gameObject.AddComponent<AudioSource>();
            sound.source.clip = sound.clip;
            sound.source.volume = sound.volume;
            sound.source.pitch = sound.pitch;
            sound.source.loop = sound.loop;
        }
    }

    public void Play(string soundName)
    {
        Sound s = GetSound(soundName);
        if(s==null)
        {
            Debug.LogWarning("Sound: " + soundName + " not found!");
            return;
        }
        s.source.Play();
    }

    public bool IsPlaying(string soundName)
    {
        Sound s = GetSound(soundName);
        if(s != null)
        {
            return s.source.isPlaying;
        }

        return false;
    }

    public Sound GetSound(string soundName)
    {
        return Array.Find(sounds, sound => sound.name == soundName);
    }
}
