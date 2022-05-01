using System;
using System.Collections;
using System.Collections.Generic;
using UniRx;
using UnityEngine;

public class SoundPlayer : MonoBehaviour
{
    private static readonly Subject<string> _onPlaySound = new();

    public static void PlaySound(string soundName)
    {
        _onPlaySound.OnNext(soundName);
    }
    
    [SerializeField] private GameObject prefabSound;
    [SerializeField] private SoundData soundData;

    private readonly Dictionary<string, AudioClip> _sounds = new();
    private void Awake()
    {
        _onPlaySound.Subscribe(soundName =>
        {
            if (_sounds.ContainsKey(soundName))
            {
                Instantiate(prefabSound, transform).GetComponent<Sound>().SetAudioClip(_sounds[soundName]);
            }
            else
            {
                Debug.LogWarning("そのサウンド名は登録されていません");
            }
        }).AddTo(gameObject);
        
        foreach (var sound in soundData.Sounds)
        {
            if (_sounds.ContainsKey(sound.name))
            {
                Debug.LogError("重複したキーが存在します。");
            }
            else
            {
                _sounds.Add(sound.name, sound.audioClip);
            }
        }
    }
}
