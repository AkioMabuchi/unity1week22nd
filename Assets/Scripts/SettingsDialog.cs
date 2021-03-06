using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
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
    [SerializeField] private GameObject dialogContents;
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
            contents.alpha = 0;
            dialogContents.transform.localScale = new Vector3(0.2f, 0.2f, 0.2f);

            contents.DOFade(1, 0.2f);
            dialogContents.transform.DOScale(new Vector3(1, 1, 1), 0.3f).SetEase(Ease.InOutCirc);
        }).AddTo(gameObject);

        _onHide.Subscribe(_ =>
        {
            contents.alpha = 1;
            dialogContents.transform.localScale = new Vector3(1, 1, 1);

            contents.DOFade(0, 0.2f);
            dialogContents.transform.DOScale(new Vector3(0, 0), 0.5f).SetEase(Ease.OutCirc).OnComplete(() =>
            {
                contents.gameObject.SetActive(false);
            });
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
            SoundPlayer.PlaySound("Test");
        }).AddTo(gameObject);

        eventTriggerSliderSound.OnPointerUpAsObservable().Subscribe(_ =>
        {
            SoundPlayer.PlaySound("Test");
        }).AddTo(gameObject);
    }

    private void Start()
    {
        contents.gameObject.SetActive(false);
    }
}
