using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UniRx;
using UnityEngine;

public class BackScreen : MonoBehaviour
{
    private static readonly Subject<float> _onFadeIn = new();
    private static readonly Subject<float> _onFadeOut = new();

    public static void FadeIn(float duration)
    {
        _onFadeIn.OnNext(duration);
    }

    public static void FadeOut(float duration)
    {
        _onFadeOut.OnNext(duration);
    }
    
    [SerializeField] private CanvasGroup contents;

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
    }

    private void Start()
    {
        contents.alpha = 0.0f;
        contents.gameObject.SetActive(false);
    }
}
