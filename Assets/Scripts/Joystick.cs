using UnityEngine;
using UnityEngine.EventSystems;

public class Joystick : MonoBehaviour, IDragHandler, IPointerUpHandler, IPointerDownHandler
{
    private RectTransform background;
    private RectTransform handle;
    private Vector2 inputVector;
    private bool isActive = true; // Joystick'in aktif olup olmadýðýný kontrol etmek için eklenen deðiþken

    private void Start()
    {
        background = GetComponent<RectTransform>();
        handle = transform.GetChild(0).GetComponent<RectTransform>();
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!isActive) return; // Joystick aktif deðilse hareket etmeye izin verme

        Vector2 pos;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(background, eventData.position, eventData.pressEventCamera, out pos);
        pos.x = (pos.x / background.sizeDelta.x);
        pos.y = (pos.y / background.sizeDelta.y);

        inputVector = new Vector2(pos.x * 2, pos.y * 2);
        inputVector = (inputVector.magnitude > 1.0f) ? inputVector.normalized : inputVector;

        handle.anchoredPosition = new Vector2(inputVector.x * (background.sizeDelta.x / 3), inputVector.y * (background.sizeDelta.y / 3));
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        OnDrag(eventData);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        ResetHandlePosition();
    }

    public float Horizontal()
    {
        return inputVector.x;
    }

    public float Vertical()
    {
        return inputVector.y;
    }

    public void ResetHandlePosition()
    {
        inputVector = Vector2.zero;
        handle.anchoredPosition = Vector2.zero;
    }

    public void SetActive(bool active) // Joystick'in aktif olup olmadýðýný ayarlayan method
    {
        isActive = active;
    }
}
