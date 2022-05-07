using System;
using System.Collections;
using System.Collections.Generic;
using UniRx;
using UnityEngine;

public class MainScreen : MonoBehaviour
{
    private static readonly Subject<Unit> _onShowMainScroll = new();
    private static readonly Subject<Unit> _onHideMainScroll = new();
    private static readonly Subject<Unit> _onShowCompleteScreen = new();
    private static readonly Subject<Unit> _onHideCompleteScreen = new();

    public static void ShowMainScroll()
    {
        _onShowMainScroll.OnNext(Unit.Default);
    }

    public static void HideMainScroll()
    {
        _onHideMainScroll.OnNext(Unit.Default);
    }

    public static void ShowCompleteScreen()
    {
        _onShowCompleteScreen.OnNext(Unit.Default);
    }

    public static void HideCompleteScreen()
    {
        _onHideCompleteScreen.OnNext(Unit.Default);
    }

    [SerializeField] private CanvasGroup mainScroll;
    [SerializeField] private CanvasGroup completeScreen;

    private void Awake()
    {
        _onShowMainScroll.Subscribe(_ =>
        {
            mainScroll.gameObject.SetActive(true);
        }).AddTo(gameObject);

        _onHideMainScroll.Subscribe(_ =>
        {
            mainScroll.gameObject.SetActive(false);
        }).AddTo(gameObject);

        _onShowCompleteScreen.Subscribe(_ =>
        {
            completeScreen.gameObject.SetActive(true);
        }).AddTo(gameObject);

        _onHideCompleteScreen.Subscribe(_ =>
        {
            completeScreen.gameObject.SetActive(false);
        }).AddTo(gameObject);
    }

    private void Start()
    {
        mainScroll.gameObject.SetActive(true);
        completeScreen.gameObject.SetActive(false);
    }
}
