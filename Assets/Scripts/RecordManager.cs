using System;
using System.Collections;
using System.Collections.Generic;
using UniRx;
using UnityEngine;
using UnityEngine.Networking;

[Serializable]
public class Records
{
    public Record[] records;
}

[Serializable]
public class Record
{
    public string playerId;
    public string playerName;
    public string pictureName;
    public int solveTime;
}

public class RecordManager : MonoBehaviour
{
    private static readonly Subject<(string playerId, string playerName, string pictureName, int solveTime)> _onSendRecord =
        new();

    private static readonly Subject<string> _onReceiveRecords = new();

    public static void SendRecord(string playerId, string playerName, string pictureName, int solveTime)
    {
        _onSendRecord.OnNext((playerId, playerName, pictureName, solveTime));
    }

    public static void ReceiveRecords(string pictureName)
    {
        _onReceiveRecords.OnNext(pictureName);
    }
    
    [SerializeField] private string accessKey;

    private void Awake()
    {
        _onSendRecord.Subscribe(tuple =>
        {
            var (playerId, playerName, pictureId, solveTime) = tuple;
            StartCoroutine(CoroutineSendRecord(playerId, playerName, pictureId, solveTime));
        }).AddTo(gameObject);

        _onReceiveRecords.Subscribe(pictureName =>
        {
            StartCoroutine(CoroutineReceiveRecord(pictureName));
        }).AddTo(gameObject);
    }
    
    private IEnumerator CoroutineSendRecord(string playerId, string playerName, string pictureName, int solveTime)
    {
        var form = new WWWForm();
        form.AddField("player_id", playerId);
        form.AddField("player_name", playerName);
        form.AddField("picture_name", pictureName);
        form.AddField("solve_time", solveTime);

        var request = UnityWebRequest.Post("https://records.akiomabuchi.com/" + accessKey + "/send", form);

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            if (request.responseCode == 204)
            {
                GameManager.SendAction(ActionName.OnSendRecordSucceeded);
            }
            else
            {
                GameManager.SendAction(ActionName.OnSendRecordFailed);
            }
        }
        else
        {
            GameManager.SendAction(ActionName.OnSendRecordFailed);
        }
    }

    private IEnumerator CoroutineReceiveRecord(string pictureName)
    {
        var request =
            UnityWebRequest.Get("https://records.akiomabuchi.com/" + accessKey + "/receive?picture_name=" + pictureName);

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            if (request.responseCode == 200)
            {
                var records = JsonUtility.FromJson<Records>(request.downloadHandler.text);
                GameManager.SendAction(ActionName.OnReceiveRecordsSucceeded, records.records);
            }
            else
            {
                GameManager.SendAction(ActionName.OnReceiveRecordsFailed);
            }
        }
        else
        {
            GameManager.SendAction(ActionName.OnReceiveRecordsFailed);
        }
    }
}
