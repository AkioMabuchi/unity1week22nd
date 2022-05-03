using System;
using System.Collections;
using System.Collections.Generic;
using UniRx;
using UniRx.Triggers;
using UnityEngine;
using UnityEngine.UI;

public class PieceManager : MonoBehaviour
{
    private static readonly Subject<int> _onPointerDownPiece = new();

    public static void OnPointerDownPiece(int index)
    {
        _onPointerDownPiece.OnNext(index);
    }
    
    [SerializeField] private GameObject prefabPiece;
    [SerializeField] private Transform transformPanelPieces;
    [SerializeField] private Transform transformScrollablePieces;
    [SerializeField] private Transform transformIndependentPieces;
    [SerializeField] private Transform transformSelectedPieces;

    [SerializeField] private Image imageScrollBar;
    [SerializeField] private ObservableEventTrigger eventTriggerScrollBar;
    [SerializeField] private ObservableEventTrigger eventTriggerScrollButtonLeft;
    [SerializeField] private ObservableEventTrigger eventTriggerScrollButtonRight;
    
    private readonly List<Piece> _pieces = new();
    private int _selectedPieceIndex = -1;
    private bool _isPieceDeckScrolled;
    private bool _isScrollButtonLeftDown;
    private bool _isScrollButtonRightDown;
    private int _maxSizeOfPieceDeck;

    private void Awake()
    {
        _onPointerDownPiece.Where(_ => _selectedPieceIndex < 0).Subscribe(index =>
        {
            _selectedPieceIndex = index;
            _pieces[_selectedPieceIndex].transform.SetParent(transformSelectedPieces);
            _pieces[_selectedPieceIndex].MovePosition(ClampVector2Int(CanvasMain.MousePosition.Value));
        }).AddTo(gameObject);

        eventTriggerScrollBar.OnPointerDownAsObservable().Subscribe(_ =>
        {
            _isPieceDeckScrolled = true;
        }).AddTo(gameObject);

        eventTriggerScrollButtonLeft.OnPointerDownAsObservable().Subscribe(_ =>
        {
            _isScrollButtonLeftDown = true;
        }).AddTo(gameObject);

        eventTriggerScrollButtonRight.OnPointerDownAsObservable().Subscribe(_ =>
        {
            _isScrollButtonRightDown = true;
        }).AddTo(gameObject);
        
        CanvasMain.MousePosition.Where(_ => _selectedPieceIndex >= 0).Subscribe(position =>
        {
            _pieces[_selectedPieceIndex].MovePosition(ClampVector2Int(position));
        }).AddTo(gameObject);

        CanvasMain.MousePosition.Where(_ => _isPieceDeckScrolled).Subscribe(position =>
        {
            UpdateScroll(Math.Clamp(position.x, -140, 140));
        }).AddTo(gameObject);
        
        PieceModel.PieceAmount.Subscribe(amount =>
        {
            foreach (var piece in _pieces)
            {
                Destroy(piece);
            }

            _pieces.Clear();
            var offset = (PieceModel.MaxPieceSizeX.Value + 2) * (amount - 1) / 2;
            for (var i = 0; i < amount; i++)
            {
                _pieces.Add(Instantiate(prefabPiece, transformScrollablePieces).GetComponent<Piece>());
                _pieces[i].SetIndex(i);
                var positionX = i * (PieceModel.MaxPieceSizeX.Value + 2) - offset;
                Debug.Log(positionX);
                _pieces[i].MovePosition(new Vector2Int(positionX, 0));
            }

            _maxSizeOfPieceDeck = (PieceModel.MaxPieceSizeX.Value + 2) * amount + 20;
            UpdateScroll(-140);
        }).AddTo(gameObject);

        PieceModel.PieceTextures.Subscribe(textures =>
        {
            for (var i = 0; i < textures.Length; i++)
            {
                _pieces[i].SetTexture(textures[i]);
            }
        }).AddTo(gameObject);

        this.UpdateAsObservable()
            .Where(_ => Input.GetMouseButtonUp(0))
            .Where(_ => _selectedPieceIndex >= 0)
            .Subscribe(_ =>
            {
                _pieces[_selectedPieceIndex].transform.SetParent(transformIndependentPieces);
                _selectedPieceIndex = -1;
            }).AddTo(gameObject);

        this.UpdateAsObservable()
            .Where(_ => Input.GetMouseButtonUp(0))
            .Subscribe(_ =>
            {
                _isPieceDeckScrolled = false;
                _isScrollButtonLeftDown = false;
                _isScrollButtonRightDown = false;
            }).AddTo(gameObject);

        this.FixedUpdateAsObservable()
            .Where(_ => _isScrollButtonLeftDown)
            .Subscribe(_ =>
            {
                var positionX = (int) imageScrollBar.transform.localPosition.x;
                positionX = Math.Clamp(positionX - 1, -140, 140);
                UpdateScroll(positionX);
            }).AddTo(gameObject);

        this.FixedUpdateAsObservable()
            .Where(_ => _isScrollButtonRightDown)
            .Subscribe(_ =>
            {
                var positionX = (int) imageScrollBar.transform.localPosition.x;
                positionX = Math.Clamp(positionX + 1, -140, 140);
                UpdateScroll(positionX);
            }).AddTo(gameObject);
    }

    private void UpdateScroll(int positionX)
    {
        imageScrollBar.transform.localPosition = new Vector3(positionX, -88.0f, 0.0f);
        var scrollWidth = _maxSizeOfPieceDeck / 2 - 160;
        var scrollPositionX = positionX * scrollWidth / -140;
        transformScrollablePieces.transform.localPosition = new Vector3(scrollPositionX, -73.0f, 0.0f);
    }
    private static Vector2Int ClampVector2Int(Vector2Int vector2Int)
    {
        var r = vector2Int;
        r.x = Math.Clamp(r.x, -160, 160);
        r.y = Math.Clamp(r.y, -90, 90);
        return r;
    }
}
