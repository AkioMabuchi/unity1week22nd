using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UniRx;
using UnityEngine;

public class MainScroll : MonoBehaviour
{
    private static readonly Subject<(float positionX, float duration)> _onSlide = new();

    public static void Slide(float positionX, float duration)
    {
        _onSlide.OnNext((positionX, duration));
    }

    [SerializeField] private Transform transformContents;
    private void Awake()
    {
        _onSlide.Subscribe(tuple =>
        {
            var (positionX, duration) = tuple;
            transformContents.DOLocalMoveX(positionX, duration, true).SetEase(Ease.InOutQuad).OnComplete(() =>
            {
                GameManager.SendAction(ActionName.OnMainScrollScrolled);
            });
        }).AddTo(gameObject);
    }

    private void Start()
    {
        transformContents.localPosition = new Vector3(0.0f, 0.0f, 0.0f);
    }
}
