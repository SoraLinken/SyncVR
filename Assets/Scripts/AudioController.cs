using System.Collections;
using System.Collections.Generic;
using UnityEngine;


// Generic class to manage audio playback in the scene.
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
