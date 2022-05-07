using System;
using System.Collections;
using System.Collections.Generic;
using UniRx;
using UniRx.Triggers;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class MusicPlayer : MonoBehaviour
{
    [SerializeField] private AudioSource audioSource;

    private void Reset()
    {
        audioSource = GetComponent<AudioSource>();
    }

    private void Awake()
    {
        this.UpdateAsObservable()
            .Where(_ => audioSource.isPlaying)
            .Where(_ => audioSource.time > 72.0f)
            .Subscribe(_ =>
            {
                audioSource.time = 0.0f;
            }).AddTo(gameObject);
    }

    private IEnumerator Start()
    {
        yield return new WaitForSeconds(2.0f);
        audioSource.Play();
    }
}
