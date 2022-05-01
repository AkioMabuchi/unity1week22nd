using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class Sound : MonoBehaviour
{
    [SerializeField] private AudioSource audioSource;

    private AudioClip _audioClip;
    private void Reset()
    {
        audioSource = GetComponent<AudioSource>();
    }

    private IEnumerator Start()
    {
        audioSource.clip = _audioClip;
        audioSource.Play();
        yield return new WaitUntil(() => !audioSource.isPlaying);
        Destroy(gameObject);
    }
    public void SetAudioClip(AudioClip audioClip)
    {
        _audioClip = audioClip;
    }
}
