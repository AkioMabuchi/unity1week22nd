using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class MusicPlayer : MonoBehaviour
{
    [SerializeField] private AudioSource audioSource;

    private void Reset()
    {
        audioSource = GetComponent<AudioSource>();
    }

    private IEnumerator Start()
    {
        yield return new WaitForSeconds(2.0f);
        audioSource.Play();
    }
}
