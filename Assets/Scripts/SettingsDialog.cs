using System;
using System.Collections;
using System.Collections.Generic;
using UniRx;
using UniRx.Triggers;
using UnityEngine;
using UnityEngine.UI;

public class SettingsDialog : MonoBehaviour
{
    private static readonly Subject<Unit> _onShow = new();
    private static readonly Subject<Unit> _onHide = new();

    public static void Show()
    {
        _onShow.OnNext(Unit.Default);
    }

    public static void Hide()
    {
        _onHide.OnNext(Unit.Default);
    }

    [SerializeField] private CanvasGroup contents;
    [SerializeField] private Slider sliderMaster;
    [SerializeField] private Slider sliderMusic;
    [SerializeField] private Slider sliderSound;
    [SerializeField] private ObservableEventTrigger eventTriggerSliderMaster;
    [SerializeField] private ObservableEventTrigger eventTriggerSliderSound;

    private void Awake()
    {
        SettingsModel.MasterVolume.Subscribe(masterVolume =>
        {
            sliderMaster.value = masterVolume;
        }).AddTo(gameObject);

        SettingsModel.MusicVolume.Subscribe(musicVolume =>
        {
            sliderMusic.value = musicVolume;
        }).AddTo(gameObject);

        SettingsModel.SoundVolume.Subscribe(soundVolume =>
        {
            sliderSound.value = soundVolume;
        }).AddTo(gameObject);
        
        _onShow.Subscribe(_ =>
        {
            contents.gameObject.SetActive(true);
        }).AddTo(gameObject);

        _onHide.Subscribe(_ =>
        {
            contents.gameObject.SetActive(false);
        }).AddTo(gameObject);

        sliderMaster.OnValueChangedAsObservable().Subscribe(masterVolume =>
        {
            SettingsModel.UpdateMasterVolume(masterVolume);
        }).AddTo(gameObject);

        sliderMusic.OnValueChangedAsObservable().Subscribe(musicVolume =>
        {
            SettingsModel.UpdateMusicVolume(musicVolume);
        }).AddTo(gameObject);

        sliderSound.OnValueChangedAsObservable().Subscribe(soundVolume =>
        {
            SettingsModel.UpdateSoundVolume(soundVolume);
        }).AddTo(gameObject);

        eventTriggerSliderMaster.OnPointerUpAsObservable().Subscribe(_ =>
        {
            SoundPlayer.PlaySound("PutPiece");
        }).AddTo(gameObject);

        eventTriggerSliderSound.OnPointerUpAsObservable().Subscribe(_ =>
        {
            SoundPlayer.PlaySound("PutPiece");
        }).AddTo(gameObject);
    }

    private void Start()
    {
        contents.gameObject.SetActive(false);
    }
}
