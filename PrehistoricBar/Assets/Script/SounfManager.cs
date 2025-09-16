using System;
using UnityEngine;

public class SounfManager : MonoBehaviour
{
    public static SounfManager instance { get; private set; }
    public AudioClip[] sounds;
    
    public AudioSource soundToPlay;

    private void Awake()
    {
        instance = this;
    }

    void PlaySoundButtonMenu()
    {
        sounds[0].samples = soundToPlay.clip.samples;
    }

    void SoundClient()
    {
        
    }

    void soundclientCome()
    {
        
    }

    void clochette()
    {
        
    }

    void bave()
    {
        
    }

    void liquide()
    {
        
    }

    void Mortier()
    {
        
    }
}
