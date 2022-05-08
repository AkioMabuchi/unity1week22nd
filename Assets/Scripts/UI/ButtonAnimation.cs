using UniRx;
using UniRx.Triggers;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    [RequireComponent(typeof(ObservableEventTrigger), typeof(Image))]
    public class ButtonAnimation : MonoBehaviour
    {
        [SerializeField] private ObservableEventTrigger eventTrigger;
        [SerializeField] private Image image;
        [SerializeField] private Sprite normalSprite;
        [SerializeField] private Sprite pressedSprite;

        [SerializeField] private RectTransform[] children;
    
        private void Reset()
        {
            eventTrigger = GetComponent<ObservableEventTrigger>();
            image = GetComponent<Image>();
        }

        private void Awake()
        {
            eventTrigger.OnPointerDownAsObservable().Subscribe(_ =>
            {
                image.sprite = pressedSprite;

                foreach (var child in children)
                {
                    child.anchoredPosition -= Vector2.up;
                }
                
            }).AddTo(gameObject);

            eventTrigger.OnPointerUpAsObservable().Subscribe(_ =>
            {
                image.sprite = normalSprite;
                foreach (var child in children)
                {
                    child.anchoredPosition += Vector2.up;
                }
                
            }).AddTo(gameObject);
        }
    }
}
