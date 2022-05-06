using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class RecordView : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI textMeshProRank;
    [SerializeField] private TextMeshProUGUI textMeshProTime;
    [SerializeField] private TextMeshProUGUI textMeshProPlayerName;
    [SerializeField] private TextMeshProUGUI textMeshProYou;

    public void SetParams(int rank, int time, string playerName, bool isYou)
    {
        textMeshProRank.text = rank.ToString();
        textMeshProTime.text = GenerateTime(time);
        textMeshProPlayerName.text = playerName;
        textMeshProYou.text = isYou ? "YOU" : "";
    }

    private static string GenerateTime(int time)
    {
        var minute = (time / 3000 % 100).ToString("D2");
        var second = (time / 50 % 60).ToString("D2");
        return minute + "'" + second;
    }
}
