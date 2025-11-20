using UnityEngine;
using UnityEngine.EventSystems;

public class MobileJoystick : MonoBehaviour, IDragHandler, IPointerUpHandler, IPointerDownHandler
{
    public RectTransform background;
    public RectTransform handle;

    Vector2 inputVector;

    public void OnDrag(PointerEventData eventData)
    {
        Vector2 pos;
        RectTransformUtility.ScreenPointToLocalPointInRectangle
            (background, eventData.position, eventData.pressEventCamera, out pos);

        pos /= background.sizeDelta / 2f;
        inputVector = new Vector2(pos.x, pos.y);
        inputVector = (inputVector.magnitude > 1) ? inputVector.normalized : inputVector;

        handle.anchoredPosition = 
            new Vector2(inputVector.x * (background.sizeDelta.x / 2.5f),
                        inputVector.y * (background.sizeDelta.y / 2.5f));
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        OnDrag(eventData);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        inputVector = Vector2.zero;
        handle.anchoredPosition = Vector2.zero;
    }

    public float Horizontal() => inputVector.x;
    public float Vertical() => inputVector.y;
}

