using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

public class FinishExpression : MonoBehaviour
{
    private static readonly Subject<PictureInfo> _onSetPicture = new();
    private static readonly Subject<float> _onShowDown = new();
    private static readonly Subject<float> _onHideUp = new();

    public static void SetPicture(PictureInfo picture)
    {
        _onSetPicture.OnNext(picture);
    }

    public static void ShowDown(float duration)
    {
        _onShowDown.OnNext(duration);
    }

    public static void HideUp(float duration)
    {
        _onHideUp.OnNext(duration);
    }
    
    [SerializeField] private Transform transformScrollable;
    [SerializeField] private Image imageFrame;
    [SerializeField] private RawImage rawImagePicture;
    [SerializeField] private TextMeshProUGUI textMeshProPieceNumber;
    [SerializeField] private TextMeshProUGUI textMeshProTitle;

    private void Awake()
    {
        _onSetPicture.Subscribe(picture =>
        {
            var sizeX = picture.texture.width;
            var sizeY = picture.texture.height;
            imageFrame.rectTransform.sizeDelta = new Vector2(sizeX + 6, sizeY + 6);
            rawImagePicture.rectTransform.sizeDelta = new Vector2(sizeX, sizeY);
            rawImagePicture.texture = picture.texture;
            textMeshProPieceNumber.text = (picture.sizeX * picture.sizeY).ToString();
            textMeshProTitle.text = picture.title;
        }).AddTo(gameObject);
        _onShowDown.Subscribe(duration =>
        {
            transformScrollable.DOLocalMoveY(0.0f, duration, true).SetEase(Ease.OutBack);
        }).AddTo(gameObject);

        _onHideUp.Subscribe(duration =>
        {
            transformScrollable.DOLocalMoveY(180.0f, duration, true).SetEase(Ease.InBack);
        }).AddTo(gameObject);
    }
}
