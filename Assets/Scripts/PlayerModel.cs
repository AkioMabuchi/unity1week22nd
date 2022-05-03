using System.Collections;
using System.Collections.Generic;
using UniRx;
using UnityEngine;

public static class PlayerModel
{
    private const string IdKey = "PlayerId";
    private const string NameKey = "PlayerName";
    
    private static readonly ReactiveProperty<string> _id = new();
    public static IReadOnlyReactiveProperty<string> Id => _id;
    private static readonly ReactiveProperty<string> _name = new();
    public static IReadOnlyReactiveProperty<string> Name => _name;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Initialize()
    {
        if (ES3.KeyExists(IdKey))
        {
            _id.Value = ES3.Load<string>(IdKey);
        }
        else
        {
            var characters = "0123456789abcdefghijklmnopqrstuvwxyz".ToCharArray();
            var id = "";
            for (var i = 0; i < 64; i++)
            {
                id += characters[UnityEngine.Random.Range(0, characters.Length)];
            }

            _id.Value = id;
            ES3.Save(IdKey, _id.Value);
        }

        if (ES3.KeyExists(NameKey))
        {
            _name.Value = ES3.Load<string>(NameKey);
        }
        else
        {
            var num = UnityEngine.Random.Range(0, 10000).ToString("D4");
            _name.Value = "Guest" + num;
            ES3.Save(NameKey, _name.Value);
        }
    }

    public static void ChangeName(string name)
    {
        _name.Value = name;
        ES3.Save(NameKey, _name.Value);
    }
}
