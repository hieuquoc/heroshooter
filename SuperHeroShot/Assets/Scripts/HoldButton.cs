using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;

public class HoldButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    public UnityEvent onDown = new UnityEvent();
    public UnityEvent onUp = new UnityEvent();

    public void OnPointerDown(PointerEventData eventData)
    {
        onDown.Invoke();
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        onUp.Invoke();
    }
}
