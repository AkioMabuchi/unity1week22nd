using System.Collections;
using System.Collections.Generic;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

public class RecordsDialog : MonoBehaviour
{
    private static readonly Subject<Unit> _onShow = new();
    private static readonly Subject<Unit> _onHide = new();
    private static readonly Subject<Record[]> _onUpdateRecords = new();
    private static readonly Subject<Unit> _onClearRecords = new();

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

    [SerializeField] private GameObject prefabRecordView;

    [SerializeField] private CanvasGroup contents;
    [SerializeField] private ScrollRect scrollRecords;

    private readonly List<RecordView> _recordViews = new();

    private void Awake()
    {
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
                recordView.SetParams(i + 1, records[i].solveTime, records[i].playerName,
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
    }

    private void Start()
    {
        contents.gameObject.SetActive(false);
    }
}
