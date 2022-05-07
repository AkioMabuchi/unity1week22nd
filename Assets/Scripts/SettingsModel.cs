using System.Collections;
using System.Collections.Generic;
using UniRx;
using UnityEngine;

public static class SettingsModel
{
    private const string MasterVolumeKey = "MasterVolume";
    private const string MusicVolumeKey = "MusicVolume";
    private const string SoundVolumeKey = "SoundVolume";

    private static readonly ReactiveProperty<float> _masterVolume = new(0.0f);
    public static IReadOnlyReactiveProperty<float> MasterVolume => _masterVolume;
    private static readonly ReactiveProperty<float> _musicVolume = new(0.0f);
    public static IReadOnlyReactiveProperty<float> MusicVolume => _musicVolume;
    private static readonly ReactiveProperty<float> _soundVolume = new(0.0f);
    public static IReadOnlyReactiveProperty<float> SoundVolume => _soundVolume;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Initialize()
    {
        if (ES3.KeyExists(MasterVolumeKey))
        {
            _masterVolume.Value = ES3.Load<float>(MasterVolumeKey);
        }
        else
        {
            ES3.Save(MasterVolumeKey, 0.0f);
        }
        
        if (ES3.KeyExists(MusicVolumeKey))
        {
            _musicVolume.Value = ES3.Load<float>(MusicVolumeKey);
        }
        else
        {
            ES3.Save(MusicVolumeKey, 0.0f);
        }

        if (ES3.KeyExists(SoundVolumeKey))
        {
            _soundVolume.Value = ES3.Load<float>(SoundVolumeKey);
        }
        else
        {
            ES3.Save(SoundVolumeKey, 0.0f);
        }
    }

    public static void UpdateMasterVolume(float masterVolume)
    {
        _masterVolume.Value = masterVolume;
        ES3.Save(MasterVolumeKey, masterVolume);
    }
    public static void UpdateMusicVolume(float musicVolume)
    {
        _musicVolume.Value = musicVolume;
        ES3.Save(MusicVolumeKey, musicVolume);
    }

    public static void UpdateSoundVolume(float soundVolume)
    {
        _soundVolume.Value = soundVolume;
        ES3.Save(SoundVolumeKey, soundVolume);
    }
}
