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
    }

    private void Start()
    {
        _actions.Clear();
        foreach (var action in _actionsBeforeChangeState)
        {
            action();
        }
        _actionsBeforeChangeState.Clear();
        _state.Subscribe(state =>
        {
            switch (state)
            {
                case GameStateName.Start:
                {
                    _actions.Add(ActionName.OnFinishWait, _ =>
                    {

                    });
                    Wait(0.1f);
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
