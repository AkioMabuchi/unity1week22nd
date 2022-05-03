using System;
using System.Collections;
using System.Collections.Generic;
using UniRx;
using UniRx.Triggers;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(RectTransform))]
[RequireComponent(typeof(CanvasScaler))]
[RequireComponent(typeof(ObservableEventTrigger))]
public class CanvasMain : MonoBehaviour
{
    private static readonly ReactiveProperty<Vector2Int> _mousePosition = new(Vector2Int.zero);
    public static IReadOnlyReactiveProperty<Vector2Int> MousePosition => _mousePosition;
    private static readonly Subject<Unit> _onPointerUp = new();
    public static IObservable<Unit> OnPointerUp => _onPointerUp;

    [SerializeField] private RectTransform rectTransform;
    [SerializeField] private CanvasScaler canvasScaler;
    [SerializeField] private ObservableEventTrigger eventTrigger;
    private void Reset()
    {
        rectTransform = GetComponent<RectTransform>();
        canvasScaler = GetComponent<CanvasScaler>();
        canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        canvasScaler.referenceResolution = new Vector2(320.0f, 180.0f);
        canvasScaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        eventTrigger = GetComponent<ObservableEventTrigger>();
    }

    private void Awake()
    {
        this.UpdateAsObservable()
            .Subscribe(_ =>
            {
                var mousePosition = Input.mousePosition;
                var sizeDelta = rectTransform.sizeDelta;
                var magnification = sizeDelta.x / Screen.width;
                var mousePositionX = (int) (mousePosition.x * magnification - sizeDelta.x / 2);
                var mousePositionY = (int) (mousePosition.y * magnification - sizeDelta.y / 2);
                _mousePosition.Value = new Vector2Int(mousePositionX, mousePositionY);
            }).AddTo(gameObject);
    }

    private void Start()
    {
        canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        canvasScaler.referenceResolution = new Vector2(320.0f, 180.0f);
        canvasScaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        canvasScaler.matchWidthOrHeight = Screen.width * 9 / Screen.height >= 16 ? 1.0f : 0.0f;
    }

    private void Update()
    {
        canvasScaler.matchWidthOrHeight = Screen.width * 9 / Screen.height >= 16 ? 1.0f : 0.0f;
    }
}
