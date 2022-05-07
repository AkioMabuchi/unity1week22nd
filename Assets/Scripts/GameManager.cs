using System;
using System.Collections;
using System.Collections.Generic;
using UniRx;
using UniRx.Triggers;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    private static readonly Subject<(ActionName, object)> _onSendAction = new();
    private static readonly ReactiveProperty<GameStateName> _state = new(GameStateName.Start);

    public static void SendAction(ActionName actionName)
    {
        _onSendAction.OnNext((actionName, null));
    }

    public static void SendAction(ActionName actionName, object obj)
    {
        _onSendAction.OnNext((actionName, obj));
    }
    
    private readonly Dictionary<ActionName, Action<object>> _actions = new();
    private readonly List<Action> _actionsBeforeChangeState = new();
    private readonly List<Coroutine> _coroutines = new();

    private void Awake()
    {
        _onSendAction.Subscribe(tuple =>
        {
            var (actionName, obj) = tuple;
            if (_actions.ContainsKey(actionName))
            {
                _actions[actionName](obj);
            }
        }).AddTo(gameObject);

        this.UpdateAsObservable()
            .Where(_ => Input.GetMouseButtonUp(0))
            .Subscribe(_ =>
            {
                SendAction(ActionName.OnPointerUp);
            }).AddTo(gameObject);
    }

    private void Start()
    {
        _state.Subscribe(state =>
        {
            _actions.Clear();
            foreach (var action in _actionsBeforeChangeState)
            {
                action();
            }
            _actionsBeforeChangeState.Clear();
            switch (state)
            {
                case GameStateName.Start:
                {
                    _actions.Add(ActionName.OnFinishWait, _ =>
                    {
                        if (ES3.KeyExists("Story"))
                        {
                            _state.Value = GameStateName.TitleScreen;
                        }
                        else
                        {
                            ES3.Save("Story", Unit.Default);
                            _state.Value = GameStateName.WaitForStoryDialog;
                        }
                    });
                    Wait(0.1f);
                    break;
                }
                case GameStateName.WaitForStoryDialog:
                {
                    _actions.Add(ActionName.OnFinishWait, _ =>
                    {
                        _state.Value = GameStateName.StoryDialog;
                    });
                    Wait(1.0f);
                    break;
                }
                case GameStateName.StoryDialog:
                {
                    _actions.Add(ActionName.OnPointerDownImageButton, obj =>
                    {
                        if (obj is string buttonName)
                        {
                            switch (buttonName)
                            {
                                case "Close":
                                {
                                    _state.Value = GameStateName.TitleScreen;
                                    break;
                                }
                            }
                        }
                    });
                    _actionsBeforeChangeState.Add(() =>
                    {
                        StoryDialog.Hide();
                        BackScreen.FadeOut(0.0f);
                    });
                    StoryDialog.Show();
                    BackScreen.FadeIn(0.0f);
                    break;
                }
                case GameStateName.TitleScreen:
                {
                    _actions.Add(ActionName.OnPointerDownImageButton, obj =>
                    {
                        if (obj is string buttonName)
                        {
                            switch (buttonName)
                            {
                                case "Screen":
                                {
                                    _state.Value = GameStateName.ScrollToSelectScreenFromTitleScreen;
                                    break;
                                }
                                case "Story":
                                {
                                    _state.Value = GameStateName.StoryDialog;
                                    break;
                                }
                                case "Credits":
                                {
                                    _state.Value = GameStateName.CreditsDialog;
                                    break;
                                }
                                case "Settings":
                                {
                                    _state.Value = GameStateName.SettingsDialogAtTitleScreen;
                                    break;
                                }
                            }
                        }
                    });
                    _actionsBeforeChangeState.Add(() =>
                    {
                        BottomNavigation.HideUIsForTitleScreen();
                    });
                    BottomNavigation.ShowUIsForTitleScreen();
                    break;
                }
                case GameStateName.CreditsDialog:
                {
                    _actions.Add(ActionName.OnPointerDownImageButton, obj =>
                    {
                        if (obj is string buttonName)
                        {
                            switch (buttonName)
                            {
                                case "Close":
                                {
                                    _state.Value = GameStateName.TitleScreen;
                                    break;
                                }
                            }
                        }
                    });
                    _actionsBeforeChangeState.Add(() =>
                    {
                        BackScreen.FadeOut(0.0f);
                        CreditsDialog.Hide();
                    });
                    BackScreen.FadeIn(0.0f);
                    CreditsDialog.Show();
                    break;
                }
                case GameStateName.SettingsDialogAtTitleScreen:
                {
                    _actions.Add(ActionName.OnPointerDownImageButton, obj =>
                    {
                        if (obj is string buttonName)
                        {
                            switch (buttonName)
                            {
                                case "Close":
                                {
                                    _state.Value = GameStateName.TitleScreen;
                                    break;
                                }
                            }
                        }
                    });
                    _actionsBeforeChangeState.Add(() =>
                    {
                        BackScreen.FadeOut(0.0f);
                        SettingsDialog.Hide();
                    });
                    BackScreen.FadeIn(0.0f);
                    SettingsDialog.Show();
                    break;
                }
                case GameStateName.ScrollToSelectScreenFromTitleScreen:
                {
                    _actions.Add(ActionName.OnMainScrollScrolled, _ =>
                    {
                        _state.Value = GameStateName.SelectScreen;
                    });
                    MainScroll.Slide(-PictureManager.PicturePositions[PictureManager.SelectedPictureIndex.Value], 3.0f);
                    break;
                }
                case GameStateName.ScrollToTitleScreenFromSelectScreen:
                {
                    _actions.Add(ActionName.OnMainScrollScrolled, _ =>
                    {
                        _state.Value = GameStateName.TitleScreen;
                    });
                    MainScroll.Slide(0.0f, 3.0f);
                    break;
                }
                case GameStateName.SelectScreen:
                {
                    _actions.Add(ActionName.OnPointerDownImageButton, obj =>
                    {
                        if (obj is string buttonName)
                        {
                            switch (buttonName)
                            {
                                case "PrevPicture":
                                {
                                    PictureManager.HideImageButtons();
                                    PictureManager.PrevPicture();
                                    _state.Value = GameStateName.ScrollAtSelectScreen;
                                    break;
                                }
                                case "NextPicture":
                                {
                                    PictureManager.HideImageButtons();
                                    PictureManager.NextPicture();
                                    _state.Value = GameStateName.ScrollAtSelectScreen;
                                    break;
                                }
                                case "PictureStart":
                                {
                                    if (ES3.KeyExists("PictureSave(" + PictureManager.CurrentPicture.name + ")"))
                                    {
                                        PictureManager.HideImageButtons();
                                        _state.Value = GameStateName.ConfirmLoadPicture;
                                    }
                                    else
                                    {
                                        PictureManager.ClearCurrentPicture();
                                        PictureManager.HideImageButtons();
                                        MainTimer.SetCount(0);
                                        _state.Value = GameStateName.MoveToMainScreen;
                                    }
                                    break;
                                }
                                case "Title":
                                {
                                    PictureManager.HideImageButtons();
                                    _state.Value = GameStateName.ScrollToTitleScreenFromSelectScreen;
                                    break;
                                }
                                case "Records":
                                {
                                    PictureManager.HideImageButtons();
                                    _state.Value = GameStateName.RecordsDialogAtSelectScreen;
                                    break;
                                }
                                case "Door":
                                {
                                    if (PictureManager.IsDoorOpen.Value)
                                    {
                                        
                                    }
                                    break;
                                }
                            }
                        }
                    });
                    _actionsBeforeChangeState.Add(()=>
                    {
                        BottomNavigation.HideUIsForSelectScreen();
                    });
                    BottomNavigation.ShowUIsForSelectScreen();
                    BottomNavigation.SetClearTime(PictureManager.CurrentPictureClearTime);
                    PictureManager.ShowImageButtons();
                    break;
                }
                case GameStateName.ScrollAtSelectScreen:
                {
                    _actions.Add(ActionName.OnMainScrollScrolled, _ =>
                    {
                        _state.Value = GameStateName.SelectScreen;
                    });
                    MainScroll.Slide(-PictureManager.PicturePositions[PictureManager.SelectedPictureIndex.Value], 1.0f);
                    break;
                }
                case GameStateName.RecordsDialogAtSelectScreen:
                {
                    _actions.Add(ActionName.OnPointerDownImageButton, obj =>
                    {
                        if (obj is string buttonName)
                        {
                            switch (buttonName)
                            {
                                case "Close":
                                {
                                    _state.Value = GameStateName.SelectScreen;
                                    break;
                                }
                            }
                        }
                    });
                    _actions.Add(ActionName.OnReceiveRecordsSucceeded, obj =>
                    {
                        if (obj is Record[] records)
                        {
                            RecordsDialog.UpdateRecords(records);
                        }
                    });
                    _actions.Add(ActionName.OnReceiveRecordsFailed, _ =>
                    {
                        Debug.Log("データの受信に失敗");
                    });
                    _actionsBeforeChangeState.Add(() =>
                    {
                        BackScreen.FadeOut(0.0f);
                        RecordsDialog.Hide();
                    });
                    BackScreen.FadeIn(0.0f);
                    RecordsDialog.ClearRecords();
                    RecordsDialog.Show();
                    RecordManager.ReceiveRecords(PictureManager.CurrentPicture.name);
                    break;
                }
                case GameStateName.ConfirmLoadPicture:
                {
                    _actions.Add(ActionName.OnPointerDownImageButton, obj =>
                    {
                        var saveKey = "PictureSave(" + PictureManager.CurrentPicture.name + ")";
                        if (obj is string buttonName)
                        {
                            switch (buttonName)
                            {
                                case "Reset":
                                {
                                    ES3.DeleteKey(saveKey);
                                    PictureManager.ClearCurrentPicture();
                                    PictureManager.HideImageButtons();
                                    MainTimer.SetCount(0);
                                    _state.Value = GameStateName.MoveToMainScreen;
                                    break;
                                }
                                case "Load":
                                {
                                    var saveData = ES3.Load<PicturePieceSaveInfo>(saveKey);
                                    PictureManager.DrawCurrentPictureBySaveData(saveData);
                                    PictureManager.HideImageButtons();
                                    MainTimer.SetCount(saveData.time);
                                    _state.Value = GameStateName.MoveToMainScreen;
                                    break;
                                }
                                case "Cancel":
                                {
                                    _state.Value = GameStateName.SelectScreen;
                                    break;
                                }
                            }
                        }
                    });
                    _actionsBeforeChangeState.Add(() =>
                    {
                        BackScreen.FadeOut(0.0f);
                        PictureLoadDialog.Hide();
                    });
                    BackScreen.FadeIn(0.0f);
                    PictureLoadDialog.Show();
                    break;
                }
                case GameStateName.MoveToMainScreen:
                {
                    _actions.Add(ActionName.OnFinishWait, _ =>
                    {
                        _state.Value = GameStateName.ReadyToMainScreen;
                    });
                    PictureManager.Escape();
                    PictureManager.HideDetails();
                    Wait(2.0f);
                    break;
                }
                case GameStateName.ReadyToMainScreen:
                {
                    _actions.Add(ActionName.OnFinishWait, _ =>
                    {
                        PieceManager.ClearPieces();
                        var key = "PictureSave(" + PictureManager.CurrentPicture.name + ")";
                        if (ES3.KeyExists(key))
                        {
                            var saveData = ES3.Load<PicturePieceSaveInfo>(key);
                            PieceManager.LoadPieces(PictureManager.CurrentPicture, saveData);
                            PictureManager.ClearCurrentPicture();

                        }
                        else
                        {
                            PieceManager.GeneratePieces(PictureManager.CurrentPicture);
                            PictureManager.ClearCurrentPicture();
     
                        }
                        _state.Value = GameStateName.MainScreen;
                    });
                    TopNavigation.SetTitle(PictureManager.CurrentPicture.title);
                    TopNavigation.ShowUpContents(1.0f);
                    BottomNavigation.ShowUpContentsHigh(1.0f);
                    PictureManager.FocusSelectedPicture(1.0f);
                    Wait(1.2f);
                    break;
                }
                case GameStateName.MainScreen:
                {
                    _actions.Add(ActionName.OnPointerUp, _ =>
                    {
                        PieceManager.OnPointerUp();
                    });
                    _actions.Add(ActionName.OnPointerDownImageButton, obj =>
                    {
                        if (obj is string buttonName)
                        {
                            switch (buttonName)
                            {
                                case "Return":
                                {
                                    PieceManager.Save(PictureManager.CurrentPicture.name);
                                    PieceManager.ClearPieces();
                                    var saveKey = "PictureSave(" + PictureManager.CurrentPicture.name + ")";
                                    var saveData = ES3.Load<PicturePieceSaveInfo>(saveKey);
                                    PictureManager.DrawCurrentPictureBySaveData(saveData);
                                    _state.Value = GameStateName.ReturnToSelectScreenFromMainScreen1;
                                    break;
                                }
                                case "Records":
                                {
                                    _state.Value = GameStateName.RecordsDialogAtMainScreen;
                                    break;
                                }
                                case "Settings":
                                {
                                    _state.Value = GameStateName.SettingsDialogAtMainScreen;
                                    break;
                                }
                            }
                        }
                    });
                    _actions.Add(ActionName.OnPointerDownPiece, obj =>
                    {
                        if (obj is int index)
                        {
                            PieceManager.OnPointerDownPiece(index);
                        }
                    });
                    _actions.Add(ActionName.OnPuzzleFinish, _ =>
                    {
                        var saveKey = "PictureSave(" + PictureManager.CurrentPicture.name + ")";
                        var clearTimeKey = "PictureClearTime(" + PictureManager.CurrentPicture.name + ")";
                        ES3.DeleteKey(saveKey);
                        var clearTime = ES3.Load(clearTimeKey, int.MaxValue);
                        ES3.Save(clearTimeKey, Math.Min(clearTime, MainTimer.Count.Value));
                        PieceManager.ClearPieces();
                        PictureManager.DrawCurrentPicture();
                        PictureManager.SetClearTime(MainTimer.Count.Value);
                        _state.Value = GameStateName.MainScreenPuzzleFinish;
                    });
                    _actionsBeforeChangeState.Add(() =>
                    {
                        MainTimer.SetActive(false);
                        BottomNavigation.HideUIsForPieceScroll();
                    });
                    MainTimer.SetActive(true);
                    BottomNavigation.ShowUIsForPieceScroll();
                    break;
                }
                case GameStateName.SettingsDialogAtMainScreen:
                {
                    _actions.Add(ActionName.OnPointerDownImageButton, obj =>
                    {
                        if (obj is string buttonName)
                        {
                            switch (buttonName)
                            {
                                case "Close":
                                {
                                    _state.Value = GameStateName.MainScreen;
                                    break;
                                }
                            }
                        }
                    });
                    _actionsBeforeChangeState.Add(() =>
                    {
                        BackScreen.FadeOut(0.0f);
                        SettingsDialog.Hide();
                    });
                    BackScreen.FadeIn(0.0f);
                    SettingsDialog.Show();
                    break;
                }
                case GameStateName.RecordsDialogAtMainScreen:
                {
                    _actions.Add(ActionName.OnPointerDownImageButton, obj =>
                    {
                        if (obj is string buttonName)
                        {
                            switch (buttonName)
                            {
                                case "Close":
                                {
                                    _state.Value = GameStateName.MainScreen;
                                    break;
                                }
                            }
                        }
                    });
                    _actions.Add(ActionName.OnReceiveRecordsSucceeded, obj =>
                    {
                        if (obj is Record[] records)
                        {
                            RecordsDialog.UpdateRecords(records);
                        }
                    });
                    _actions.Add(ActionName.OnReceiveRecordsFailed, _ =>
                    {
                        Debug.Log("データの受信に失敗");
                    });
                    _actionsBeforeChangeState.Add(() =>
                    {
                        BackScreen.FadeOut(0.0f);
                        RecordsDialog.Hide();
                    });
                    BackScreen.FadeIn(0.0f);
                    RecordsDialog.ClearRecords();
                    RecordsDialog.Show();
                    RecordManager.ReceiveRecords(PictureManager.CurrentPicture.name);
                    break;
                }
                case GameStateName.MainScreenPuzzleFinish:
                {
                    _actions.Add(ActionName.OnFinishWait, _ =>
                    {
                        _state.Value = GameStateName.WaitForFadeInBackScreenForFinish;
                    });
                    Wait(1.0f);
                    break;
                }
                case GameStateName.WaitForFadeInBackScreenForFinish:
                {
                    _actions.Add(ActionName.OnFinishWait, _ =>
                    {
                        _state.Value = GameStateName.ShowDownFinishExpression;
                    });
                    BackScreen.FadeIn(0.5f);
                    Wait(1.0f);
                    break;
                }
                case GameStateName.ShowDownFinishExpression:
                {
                    _actions.Add(ActionName.OnFinishWait, _ =>
                    {
                        _state.Value = GameStateName.WaitFinishExpression;
                    });
                    FinishExpression.SetPicture(PictureManager.CurrentPicture);
                    FinishExpression.ShowDown(2.0f);
                    Wait(2.5f);
                    break;
                }
                case GameStateName.WaitFinishExpression:
                {
                    _actions.Add(ActionName.OnFinishWait, _ =>
                    {
                        _state.Value = GameStateName.HideUpFinishExpression;
                    });
                    Wait(3.0f);
                    break;
                }
                case GameStateName.HideUpFinishExpression:
                {
                    _actions.Add(ActionName.OnFinishWait, _ =>
                    {
                        _state.Value = GameStateName.TweetDialogAtFinishPuzzle;
                    });
                    FinishExpression.HideUp(2.0f);
                    Wait(4.0f);
                    break;
                }
                case GameStateName.TweetDialogAtFinishPuzzle:
                {
                    _actions.Add(ActionName.OnPointerDownImageButton, obj =>
                    {
                        if (obj is string buttonName)
                        {
                            switch (buttonName)
                            {
                                case "Tweet":
                                {
                                    TwitterModel.Tweet("クリアしたよ！");
                                    _state.Value = GameStateName.WaitForSendableRecordsDialog;
                                    break;
                                }
                                case "Close":
                                {
                                    _state.Value = GameStateName.WaitForSendableRecordsDialog;
                                    break;
                                }
                            }
                        }
                    });
                    _actionsBeforeChangeState.Add(() =>
                    {
                        TweetDialog.Hide();
                    });
                    TweetDialog.Show();
                    break;
                }
                case GameStateName.WaitForSendableRecordsDialog:
                {
                    _actions.Add(ActionName.OnFinishWait, _ =>
                    {
                        _state.Value = GameStateName.SendableRecordsDialog;
                    });
                    Wait(1.0f);
                    break;
                }
                case GameStateName.SendableRecordsDialog:
                {
                    _actions.Add(ActionName.OnPointerDownImageButton, obj =>
                    {
                        if (obj is string buttonName)
                        {
                            switch (buttonName)
                            {
                                case "SendRecord":
                                {
                                    SendableRecordsDialog.ClearRecords();
                                    RecordManager.SendRecord(PlayerModel.Id.Value, PlayerModel.Name.Value,
                                        PictureManager.CurrentPicture.name, MainTimer.Count.Value);
                                    break;
                                }
                                case "Close":
                                {
                                    if (PictureManager.IsComplete && !PictureManager.IsDoorOpen.Value)
                                    {
                                        PictureManager.SetDoorOpen(true);
                                        _state.Value = GameStateName.WaitForOpenTheDoorDialog;
                                    }
                                    else
                                    {
                                        _state.Value = GameStateName.FadeOutDialogForSelectScreen;
                                    }
                                    break;
                                }
                            }
                        }
                    });
                    _actions.Add(ActionName.OnSendRecordSucceeded, _ =>
                    {
                        RecordManager.ReceiveRecords(PictureManager.CurrentPicture.name);
                    });
                    _actions.Add(ActionName.OnSendRecordFailed, _ =>
                    {
                        Debug.Log("データ送信失敗");
                    });
                    _actions.Add(ActionName.OnReceiveRecordsSucceeded, obj =>
                    {
                        if (obj is Record[] records)
                        {
                            SendableRecordsDialog.UpdateRecords(records);
                        }
                    });
                    _actions.Add(ActionName.OnReceiveRecordsFailed, _ =>
                    {
                        Debug.Log("データ受信失敗");
                    });
                    
                    _actionsBeforeChangeState.Add(() =>
                    {
                        SendableRecordsDialog.Hide();
                    });
                    SendableRecordsDialog.Show();
                    RecordManager.ReceiveRecords(PictureManager.CurrentPicture.name);
                    break;
                }
                case GameStateName.WaitForOpenTheDoorDialog:
                {
                    _actions.Add(ActionName.OnFinishWait, _ =>
                    {
                        _state.Value = GameStateName.OpenTheDoorDialog;
                    });
                    Wait(1.0f);
                    break;
                }
                case GameStateName.OpenTheDoorDialog:
                {
                    _actions.Add(ActionName.OnPointerDownImageButton, obj =>
                    {
                        if (obj is string buttonName)
                        {
                            switch (buttonName)
                            {
                                case "Close":
                                {
                                    _state.Value = GameStateName.FadeOutDialogForSelectScreen;
                                    break;
                                }
                            }
                        }
                    });
                    _actionsBeforeChangeState.Add(() =>
                    {
                        DoorOpenDialog.Hide();
                    });
                    DoorOpenDialog.Show();
                    break;
                }
                case GameStateName.FadeOutDialogForSelectScreen:
                {
                    _actions.Add(ActionName.OnFinishWait, _ =>
                    {
                        _state.Value = GameStateName.ReturnToSelectScreenFromMainScreen1;
                    });
                    BackScreen.FadeOut(0.5f);
                    Wait(0.6f);
                    break;
                }
                case GameStateName.ReturnToSelectScreenFromMainScreen1:
                {
                    _actions.Add(ActionName.OnFinishWait, _ =>
                    {
                        _state.Value = GameStateName.ReturnToSelectScreenFromMainScreen2;
                    });
                    PictureManager.ReturnFocusSelectedPicture(2.0f);
                    TopNavigation.HideDownContents(2.0f);
                    BottomNavigation.ShowUpContentsMiddle(2.0f);
                    Wait(2.0f);
                    break;
                }
                case GameStateName.ReturnToSelectScreenFromMainScreen2:
                {
                    _actions.Add(ActionName.OnFinishWait, _ =>
                    {
                        var clearTimeKey = "PictureClearTime(" + PictureManager.CurrentPicture.name + ")";
                        if (ES3.KeyExists(clearTimeKey))
                        {
                            PictureManager.DrawCurrentPicture();
                        }
                        _state.Value = GameStateName.SelectScreen;
                    });
                    _actionsBeforeChangeState.Add(() =>
                    {
                        PictureManager.ShowDetails();
                    });
                    PictureManager.ReturnEscape();
                    Wait(2.0f);
                    break;
                }
            }
        }).AddTo(gameObject);
    }

    private void Wait(float duration)
    {
        _coroutines.Add(StartCoroutine(CoroutineWait(duration)));
    }

    private void StopWait()
    {
        foreach (var coroutine in _coroutines)
        {
            StopCoroutine(coroutine);
        }
    }

    private IEnumerator CoroutineWait(float duration)
    {
        yield return new WaitForSeconds(duration);
        SendAction(ActionName.OnFinishWait);
    }
}
