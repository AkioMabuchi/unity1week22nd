using System.Collections;
using System.Collections.Generic;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

public class CompleteRecordsDialog : MonoBehaviour
{
    private static readonly Subject<Unit> _onShow = new();
    private static readonly Subject<Unit> _onHide = new();
    private static readonly Subject<Record[]> _onUpdateRecords = new();
    private static readonly Subject<Unit> _onClearRecords = new();

    private static readonly ReactiveProperty<int> _completeTime = new(int.MaxValue);
    public static void Show()
    {
        _onShow.OnNext(Unit.Default);
    }

    public static void Hide()
    {
        _onHide.OnNext(Unit.Default);
    }

    public static void UpdateRecords(Record[] records)
    {
        _onUpdateRecords.OnNext(records);
    }

    public static void ClearRecords()
    {
        _onClearRecords.OnNext(Unit.Default);
    }

    public static void SetCompleteTime(int completeTime)
    {
        _completeTime.Value = completeTime;
    }

    [SerializeField] private GameObject prefabRecordView;

    [SerializeField] private CanvasGroup contents;
    [SerializeField] private TMP_InputField inputFieldPlayerName;
    [SerializeField] private ScrollRect scrollRecords;
    [SerializeField] private TextMeshProUGUI textMeshProTime;

    private readonly List<RecordView> _recordViews = new();

    private void Awake()
    {
        _completeTime.Subscribe(time =>
        {
            var hour = (time / 180000 % 100).ToString("D2");
            var minute = (time / 3000 % 60).ToString("D2");
            var second = (time / 50 % 60).ToString("D2");
            textMeshProTime.text = hour + ":" + minute + "'" + second;
        }).AddTo(gameObject);
        
        PlayerModel.Name.Subscribe(playerName =>
        {
            inputFieldPlayerName.text = playerName;
        }).AddTo(gameObject);

        _onShow.Subscribe(_ =>
        {
            contents.gameObject.SetActive(true);
        }).AddTo(gameObject);

        _onHide.Subscribe(_ =>
        {
            contents.gameObject.SetActive(false);
        }).AddTo(gameObject);

        _onUpdateRecords.Subscribe(records =>
        {
            var sizeDelta = scrollRecords.content.sizeDelta;
            sizeDelta.y = records.Length * 18.0f;
            scrollRecords.content.sizeDelta = sizeDelta;

            for (var i = 0; i < records.Length; i++)
            {
                var recordView = Instantiate(prefabRecordView, scrollRecords.content.transform)
                    .GetComponent<RecordView>();
                var positionY = i * -18.0f - 9.0f;
                recordView.transform.localPosition = new Vector3(81.0f, positionY, 0.0f);
                recordView.SetParamsComplete(i + 1, records[i].solveTime, records[i].playerName,
                    records[i].playerId == PlayerModel.Id.Value);
                _recordViews.Add(recordView);
            }
        }).AddTo(gameObject);

        _onClearRecords.Subscribe(_ =>
        {
            foreach (var recordView in _recordViews)
            {
                Destroy(recordView.gameObject);
            }
            _recordViews.Clear();
        }).AddTo(gameObject);
        
        inputFieldPlayerName.onDeselect.AddListener(playerName =>
        {
            PlayerModel.ChangeName(playerName);
        });
    }

    private void Start()
    {
        contents.gameObject.SetActive(false);
    }
}
