using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UniRx;
using UnityEngine;

public class PictureLoadDialog : MonoBehaviour
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


    private void Awake()
    {
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
    }

    private void Start()
    {
        contents.gameObject.SetActive(false);
    }
}
