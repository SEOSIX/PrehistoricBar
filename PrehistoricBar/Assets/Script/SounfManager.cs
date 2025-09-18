using System;
using UnityEngine;

public class SounfManager : MonoBehaviour
{
    public static SounfManager Singleton { get; private set; }
    public AudioClip[] sounds;
    
    public AudioSource soundToPlay;

    private void Awake()
    {
        if (Singleton != null && Singleton != this)
        {
            Destroy(gameObject);
            return;
        }
        Singleton = this;
        DontDestroyOnLoad(gameObject);
    }

    public void PlaySound(int index, bool loop = false)
    {
        if (index < 0 || index >= sounds.Length)
        {
            return;
        }

        if (loop)
        {
            soundToPlay.loop = true;
        }
        else
        {
            soundToPlay.loop = false;
        }
        soundToPlay.clip = sounds[index];
        soundToPlay.Play();
    }
    
    public void StopSound(int index)
    {
        if (index < 0 || index >= sounds.Length)
        {
            return;
        }
        soundToPlay.clip = sounds[index];
        soundToPlay.Stop();
    }
}
