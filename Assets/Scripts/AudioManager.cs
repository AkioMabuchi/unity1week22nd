using System;
using System.Collections;
using System.Collections.Generic;
using UniRx;
using UnityEngine;
using UnityEngine.Audio;

public class AudioManager : MonoBehaviour
{
    [SerializeField] private AudioMixer audioMixer;

    private void Awake()
    {
        SettingsModel.MasterVolume.Subscribe(masterVolume =>
        {
            audioMixer.SetFloat("Master", ClampVolume(masterVolume));
        }).AddTo(gameObject);
        
        SettingsModel.MusicVolume.Subscribe(musicVolume =>
        {
            audioMixer.SetFloat("Music", ClampVolume(musicVolume));
        }).AddTo(gameObject);

        SettingsModel.SoundVolume.Subscribe(soundVolume =>
        {
            audioMixer.SetFloat("Sound",  ClampVolume(soundVolume));
        }).AddTo(gameObject);
    }

    private void Start()
    {
        audioMixer.SetFloat("Master", ClampVolume(SettingsModel.MasterVolume.Value));
        audioMixer.SetFloat("Music", ClampVolume(SettingsModel.MusicVolume.Value));
        audioMixer.SetFloat("Sound", ClampVolume(SettingsModel.SoundVolume.Value));
    }

    private static float ClampVolume(float volume)
    {
        return volume > -40.0f ? volume : -80.0f;
    }
}
