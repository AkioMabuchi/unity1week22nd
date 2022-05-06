using System;
using System.Collections;
using System.Collections.Generic;
using UniRx;
using UnityEngine;

public class TweetDialog : MonoBehaviour
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

    private void Awake()
    {
        _onShow.Subscribe(_ =>
        {
            contents.gameObject.SetActive(true);
        }).AddTo(gameObject);

        _onHide.Subscribe(_ =>
        {
            contents.gameObject.SetActive(false);
        }).AddTo(gameObject);
    }

    private void Start()
    {
        contents.gameObject.SetActive(false);
    }
}
