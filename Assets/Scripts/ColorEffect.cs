using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

public class ColorEffect : MonoBehaviour
{
    private static readonly Subject<float> _onFadeIn = new();
    private static readonly Subject<float> _onFadeOut = new();
    private static readonly Subject<Color> _onChangeColor = new();

    public static void FadeIn(float duration)
    {
        _onFadeIn.OnNext(duration);
    }

    public static void FadeOut(float duration)
    {
        _onFadeOut.OnNext(duration);
    }

    public static void ChangeColor(Color color)
    {
        _onChangeColor.OnNext(color);
    }

    [SerializeField] private CanvasGroup contents;
    [SerializeField] private Image imageEffect;

    private void Awake()
    {
        _onFadeIn.Subscribe(duration =>
        {
            contents.gameObject.SetActive(true);
            contents.DOFade(1.0f, duration).SetEase(Ease.Linear);
        }).AddTo(gameObject);

        _onFadeOut.Subscribe(duration =>
        {
            contents.DOFade(0.0f, duration).SetEase(Ease.Linear).OnComplete(() =>
            {
                contents.gameObject.SetActive(false);
            });
        }).AddTo(gameObject);

        _onChangeColor.Subscribe(color =>
        {
            imageEffect.color = color;
        }).AddTo(gameObject);
    }

    private void Start()
    {
        contents.alpha = 0.0f;
        contents.gameObject.SetActive(false);
    }
}
