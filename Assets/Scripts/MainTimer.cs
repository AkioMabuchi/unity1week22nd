using System;
using System.Collections;
using System.Collections.Generic;
using UniRx;
using UniRx.Triggers;
using UnityEngine;

public class MainTimer : MonoBehaviour
{
    private static readonly ReactiveProperty<int> _count = new(0);
    public static IReadOnlyReactiveProperty<int> Count => _count;
    private static readonly ReactiveProperty<bool> _isActive = new(false);

    public static void SetCount(int count)
    {
        _count.Value = count;
    }

    public static void SetActive(bool active)
    {
        _isActive.Value = active;
    }

    private void Awake()
    {
        this.FixedUpdateAsObservable()
            .Where(_ => _isActive.Value)
            .Where(_ => _count.Value < 299999)
            .Subscribe(_ =>
            {
                _count.Value++;
            }).AddTo(gameObject);
    }
}
