using System;
using System.Collections;
using System.Collections.Generic;
using UniRx;
using UniRx.Triggers;
using UnityEngine;
using UnityEngine.UI;

public class Piece : MonoBehaviour
{
    [SerializeField] private RawImage rawImage;
    [SerializeField] private ObservableEventTrigger eventTrigger;

    private int _index;
    private void Awake()
    {
        eventTrigger.OnPointerDownAsObservable().Subscribe(_ =>
        {
            PieceManager.OnPointerDownPiece(_index);
        }).AddTo(gameObject);
    }

    public void SetIndex(int index)
    {
        _index = index;
    }
    public void SetTexture(Texture2D texture2D)
    {
        texture2D.Apply();
        rawImage.texture = texture2D;
        rawImage.SetNativeSize();
    }

    public void MovePosition(Vector2Int position)
    {
        var t = transform;
        var localPosition = t.localPosition;
        localPosition.x = position.x;
        localPosition.y = position.y;
        t.localPosition = localPosition;
    }
}
