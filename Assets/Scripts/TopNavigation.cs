using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UniRx;
using UnityEngine;

public class TopNavigation : MonoBehaviour
{
    private static readonly Subject<float> _onShowUpContents = new();
    private static readonly Subject<float> _onHideDownContents = new();

    private static readonly ReactiveProperty<string> _title = new("");
    
    public static void ShowUpContents(float duration)
    {
        _onShowUpContents.OnNext(duration);
    }

    public static void HideDownContents(float duration)
    {
        _onHideDownContents.OnNext(duration);
    }

    public static void SetTitle(string title)
    {
        _title.Value = title;
    }

    [SerializeField] private GameObject contents;
    [SerializeField] private TextMeshProUGUI textMeshProTime;
    [SerializeField] private TextMeshProUGUI textMeshProTitle;
    [SerializeField] private TextMeshProUGUI textMeshProPiece;
    private void Awake()
    {
        PieceManager.PieceNum.Subscribe(pieceNum =>
        {
            textMeshProPiece.text = PieceManager.PutPieceNum.Value + "/" + pieceNum;
        }).AddTo(gameObject);

        PieceManager.PutPieceNum.Subscribe(putPieceNum =>
        {
            textMeshProPiece.text = putPieceNum + "/" + PieceManager.PieceNum.Value;
        }).AddTo(gameObject);
        
        MainTimer.Count.Subscribe(time =>
        {
            var minute = (time / 3000 % 100).ToString("D2");
            var second = (time / 50 % 60).ToString("D2");
            textMeshProTime.text = minute + "'" + second;
        }).AddTo(gameObject);

        _title.Subscribe(title =>
        {
            textMeshProTitle.text = title;
        }).AddTo(gameObject);
        
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
