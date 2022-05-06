using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UniRx;
using UnityEngine;

public class BottomNavigation : MonoBehaviour
{
    private static readonly Subject<float> _onShowUpContentsMiddle = new();
    private static readonly Subject<float> _onShowUpContentsHigh = new();
    private static readonly Subject<float> _onHideDownContents = new();
    private static readonly Subject<Unit> _onShowUIsForTitleScreen = new();
    private static readonly Subject<Unit> _onHideUIsForTitleScreen = new();
    private static readonly Subject<Unit> _onShowUIsForSelectScreen = new();
    private static readonly Subject<Unit> _onHideUIsForSelectScreen = new();
    private static readonly Subject<Unit> _onShowUIsForPieceScroll = new();
    private static readonly Subject<Unit> _onHideUIsForPieceScroll = new();

    private static readonly ReactiveProperty<int> _clearTime = new(-1);

    public static void ShowUpContentsMiddle(float duration)
    {
        _onShowUpContentsMiddle.OnNext(duration);
    }

    public static void ShowUpContentsHigh(float duration)
    {
        _onShowUpContentsHigh.OnNext(duration);
    }

    public static void HideDownContents(float duration)
    {
        _onHideDownContents.OnNext(duration);
    }
    public static void ShowUIsForTitleScreen()
    {
        _onShowUIsForTitleScreen.OnNext(Unit.Default);
    }

    public static void HideUIsForTitleScreen()
    {
        _onHideUIsForTitleScreen.OnNext(Unit.Default);
    }

    public static void ShowUIsForSelectScreen()
    {
        _onShowUIsForSelectScreen.OnNext(Unit.Default);
    }

    public static void HideUIsForSelectScreen()
    {
        _onHideUIsForSelectScreen.OnNext(Unit.Default);
    }

    public static void ShowUIsForPieceScroll()
    {
        _onShowUIsForPieceScroll.OnNext(Unit.Default);
    }

    public static void HideUIsForPieceScroll()
    {
        _onHideUIsForPieceScroll.OnNext(Unit.Default);
    }

    public static void SetClearTime(int clearTime)
    {
        _clearTime.Value = clearTime;
    }
    
    [SerializeField] private GameObject contents;
    [SerializeField] private GameObject titleScreen;
    [SerializeField] private GameObject selectScreen;
    [SerializeField] private GameObject pieceScroll;
    [SerializeField] private TextMeshProUGUI textMeshProClearTime;
    private void Awake()
    {
        _onShowUpContentsMiddle.Subscribe(duration =>
        {
            contents.transform.DOLocalMoveY(-90.0f, duration, true).SetEase(Ease.Linear);
        }).AddTo(gameObject);

        _onShowUpContentsHigh.Subscribe(duration =>
        {
            contents.transform.DOLocalMoveY(-73.0f, duration, true).SetEase(Ease.Linear);
        }).AddTo(gameObject);

        _onHideDownContents.Subscribe(duration =>
        {
            contents.transform.DOLocalMoveY(-110.0f, duration, true).SetEase(Ease.Linear);
        }).AddTo(gameObject);
        
        _onShowUIsForTitleScreen.Subscribe(_ =>
        {
            titleScreen.SetActive(true);
        }).AddTo(gameObject);

        _onHideUIsForTitleScreen.Subscribe(_ =>
        {
            titleScreen.SetActive(false);
        }).AddTo(gameObject);

        _onShowUIsForSelectScreen.Subscribe(_ =>
        {
            selectScreen.SetActive(true);
        }).AddTo(gameObject);

        _onHideUIsForSelectScreen.Subscribe(_ =>
        {
            selectScreen.SetActive(false);
        }).AddTo(gameObject);

        _onShowUIsForPieceScroll.Subscribe(_ =>
        {
            pieceScroll.SetActive(true);
        }).AddTo(gameObject);

        _onHideUIsForPieceScroll.Subscribe(_ =>
        {
            pieceScroll.SetActive(false);
        }).AddTo(gameObject);

        _clearTime.Subscribe(clearTime =>
        {
            if (clearTime < 0)
            {
                textMeshProClearTime.text = "--'--";
            }
            else
            {
                var minute = (clearTime / 3000 % 100).ToString("D2");
                var second = (clearTime / 50 % 60).ToString("D2");
                textMeshProClearTime.text = minute + "'" + second;
            }
        }).AddTo(gameObject);
    }

    private void Start()
    {
        titleScreen.SetActive(false);
        selectScreen.SetActive(false);
        pieceScroll.SetActive(false);
        contents.transform.localPosition = new Vector3(0.0f, -90.0f, 0.0f);
    }
}
