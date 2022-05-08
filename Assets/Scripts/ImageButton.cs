using System;
using System.Collections;
using System.Collections.Generic;
using UniRx;
using UniRx.Triggers;
using UnityEngine;
using UnityEngine.EventSystems;

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
        eventTrigger.OnPointerClickAsObservable().Subscribe(_ =>
        {
            if (EventSystem.current.IsPointerOverGameObject())
            {
                GameManager.SendAction(ActionName.OnPointerDownImageButton, buttonName);
            }
        }).AddTo(gameObject);
    }
}
