using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace rescueforce
{
    public class VirtualJoystick : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
{
    public RectTransform joystickBackground;
    public RectTransform joystickHandle;
    public float handleRange = 100f;
    public float deadzone = 0.1f;

    Vector2 input = Vector2.zero;

    Canvas canvas;
    Camera cam;

    public float Horizontal => input.x;
    public float Vertical => input.y;
    public Vector2 Direction => input;

    void Start()
    {
        if (joystickBackground == null) joystickBackground = GetComponent<RectTransform>();
        if (joystickHandle == null && joystickBackground != null && joystickBackground.childCount > 0)
            joystickHandle = joystickBackground.GetChild(0) as RectTransform;
        canvas = GetComponentInParent<Canvas>();
        cam = canvas != null ? canvas.worldCamera : null;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        OnDrag(eventData);
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (joystickBackground == null) return;
        Vector2 pos;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(joystickBackground, eventData.position, cam, out pos);
        Vector2 clamped = Vector2.ClampMagnitude(pos, handleRange);
        input = clamped / handleRange;
        if (input.magnitude < deadzone) input = Vector2.zero;
        if (joystickHandle != null)
            joystickHandle.anchoredPosition = clamped;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        input = Vector2.zero;
        if (joystickHandle != null) joystickHandle.anchoredPosition = Vector2.zero;
    }
}
}

