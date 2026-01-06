using UnityEngine;
using UnityEngine.EventSystems;
using System;

public class ButtonPointerNotifier : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    // 他クラスが購読できるイベント
    public event Action OnPointerDownEvent;
    public event Action OnPointerUpEvent;

    private bool isPressed = false;

    public void OnPointerDown(PointerEventData eventData)
    {
        if (!isPressed)
        {
            isPressed = true;
            OnPointerDownEvent?.Invoke();
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (isPressed)
        {
            isPressed = false;
            OnPointerUpEvent?.Invoke();
        }
    }

    // Drag が発生しても Up を呼ばないように補正したい場合
    public void OnDrag(PointerEventData eventData)
    {
        // ScrollRect で Drag している場合に Up を呼ばせないならここで調整可能
    }
}