using System;
using System.Collections;
using System.Collections.Generic;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

public class PictureManager : MonoBehaviour
{
    private static readonly Subject<Unit> _onShowImageButtons = new();
    private static readonly Subject<Unit> _onHideImageButtons = new();
    private static readonly Subject<Unit> _onEscape = new();
    private static readonly Subject<Unit> _onShowDetails = new();
    private static readonly Subject<Unit> _onHideDetails = new();
    private static readonly Subject<float> _onFocusSelectedPicture = new();
    private static readonly Subject<float> _onReturnFocusSelectedPicture = new();
    private static readonly Subject<Unit> _onReturnEscape = new();
    private static readonly Subject<Unit> _onClearCurrentPicture = new();
    private static readonly Subject<Unit> _onDrawCurrentPicture = new();
    private static readonly Subject<PicturePieceSaveInfo> _onDrawCurrentPictureBySaveData = new();

    public static void ShowImageButtons()
    {
        _onShowImageButtons.OnNext(Unit.Default);
    }

    public static void HideImageButtons()
    {
        _onHideImageButtons.OnNext(Unit.Default);
    }

    public static void Escape()
    {
        _onEscape.OnNext(Unit.Default);
    }

    public static void ShowDetails()
    {
        _onShowDetails.OnNext(Unit.Default);
    }

    public static void HideDetails()
    {
        _onHideDetails.OnNext(Unit.Default);
    }

    public static void FocusSelectedPicture(float duration)
    {
        _onFocusSelectedPicture.OnNext(duration);
    }

    public static void ReturnFocusSelectedPicture(float duration)
    {
        _onReturnFocusSelectedPicture.OnNext(duration);
    }

    public static void ReturnEscape()
    {
        _onReturnEscape.OnNext(Unit.Default);
    }

    public static void ClearCurrentPicture()
    {
        _onClearCurrentPicture.OnNext(Unit.Default);
    }

    public static void DrawCurrentPicture()
    {
        _onDrawCurrentPicture.OnNext(Unit.Default);
    }

    public static void DrawCurrentPictureBySaveData(PicturePieceSaveInfo saveData)
    {
        _onDrawCurrentPictureBySaveData.OnNext(saveData);
    }
    
    private static readonly ReactiveProperty<int> _selectedPictureIndex = new(0);
    public static IReadOnlyReactiveProperty<int> SelectedPictureIndex => _selectedPictureIndex;
    private static readonly ReactiveProperty<bool> _isDoorOpen = new(true);
    public static IReadOnlyReactiveProperty<bool> IsDoorOpen => _isDoorOpen;
    private static readonly List<int> _picturePositions = new();
    public static IReadOnlyList<int> PicturePositions => _picturePositions;
    private static readonly List<PictureInfo> _pictures = new();
    public static PictureInfo CurrentPicture => _pictures[_selectedPictureIndex.Value];

    private static readonly List<int> _clearTimes = new();
    public static int CurrentPictureClearTime => _clearTimes[_selectedPictureIndex.Value];
    public static bool IsComplete {
        get
        {
            var r = true;
            foreach (var clearTime in _clearTimes)
            {
                if (clearTime < 0)
                {
                    r = false;
                }
            }

            return r;
        }
    }

    public static int CompleteTime
    {
        get
        {
            var sum = 0;
            foreach (var time in _clearTimes)
            {
                if (time >= 0)
                {
                    sum += time;
                }
            }

            return sum;
        }
    }
    public static void NextPicture()
    {
        _selectedPictureIndex.Value++;
    }

    public static void PrevPicture()
    {
        _selectedPictureIndex.Value--;
    }

    public static void SetClearTime(int clearTime)
    {
        _clearTimes[_selectedPictureIndex.Value] = clearTime;
    }

    public static void SetDoorOpen(bool isDoorOpen)
    {
        _isDoorOpen.Value = isDoorOpen;
    }
    [SerializeField] private PictureData data;

    [SerializeField] private GameObject prefabPictureView;
    [SerializeField] private Sprite spriteDoor;
    [SerializeField] private Sprite spriteDoorOpen;
    [SerializeField] private Transform transformSelectScreen;
    [SerializeField] private Image imageDoor;

    private readonly List<PictureView> _pictureViews = new();

    private void Awake()
    {
        _isDoorOpen.Subscribe(isDoorOpen =>
        {
            imageDoor.sprite = isDoorOpen ? spriteDoorOpen : spriteDoor;
        }).AddTo(gameObject);
        
        _onShowImageButtons.Subscribe(_ =>
        {
            _pictureViews[_selectedPictureIndex.Value].ShowImageButtons();
        }).AddTo(gameObject);

        _onHideImageButtons.Subscribe(_ =>
        {
            _pictureViews[_selectedPictureIndex.Value].HideImageButtons();
        }).AddTo(gameObject);

        _onEscape.Subscribe(_ =>
        {
            for (var i = 0; i < _pictureViews.Count; i++)
            {
                if (i < _selectedPictureIndex.Value)
                {
                    _pictureViews[i].EscapeToLeft();
                }

                if (i > _selectedPictureIndex.Value)
                {
                    _pictureViews[i].EscapeToRight();
                }
            }
        }).AddTo(gameObject);

        _onShowDetails.Subscribe(_ =>
        {
            foreach (var pictureView in _pictureViews)
            {
                pictureView.ShowDetails();
            }
        }).AddTo(gameObject);

        _onHideDetails.Subscribe(_ =>
        {
            foreach (var pictureView in _pictureViews)
            {
                pictureView.HideDetails();
            }
        }).AddTo(gameObject);

        _onFocusSelectedPicture.Subscribe(duration =>
        {
            _pictureViews[_selectedPictureIndex.Value].Focus(duration);
        }).AddTo(gameObject);

        _onReturnFocusSelectedPicture.Subscribe(duration =>
        {
            _pictureViews[_selectedPictureIndex.Value].ReturnFocus(duration);
        }).AddTo(gameObject);

        _onReturnEscape.Subscribe(_ =>
        {
            for (var i = 0; i < _pictureViews.Count; i++)
            {
                if (i != _selectedPictureIndex.Value)
                {
                    _pictureViews[i].ReturnEscape();
                }
            }
        }).AddTo(gameObject);

        _onClearCurrentPicture.Subscribe(_ =>
        {
            _pictureViews[_selectedPictureIndex.Value].ClearPicture();
        }).AddTo(gameObject);

        _onDrawCurrentPicture.Subscribe(_ =>
        {
            _pictureViews[_selectedPictureIndex.Value].DrawPicture(_pictures[_selectedPictureIndex.Value]);
        }).AddTo(gameObject);
        
        _onDrawCurrentPictureBySaveData.Subscribe(saveData =>
        {
            _pictureViews[_selectedPictureIndex.Value]
                .DrawPictureBySaveData(_pictures[_selectedPictureIndex.Value], saveData);
        }).AddTo(gameObject);
    }

    private void Start()
    {
        var picturePositionX = 400;
        for (var i = 0; i < data.Pictures.Count; i++)
        {
            var picture = data.Pictures[i];
            _pictures.Add(picture);
            var pictureView = Instantiate(prefabPictureView, transformSelectScreen).GetComponent<PictureView>();
            _pictureViews.Add(pictureView);

            picturePositionX += picture.texture.width / 2 + 20;
            
            _pictureViews[i].transform.localPosition = new Vector3(picturePositionX, 0.0f, 0.0f);
            _pictureViews[i].Initialize(picture);

            _pictureViews[i].HideImageButtons();
            if (i > 0)
            {
                _pictureViews[i - 1].SetActiveImageButtonNext(true);
                _pictureViews[i].SetActiveImageButtonPrev(true);
            }

            var clearTimeKey = "PictureClearTime(" + picture.name + ")";
            if (ES3.KeyExists(clearTimeKey))
            {
                _pictureViews[i].DrawPicture(picture);
                _clearTimes.Add(ES3.Load<int>(clearTimeKey));
            }
            else
            {
                var saveKey = "PictureSave(" + picture.name + ")";
                if (ES3.KeyExists(saveKey))
                {
                    var saveData = ES3.Load<PicturePieceSaveInfo>(saveKey);
                    _pictureViews[i].DrawPictureBySaveData(picture, saveData);
                }
                else
                {
                    _pictureViews[i].ClearPicture();
                }

                _isDoorOpen.Value = false;
                _clearTimes.Add(-1);
            }

            _picturePositions.Add(picturePositionX);
            picturePositionX += picture.texture.width / 2 + 20;
        }

        picturePositionX += 80;
        imageDoor.transform.localPosition = new Vector3(picturePositionX - 50.5f, -24.0f, 0.0f);
    }
}
