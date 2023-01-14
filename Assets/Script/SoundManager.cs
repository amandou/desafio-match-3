using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    private AudioSource audioSourceSFX;
    [SerializeField] private GameHandler gameHandler;

    void Start()
    {
        audioSourceSFX = GetComponent<AudioSource>();
    }

    private void PlaySound(AudioClip audio)
    {
        audioSourceSFX.clip = audio;
        audioSourceSFX.Play();
    }

    private void OnEnable()
    {
        GameHandler.onPlaySound += PlaySound;
    }

    private void OnDisable()
    {
        GameHandler.onPlaySound -= PlaySound;
    }
}
