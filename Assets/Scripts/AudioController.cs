using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Singleton class responsible for managing audio playback within the game.
/// </summary>
public class AudioController : MonoBehaviour
{
    public static AudioController Instance { get; private set; }
    public AudioSource source;
    public AudioClip clip;

    void Start()
    {
        Instance = this;
    }
    
    /// <summary>
    /// Plays the assigned audio clip once using the AudioSource component.
    /// </summary>
    public void PlaySound()
    {
        source.PlayOneShot(clip);
    }
}
