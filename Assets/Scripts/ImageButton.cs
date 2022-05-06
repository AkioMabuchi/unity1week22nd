using System;
using System.Collections;
using System.Collections.Generic;
using UniRx;
using UniRx.Triggers;
using UnityEngine;

[RequireComponent(typeof(ObservableEventTrigger))]
public class ImageButton : MonoBehaviour
{
    [SerializeField] private ObservableEventTrigger eventTrigger;
    [SerializeField] private string buttonName;
    private void Reset()
    {
        eventTrigger = GetComponent<ObservableEventTrigger>();
    }

    private void Awake()
    {
        eventTrigger.OnPointerDownAsObservable().Subscribe(_ =>
        {
            GameManager.SendAction(ActionName.OnPointerDownImageButton, buttonName);
        }).AddTo(gameObject);
    }
}
