using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UniRx;
using UnityEngine;

public class TopNavigation : MonoBehaviour
{
    private static readonly Subject<float> _onShowUpContents = new();
    private static readonly Subject<float> _onHideDownContents = new();

    public static void ShowUpContents(float duration)
    {
        _onShowUpContents.OnNext(duration);
    }

    public static void HideDownContents(float duration)
    {
        _onHideDownContents.OnNext(duration);
    }

    [SerializeField] private GameObject contents;

    private void Awake()
    {
        _onShowUpContents.Subscribe(duration =>
        {
            contents.transform.DOLocalMoveY(80.0f, duration, true).SetEase(Ease.Linear);
        }).AddTo(gameObject);

        _onHideDownContents.Subscribe(duration =>
        {
            contents.transform.DOLocalMoveY(100.0f, duration, true).SetEase(Ease.Linear);
        }).AddTo(gameObject);
    }

    private void Start()
    {
        contents.transform.localPosition = new Vector3(0.0f, 100.0f, 0.0f);
    }
}
