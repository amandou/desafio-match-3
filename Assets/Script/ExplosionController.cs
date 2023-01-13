using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExplosionController : ParticleController
{
    private AudioSource _audioSource;
    private AudioClip _explosionClip;

    private void Start()
    {
        _audioSource = GetComponent<AudioSource>();
        _explosionClip = _audioSource.clip;
        Debug.Log("Clip " + _explosionClip.name);
    }

    public override void Destroy()
    {
        _audioSource.Play();
        Destroy(gameObject);
    }
}
