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
    private bool _isControllable;

    public Vector2Int Position => new((int) transform.localPosition.x, (int) transform.localPosition.y);

    private void Awake()
    {
        eventTrigger.OnPointerDownAsObservable().Where(_ => _isControllable).Subscribe(_ =>
        {
            GameManager.SendAction(ActionName.OnPointerDownPiece, _index);
        }).AddTo(gameObject);
    }

    public void SetIndex(int index)
    {
        _index = index;
    }

    public void SetControllable(bool controllable)
    {
        _isControllable = controllable;
    }
    public void SetTexture(Texture2D texture2D)
    {
        texture2D.Apply();
        rawImage.texture = texture2D;
        rawImage.SetNativeSize();
    }

    public void SetPosition(Vector2Int position)
    {
        var t = transform;
        var localPosition = t.localPosition;
        localPosition.x = position.x;
        localPosition.y = position.y + 1;
        t.localPosition = localPosition;
    }
}
