using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(CanvasScaler))]
public class CanvasMain : MonoBehaviour
{
    [SerializeField] private CanvasScaler canvasScaler;

    private void Reset()
    {
        canvasScaler = GetComponent<CanvasScaler>();
        canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        canvasScaler.referenceResolution = new Vector2(1920.0f, 1080.0f);
        canvasScaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
    }

    private void Start()
    {
        canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        canvasScaler.referenceResolution = new Vector2(1920.0f, 1080.0f);
        canvasScaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        canvasScaler.matchWidthOrHeight = Screen.width * 9 / Screen.height >= 16 ? 1.0f : 0.0f;
    }

    private void Update()
    {
        canvasScaler.matchWidthOrHeight = Screen.width * 9 / Screen.height >= 16 ? 1.0f : 0.0f;
    }
}
