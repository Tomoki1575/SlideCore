using UnityEngine;
using UnityEngine.EventSystems;
using System;

public class ButtonPointerNotifier : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    // 他クラスが購読できるイベント
    public event Action OnPointerDownEvent;
    public event Action OnPointerUpEvent;

    public void OnPointerDown(PointerEventData eventData)
    {
        OnPointerDownEvent?.Invoke();
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        OnPointerUpEvent?.Invoke();
    }
}