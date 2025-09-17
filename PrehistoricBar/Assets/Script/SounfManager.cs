using System;
using UnityEngine;

public class SounfManager : MonoBehaviour
{
    public static SounfManager singleton { get; private set; }
    public AudioClip[] sounds;
    
    public AudioSource soundToPlay;

    private void Awake()
    {
        singleton = this;
    }

    public void PlaySound(int index)
    {
        if (index < 0 || index >= sounds.Length)
        {
            return;
        }
        soundToPlay.clip = sounds[index];
        soundToPlay.Play();
    }
}
