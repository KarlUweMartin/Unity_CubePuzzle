using DG.Tweening;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class HoldButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{

    private void Awake()
    {
        _rect = GetComponent<RectTransform>();
        _selectable.interactable = false;
    }

    private void StartHold()
    {
        CancelHold();

        var startSize = _processIndicator.sizeDelta;
        var targetSize = new Vector2(_rect.sizeDelta.x, startSize.y);

        _holdTween = _processIndicator.DOSizeDelta(targetSize, HoldDuration)
            .SetEase(Ease.Linear)
            .OnComplete(() =>
            {
                TriggerSelectable();
            });
    }

    private void CancelHold()
    {
        if (_holdTween != null && _holdTween.IsActive())
        {
            _holdTween.Kill();
        }

        var resetSize = new Vector2(0, _processIndicator.sizeDelta.y);
        _processIndicator.sizeDelta = resetSize;
    }

    private void TriggerSelectable()
    {
        var button = _selectable as Button;
        if (button != null)
        {
            button.onClick.Invoke();
            return;
        }

        var toggle = _selectable as Toggle;
        if (toggle != null)
        {
            toggle.isOn = !toggle.isOn;
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        StartHold();
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        CancelHold();
    }

    public float HoldDuration = 1f;

    private RectTransform _rect;
    [SerializeField] private RectTransform _processIndicator;
    [SerializeField] private Selectable _selectable;

    private Tween _holdTween;
}
