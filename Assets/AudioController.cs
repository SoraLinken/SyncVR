using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioController : MonoBehaviour
{
    public static AudioController Instance { get; private set; }
    public AudioSource source;
    public AudioClip clip;

    void Start()
    {
        Instance = this;
    }
    
    public void PlaySound()
    {
        source.PlayOneShot(clip);
    }
}
