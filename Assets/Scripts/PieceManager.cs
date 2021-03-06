using System;
using System.Collections;
using System.Collections.Generic;
using UniRx;
using UniRx.Triggers;
using UnityEngine;
using UnityEngine.UI;

[Serializable]
public class PieceStatus
{
    public Vector2Int basePosition;
    public int ownedStatus;
    public List<int> neighbours;
    public bool canPut;

    public bool CanPut(Vector2Int position, int putRange)
    {
        return canPut && basePosition.x - putRange <= position.x && position.x < basePosition.x + putRange &&
               basePosition.y - putRange <= position.y && position.y < basePosition.y + putRange;
    }
}



public class PieceManager : MonoBehaviour
{
    private const int PictureOffsetX = 0;
    private const int PictureOffsetY = 8;
    
    private static readonly Subject<PictureInfo> _onGeneratePieces = new();
    private static readonly Subject<(PictureInfo picture, PicturePieceSaveInfo saveData)> _onLoadPieces = new();
    private static readonly Subject<int> _onPointerDownPiece = new();
    private static readonly Subject<Unit> _onPointerUp = new();
    private static readonly Subject<string> _onSave = new();
    private static readonly Subject<Unit> _onClearPieces = new();

    private static readonly ReactiveProperty<int> _pieceNum = new(0);
    public static IReadOnlyReactiveProperty<int> PieceNum => _pieceNum;
    private static readonly ReactiveProperty<int> _putPieceNum = new(0);
    public static IReadOnlyReactiveProperty<int> PutPieceNum => _putPieceNum;

    public static void GeneratePieces(PictureInfo picture)
    {
        _onGeneratePieces.OnNext(picture);
    }

    public static void LoadPieces(PictureInfo picture, PicturePieceSaveInfo saveData)
    {
        _onLoadPieces.OnNext((picture, saveData));
    }
    public static void OnPointerDownPiece(int index)
    {
        _onPointerDownPiece.OnNext(index);
    }

    public static void OnPointerUp()
    {
        _onPointerUp.OnNext(Unit.Default);
    }
    
    public static void Save(string pictureName)
    {
        _onSave.OnNext(pictureName);
    }

    public static void ClearPieces()
    {
        _onClearPieces.OnNext(Unit.Default);
    }

    public static void SetPieceNum(int num)
    {
        _pieceNum.Value = num;
    }

    public static void SetPutPieceNum(int num)
    {
        _putPieceNum.Value = num;
    }
    
    [SerializeField] private GameObject prefabPiece;
    [SerializeField] private Transform transformPanelPieces;
    [SerializeField] private Transform transformIndependentPieces;
    [SerializeField] private Transform transformSelectedPieces;
    [SerializeField] private ScrollRect scroll;
    
    private readonly List<Piece> _pieces = new();
    private int _selectedPieceIndex = -1;
    
    private int _maxSizeOfPieceDeck;
    
    private int _maxPieceSizeX;
    private int _maxPieceSizeY;
    private int _pieceSizeX;
    private int _pieceSizeY;
    private int _putRange;
    private readonly List<int> _pieceMap = new();
    private readonly List<Color> _colorMap = new();
    private readonly List<PieceStatus> _pieceStatus = new();
    
    private void Awake()
    {
        _onGeneratePieces.Subscribe(picture =>
        {
            var amount = picture.sizeX * picture.sizeY;
            for (var i = 0; i < amount; i++)
            {
                _pieceStatus.Add(new PieceStatus());
            }
            
            _pieceSizeX = picture.texture.width / picture.sizeX;
            _pieceSizeY = picture.texture.height / picture.sizeY;
            _maxPieceSizeX = _pieceSizeX >= 24 ? _pieceSizeX + 10 :
                _pieceSizeX >= 18 ? _pieceSizeX + 8 :
                _pieceSizeX >= 12 ? _pieceSizeX + 6 : _pieceSizeX + 4;
            _maxPieceSizeY = _pieceSizeY >= 24 ? _pieceSizeY + 10 :
                _pieceSizeY >= 18 ? _pieceSizeY + 8 :
                _pieceSizeY >= 12 ? _pieceSizeY + 6 : _pieceSizeY + 4;
            _putRange = _pieceSizeX >= 20 ? 5 : 4;
            var pixelAmount = picture.texture.width * picture.texture.height;

            _pieceMap.Clear();
            _colorMap.Clear();
            for (var i = 0; i < pixelAmount; i++)
            {
                var mapX = i % picture.texture.width;
                var mapY = i / picture.texture.width;
                _colorMap.Add(picture.texture.GetPixel(mapX, mapY));
                var pieceX = mapX / _pieceSizeX;
                var pieceY = mapY / _pieceSizeY;
                var pieceIndex = pieceY * picture.sizeX + pieceX;
                _pieceMap.Add(pieceIndex);
            }

            var pieceTextures = new Texture2D[amount];
            for (var i = 0; i < amount; i++)
            {
                pieceTextures[i] = new Texture2D(_maxPieceSizeX, _maxPieceSizeY, TextureFormat.RGBA32, false)
                {
                    filterMode = FilterMode.Point
                };
                for (var x = 0; x < _maxPieceSizeX; x++)
                {
                    for (var y = 0; y < _maxPieceSizeY; y++)
                    {
                        pieceTextures[i].SetPixel(x, y, Color.clear);
                    }
                }
            }

            for (var x = 1; x < picture.sizeX; x++)
            {
                for (var y = 0; y < picture.sizeY; y++)
                {
                    var index = x + y * picture.sizeX;
                    var neighbourIndex = index - 1;
                    var dig = (x + y) % 2 == 0;
                    if (UnityEngine.Random.Range(0, 20) == 0)
                    {
                        dig = !dig;
                    }

                    var notchX = _pieceSizeX >= 24 ? 5 : _pieceSizeX >= 18 ? 4 : _pieceSizeX >= 12 ? 3 : 2;
                    var notchY = _pieceSizeY >= 24 ? 6 : _pieceSizeY >= 12 ? 4 : 2;
                    for (var nx = 0; nx < notchX; nx++)
                    {
                        for (var ny = 0; ny < notchY; ny++)
                        {
                            var ni = dig ? index : neighbourIndex;
                            var mapX = dig ? x * _pieceSizeX - nx - 1 : x * _pieceSizeX + nx;
                            var mapY = y * _pieceSizeY + (_pieceSizeY - notchY) / 2 + ny;
                            var mapIndex = mapX + mapY * picture.texture.width;
                            _pieceMap[mapIndex] = ni;
                        }
                    }
                }
            }
            
            for (var x = 0; x < picture.sizeX; x++)
            {
                for (var y = 1; y < picture.sizeY; y++)
                {
                    var index = x + y * picture.sizeX;
                    var neighbourIndex = index - picture.sizeX;
                    var dig = (x + y) % 2 == 1;
                    if (UnityEngine.Random.Range(0, 20) == 0)
                    {
                        dig = !dig;
                    }

                    var notchX = _pieceSizeX >= 24 ? 6 : _pieceSizeX >= 12 ? 4 : 2;
                    var notchY = _pieceSizeY >= 24 ? 5 : _pieceSizeY >= 18 ? 4 : _pieceSizeY >= 12 ? 3 : 2;
                    for (var nx = 0; nx < notchX; nx++)
                    {
                        for (var ny = 0; ny < notchY; ny++)
                        {
                            var ni = dig ? index : neighbourIndex;
                            var mapX = x * _pieceSizeX + (_pieceSizeX - notchX) / 2 + nx;
                            var mapY = dig ? y * _pieceSizeY - ny - 1 : y * _pieceSizeY + ny;
                            var mapIndex = mapX + mapY * picture.texture.width;
                            _pieceMap[mapIndex] = ni;
                        }
                    }
                }
            }

            for (var i = 0; i < pixelAmount; i++)
            {
                var mapX = i % picture.texture.width;
                var mapY = i / picture.texture.width;
                var index = _pieceMap[i];
                var indexX = index % picture.sizeX;
                var indexY = index / picture.sizeX;
                var baseX = (_pieceSizeX - _maxPieceSizeX) / 2 + indexX * _pieceSizeX;
                var baseY = (_pieceSizeY - _maxPieceSizeY) / 2 + indexY * _pieceSizeY;
                var texX = mapX - baseX;
                var texY = mapY - baseY;
                pieceTextures[index].SetPixel(texX, texY, _colorMap[i]);
            }
            
            for (var i = 0; i < amount; i++)
            {
                _pieceStatus[i].neighbours = new List<int>();
                var x = i % picture.sizeX;
                var y = i / picture.sizeX;
                var posX = x * _pieceSizeX - (picture.texture.width - _pieceSizeX) / 2 + PictureOffsetX;
                var posY = y * _pieceSizeY - (picture.texture.height - _pieceSizeY) / 2 + PictureOffsetY;
                if (x > 0)
                {
                    _pieceStatus[i].neighbours.Add(i - 1);
                }

                if (x < picture.sizeX - 1)
                {
                    _pieceStatus[i].neighbours.Add(i + 1);
                }

                if (y > 0)
                {
                    _pieceStatus[i].neighbours.Add(i - picture.sizeX);
                }

                if (y < picture.sizeY - 1)
                {
                    _pieceStatus[i].neighbours.Add(i + picture.sizeX);
                }

                _pieceStatus[i].canPut = _pieceStatus[i].neighbours.Count == 2;
                _pieceStatus[i].basePosition = new Vector2Int(posX, posY);
                _pieceStatus[i].ownedStatus = 0;
            }

            
            var positionXs = new int[amount];
            for (var i = 0; i < amount; i++)
            {
                positionXs[i] = i * (_maxPieceSizeX + 2) + (_maxPieceSizeX + 2) / 2;
            }

            var xi = amount - 1;
            while (xi > 0)
            {
                var i = UnityEngine.Random.Range(0, xi);
                (positionXs[i], positionXs[xi]) = (positionXs[xi], positionXs[i]);
                xi--;
            }

            _maxSizeOfPieceDeck = (_maxPieceSizeX + 2) * amount;

            var sizeDelta = scroll.content.sizeDelta;
            sizeDelta.x = _maxSizeOfPieceDeck;
            scroll.content.sizeDelta = sizeDelta;
            
            for (var i = 0; i < amount; i++)
            {
                _pieces.Add(Instantiate(prefabPiece, scroll.content).GetComponent<Piece>());
                _pieces[i].SetIndex(i);
                _pieces[i].SetControllable(true);
                _pieces[i].SetTexture(pieceTextures[i]);
                _pieces[i].SetPosition(new Vector2Int(positionXs[i], -15));
            }
        }).AddTo(gameObject);

        _onLoadPieces.Subscribe(tuple =>
        {
            var (picture, saveData) = tuple;

            _pieceMap.Clear();
            _colorMap.Clear();

            _pieceSizeX = saveData.pieceSizeX;
            _pieceSizeY = saveData.pieceSizeY;
            _maxPieceSizeX = saveData.maxPieceSizeX;
            _maxPieceSizeY = saveData.maxPieceSizeY;
            _putRange = saveData.putRange;
            foreach (var pieceMapPixel in saveData.pieceMap)
            {
                _pieceMap.Add(pieceMapPixel);
            }

            for (var i = 0; i < picture.texture.width * picture.texture.height; i++)
            {
                var mapX = i % picture.texture.width;
                var mapY = i / picture.texture.width;
                _colorMap.Add(picture.texture.GetPixel(mapX, mapY));
            }

            _pieceStatus.Clear();
            _pieces.Clear();

            var pieceTextures = new Texture2D[saveData.pieces.Length];
            for (var i = 0; i < pieceTextures.Length; i++)
            {
                pieceTextures[i] = new Texture2D(_maxPieceSizeX, _maxPieceSizeY, TextureFormat.RGBA32, false)
                {
                    filterMode = FilterMode.Point
                };
                for (var x = 0; x < _maxPieceSizeX; x++)
                {
                    for (var y = 0; y < _maxPieceSizeY; y++)
                    {
                        pieceTextures[i].SetPixel(x, y, Color.clear);
                    }
                }
            }
            
            for (var i = 0; i < _pieceMap.Count; i++)
            {
                var mapX = i % picture.texture.width;
                var mapY = i / picture.texture.width;
                var index = _pieceMap[i];
                var indexX = index % picture.sizeX;
                var indexY = index / picture.sizeX;
                var baseX = (_pieceSizeX - _maxPieceSizeX) / 2 + indexX * _pieceSizeX;
                var baseY = (_pieceSizeY - _maxPieceSizeY) / 2 + indexY * _pieceSizeY;
                var texX = mapX - baseX;
                var texY = mapY - baseY;
                pieceTextures[index].SetPixel(texX, texY, _colorMap[i]);
            }

            _maxSizeOfPieceDeck = (_maxPieceSizeX + 2) * saveData.pieces.Length;

            var sizeDelta = scroll.content.sizeDelta;
            sizeDelta.x = _maxSizeOfPieceDeck;
            scroll.content.sizeDelta = sizeDelta;

            for (var i = 0; i < saveData.pieces.Length; i++)
            {
                var piece = saveData.pieces[i];
                _pieceStatus.Add(new PieceStatus());
                _pieceStatus[i].ownedStatus = piece.belongCode;
                _pieceStatus[i].neighbours = new List<int>();
                _pieceStatus[i].canPut = piece.canPut;
                foreach (var neighbour in piece.neighbours)
                {
                    _pieceStatus[i].neighbours.Add(neighbour);
                }

                _pieceStatus[i].basePosition = new Vector2Int(piece.basePositionX, piece.basePositionY);
                _pieces.Add(Instantiate(prefabPiece).GetComponent<Piece>());
                _pieces[i].SetIndex(i);
                
                
                switch (piece.belongCode)
                {
                    case 0: // ???????????????
                    {
                        _pieces[i].SetControllable(true);
                        _pieces[i].transform.SetParent(scroll.content);
                        break;
                    }
                    case 1: // ?????????
                    {
                        _pieces[i].SetControllable(true);
                        _pieces[i].transform.SetParent(transformIndependentPieces);
                        break;
                    }
                    case 2: // ??????????????????
                    {
                        _pieces[i].SetControllable(false);
                        _pieces[i].transform.SetParent(transformPanelPieces);
                        break;
                    }
                }

                _pieces[i].SetPosition(new Vector2Int(piece.positionX, piece.positionY - 1));
                _pieces[i].SetTexture(pieceTextures[i]);
                _pieces[i].transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);
            }
        }).AddTo(gameObject);
        
        _onPointerDownPiece.Where(_ => _selectedPieceIndex < 0).Subscribe(index =>
        {
            _selectedPieceIndex = index;
            _pieces[_selectedPieceIndex].transform.SetParent(transformSelectedPieces);
            _pieces[_selectedPieceIndex].SetPosition(ClampVector2Int(CanvasMain.MousePosition.Value));
        }).AddTo(gameObject);
        

        _onSave.Subscribe(pictureName =>
        {
            var pieceSaveInfo = new PieceSaveInfo[_pieces.Count];
            for (var i = 0; i < _pieces.Count; i++)
            {
                pieceSaveInfo[i] = new PieceSaveInfo
                {
                    belongCode = _pieceStatus[i].ownedStatus,
                    positionX = _pieces[i].Position.x,
                    positionY = _pieces[i].Position.y,
                    basePositionX = _pieceStatus[i].basePosition.x,
                    basePositionY = _pieceStatus[i].basePosition.y,
                    neighbours = _pieceStatus[i].neighbours.ToArray(),
                    canPut = _pieceStatus[i].canPut
                };
            }

            var saveData = new PicturePieceSaveInfo
            {
                time = MainTimer.Count.Value,
                pieceSizeX = _pieceSizeX,
                pieceSizeY = _pieceSizeY,
                maxPieceSizeX = _maxPieceSizeX,
                maxPieceSizeY = _maxPieceSizeY,
                putRange = _putRange,
                pieceMap = _pieceMap.ToArray(),
                pieces = pieceSaveInfo
            };
            ES3.Save("PictureSave(" + pictureName + ")", saveData);
        }).AddTo(gameObject);
        
        _onClearPieces.Subscribe(_ =>
        {
            foreach (var piece in _pieces)
            {
                Destroy(piece.gameObject);
            }

            _pieces.Clear();
            _pieceStatus.Clear();
        }).AddTo(gameObject);
        
        CanvasMain.MousePosition.Where(_ => _selectedPieceIndex >= 0).Subscribe(position =>
        {
            _pieces[_selectedPieceIndex].SetPosition(ClampVector2Int(position));
        }).AddTo(gameObject);


        _onPointerUp.Subscribe(_ =>
        {
            if (_selectedPieceIndex >= 0)
            {
                if (_pieceStatus[_selectedPieceIndex].CanPut(CanvasMain.MousePosition.Value, _putRange))
                {
                    _pieces[_selectedPieceIndex].SetPosition(_pieceStatus[_selectedPieceIndex].basePosition);
                    _pieces[_selectedPieceIndex].transform.SetParent(transformPanelPieces);
                    _pieces[_selectedPieceIndex].SetControllable(false);
                    foreach (var neighbour in _pieceStatus[_selectedPieceIndex].neighbours)
                    {
                        _pieceStatus[neighbour].canPut = true;
                    }

                    _pieceStatus[_selectedPieceIndex].ownedStatus = 2; // 2 = ??????????????????????????????
                    SoundPlayer.PlaySound("PutPiece");
                    _putPieceNum.Value++;
                    var isFinish = true;
                    foreach (var pieceStatus in _pieceStatus)
                    {
                        if (pieceStatus.ownedStatus != 2)
                        {
                            isFinish = false;
                        }
                    }

                    if (isFinish)
                    {
                        GameManager.SendAction(ActionName.OnPuzzleFinish);
                    }
                }
                else
                {
                    _pieceStatus[_selectedPieceIndex].ownedStatus = 1;
                    _pieces[_selectedPieceIndex].transform.SetParent(transformIndependentPieces);
                }
            }

            _selectedPieceIndex = -1;
        }).AddTo(gameObject);
    }
    
    private static Vector2Int ClampVector2Int(Vector2Int vector2Int)
    {
        var r = vector2Int;
        r.x = Math.Clamp(r.x, -160, 160);
        r.y = Math.Clamp(r.y, -90, 90);
        return r;
    }
}
