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
                        _state.Value = GameStateName.TitleScreen;
                    });
                    Wait(0.1f);
                    break;
                }
                case GameStateName.WaitForStoryDialog:
                {
                    _actions.Add(ActionName.OnFinishWait, _ =>
                    {
                        SoundPlayer.PlaySound("PopUp");
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
                                    SoundPlayer.PlaySound("Close");
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
                                    SoundPlayer.PlaySound("EnterRoom");
                                    _state.Value = GameStateName.ScrollToSelectScreenFromTitleScreen;
                                    break;
                                }
                                case "Story":
                                {
                                    // SoundPlayer.PlaySound("PopUp");
                                    // _state.Value = GameStateName.StoryDialog;
                                    break;
                                }
                                case "Credits":
                                {
                                    SoundPlayer.PlaySound("PopUp");
                                    _state.Value = GameStateName.CreditsDialog;
                                    break;
                                }
                                case "Settings":
                                {
                                    SoundPlayer.PlaySound("PopUp");
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
                                    SoundPlayer.PlaySound("Close");
                                    _state.Value = GameStateName.TitleScreen;
                                    break;
                                }
                                case "Cancel":
                                {
                                    SoundPlayer.PlaySound("Cancel");
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
                                    SoundPlayer.PlaySound("Close");
                                    _state.Value = GameStateName.TitleScreen;
                                    break;
                                }
                                case "Cancel":
                                {
                                    SoundPlayer.PlaySound("Cancel");
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
                    MainScroll.Slide(-PictureManager.PicturePositions[PictureManager.SelectedPictureIndex.Value], 2.0f);
                    break;
                }
                case GameStateName.ScrollToTitleScreenFromSelectScreen:
                {
                    _actions.Add(ActionName.OnMainScrollScrolled, _ =>
                    {
                        _state.Value = GameStateName.TitleScreen;
                    });
                    MainScroll.Slide(0.0f, 2.0f);
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
                                        SoundPlayer.PlaySound("Click");
                                        PictureManager.HideImageButtons();
                                        _state.Value = GameStateName.ConfirmLoadPicture;
                                    }
                                    else
                                    {
                                        SoundPlayer.PlaySound("Decide");
                                        PictureManager.ClearCurrentPicture();
                                        PictureManager.HideImageButtons();
                                        PieceManager.SetPieceNum(PictureManager.CurrentPicture.sizeX *
                                                                 PictureManager.CurrentPicture.sizeY);
                                        PieceManager.SetPutPieceNum(0);
                                        MainTimer.SetCount(0);
                                        _state.Value = GameStateName.MoveToMainScreen;
                                    }
                                    break;
                                }
                                case "Title":
                                {
                                    SoundPlayer.PlaySound("EnterRoom");
                                    PictureManager.HideImageButtons();
                                    _state.Value = GameStateName.ScrollToTitleScreenFromSelectScreen;
                                    break;
                                }
                                case "Records":
                                {
                                    SoundPlayer.PlaySound("PopUp");
                                    PictureManager.HideImageButtons();
                                    _state.Value = GameStateName.RecordsDialogAtSelectScreen;
                                    break;
                                }
                                case "Door":
                                {
                                    if (PictureManager.IsDoorOpen.Value)
                                    {
                                        PictureManager.HideImageButtons();
                                        _state.Value = GameStateName.LeaveFromSelectScreen;
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
                    SoundPlayer.PlaySound("MovePicture");
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
                                    SoundPlayer.PlaySound("Close");
                                    _state.Value = GameStateName.SelectScreen;
                                    break;
                                }
                                case "Cancel":
                                {
                                    SoundPlayer.PlaySound("Cancel");
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
                            var pieceNum = PictureManager.CurrentPicture.sizeX * PictureManager.CurrentPicture.sizeY;
                            switch (buttonName)
                            {
                                case "Reset":
                                {
                                    SoundPlayer.PlaySound("Decide");
                                    ES3.DeleteKey(saveKey);
                                    PictureManager.ClearCurrentPicture();
                                    PictureManager.HideImageButtons();
                                    PieceManager.SetPieceNum(pieceNum);
                                    PieceManager.SetPutPieceNum(0);
                                    MainTimer.SetCount(0);
                                    _state.Value = GameStateName.MoveToMainScreen;
                                    break;
                                }
                                case "Load":
                                {
                                    SoundPlayer.PlaySound("Decide");
                                    var saveData = ES3.Load<PicturePieceSaveInfo>(saveKey);
                                    PictureManager.DrawCurrentPictureBySaveData(saveData);
                                    PictureManager.HideImageButtons();
                                    PieceManager.SetPieceNum(pieceNum);
                                    PieceManager.SetPutPieceNum(saveData.PutPieceNum);
                                    MainTimer.SetCount(saveData.time);
                                    _state.Value = GameStateName.MoveToMainScreen;
                                    break;
                                }
                                case "Cancel":
                                {
                                    SoundPlayer.PlaySound("Cancel");
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
                                    SoundPlayer.PlaySound("Click");
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
                                    SoundPlayer.PlaySound("PopUp");
                                    _state.Value = GameStateName.RecordsDialogAtMainScreen;
                                    break;
                                }
                                case "Settings":
                                {
                                    SoundPlayer.PlaySound("PopUp");
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
                                    SoundPlayer.PlaySound("Close");
                                    _state.Value = GameStateName.MainScreen;
                                    break;
                                }
                                case "Cancel":
                                {
                                    SoundPlayer.PlaySound("Cancel");
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
                                    SoundPlayer.PlaySound("Close");
                                    _state.Value = GameStateName.MainScreen;
                                    break;
                                }
                                case "Cancel":
                                {
                                    SoundPlayer.PlaySound("Cancel");
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
                        _state.Value = GameStateName.PuzzleFinishEffect;
                    });
                    Wait(1.0f);
                    break;
                }
                case GameStateName.PuzzleFinishEffect:
                {
                    _actions.Add(ActionName.OnFinishWait, _ =>
                    {
                        _state.Value = GameStateName.WaitForFadeInBackScreenForFinish;
                    });
                    SoundPlayer.PlaySound("PuzzleClear");
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
                    FinishExpression.HideUp(1.5f);
                    Wait(3.0f);
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
                                    var minute = (MainTimer.Count.Value / 3000 % 100).ToString("D2");
                                    var second = (MainTimer.Count.Value / 50 % 60).ToString("D2");
                                    var time = minute + "'" + second;
                                    TwitterModel.Tweet("ピクセルピース美術館にて『" + PictureManager.CurrentPicture.title + "』を「" +
                                                       time +
                                                       "」で復元したよ！\n\n#unity1week #unityroom #ピクセルアート美術館 #ピクセルピース美術館\nhttps://unityroom.com/games/pixelpiecemuseum");
                                    _state.Value = GameStateName.WaitForSendableRecordsDialog;
                                    break;
                                }
                                case "Close":
                                {
                                    SoundPlayer.PlaySound("Click");
                                    _state.Value = GameStateName.WaitForSendableRecordsDialog;
                                    break;
                                }
                                case "Cancel":
                                {
                                    SoundPlayer.PlaySound("Cancel");
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
                    SoundPlayer.PlaySound("PopUp");
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
                                    SoundPlayer.PlaySound("Click");
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
                                case "Cancel":
                                {
                                    SoundPlayer.PlaySound("Cancel");
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
                        SendableRecordsDialog.ClearRecords();
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
                    SoundPlayer.PlaySound("PopUp");
                    SendableRecordsDialog.Show();
                    SendableRecordsDialog.ClearRecords();
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
                                    SoundPlayer.PlaySound("Click");
                                    _state.Value = GameStateName.FadeOutDialogForSelectScreen;
                                    break;
                                }
                                case "Cancel":
                                {
                                    SoundPlayer.PlaySound("Cancel");
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
                    SoundPlayer.PlaySound("DoorOpen");
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
                case GameStateName.LeaveFromSelectScreen:
                {
                    _actions.Add(ActionName.OnFinishWait, _ =>
                    {
                        _state.Value = GameStateName.EnterToCompleteScreen;
                    });
                    SoundPlayer.PlaySound("EnterRoom");
                    ColorEffect.ChangeColor(Color.white);
                    ColorEffect.FadeIn(2.0f);
                    Wait(3.0f);
                    break;
                }
                case GameStateName.EnterToCompleteScreen:
                {
                    _actions.Add(ActionName.OnFinishWait, _ =>
                    {
                        _state.Value = GameStateName.CompleteScreen;
                    });
                    MainScreen.HideMainScroll();
                    MainScreen.ShowCompleteScreen();
                    ColorEffect.FadeOut(1.5f);
                    Wait(2.0f);
                    break;
                }
                case GameStateName.CompleteScreen:
                {
                    _actions.Add(ActionName.OnPointerDownImageButton, obj =>
                    {
                        if (obj is string buttonName)
                        {
                            switch (buttonName)
                            {
                                case "Tweet":
                                {
                                    var time = PictureManager.CompleteTime;
                                    var hour = (time / 180000 % 100).ToString("D2");
                                    var minute = (time / 3000 % 60).ToString("D2");
                                    var second = (time / 50 % 60).ToString("D2");
                                    var totalTime = hour + ":" + minute + "'" + second;
                                    TwitterModel.Tweet("ピクセルピース美術館にて、すべてのアートを「" +
                                                       totalTime +
                                                       "」で復元したよ！！\n\n#unity1week #unityroom #ピクセルアート美術館 #ピクセルピース美術館\nhttps://unityroom.com/games/pixelpiecemuseum");
                                    break;
                                }
                                case "Records":
                                {
                                    SoundPlayer.PlaySound("PopUp");
                                    _state.Value = GameStateName.CompleteRecordsDialog;
                                    break;
                                }
                                case "Leave":
                                {
                                    _state.Value = GameStateName.LeaveFromCompleteScreen;
                                    break;
                                }
                            }
                        }
                    });
                    _actionsBeforeChangeState.Add(() =>
                    {
                        BottomNavigation.HideUIsForCompleteScreen();
                    });
                    BottomNavigation.SetCompleteTime(PictureManager.CompleteTime);
                    BottomNavigation.ShowUIsForCompleteScreen();
                    break;
                }
                case GameStateName.LeaveFromCompleteScreen:
                {
                    _actions.Add(ActionName.OnFinishWait, _ =>
                    {
                        _state.Value = GameStateName.EnterToSelectScreen;
                    });
                    SoundPlayer.PlaySound("EnterRoom");
                    ColorEffect.ChangeColor(Color.black);
                    ColorEffect.FadeIn(2.0f);
                    Wait(3.0f);
                    break;
                }
                case GameStateName.EnterToSelectScreen:
                {
                    _actions.Add(ActionName.OnFinishWait,_=>
                    {
                        _state.Value = GameStateName.SelectScreen;
                    });
                    MainScreen.HideCompleteScreen();
                    MainScreen.ShowMainScroll();
                    ColorEffect.FadeOut(1.5f);
                    Wait(2.0f);
                    break;
                }
                case GameStateName.CompleteRecordsDialog:
                {
                    _actions.Add(ActionName.OnPointerDownImageButton, obj =>
                    {
                        if (obj is string buttonName)
                        {
                            switch (buttonName)
                            {
                                case "SendRecord":
                                {
                                    CompleteRecordsDialog.ClearRecords();
                                    RecordManager.SendRecord(PlayerModel.Id.Value, PlayerModel.Name.Value,
                                        "complete", PictureManager.CompleteTime);
                                    break;
                                }
                                case "Close":
                                {
                                    SoundPlayer.PlaySound("Close");
                                    _state.Value = GameStateName.CompleteScreen;
                                    break;
                                }
                                case "Cancel":
                                {
                                    SoundPlayer.PlaySound("Cancel");
                                    _state.Value = GameStateName.CompleteScreen;
                                    break;
                                }
                            }
                        }
                    });
                    _actions.Add(ActionName.OnSendRecordSucceeded, _ =>
                    {
                        CompleteRecordsDialog.ClearRecords();
                        RecordManager.ReceiveRecords("complete");
                    });
                    _actions.Add(ActionName.OnSendRecordFailed, _ =>
                    {
                        Debug.Log("データ送信失敗");
                    });
                    _actions.Add(ActionName.OnReceiveRecordsSucceeded, obj =>
                    {
                        if (obj is Record[] records)
                        {
                            CompleteRecordsDialog.UpdateRecords(records);
                        }
                    });
                    _actions.Add(ActionName.OnReceiveRecordsFailed, _ =>
                    {
                        Debug.Log("データ受信失敗");
                    });
                    
                    _actionsBeforeChangeState.Add(() =>
                    {
                        BackScreen.FadeOut(0.0f);
                        CompleteRecordsDialog.Hide();
                    });
                    BackScreen.FadeIn(0.0f);
                    CompleteRecordsDialog.SetCompleteTime(PictureManager.CompleteTime);
                    CompleteRecordsDialog.ClearRecords();
                    CompleteRecordsDialog.Show();
                    RecordManager.ReceiveRecords("complete");
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
