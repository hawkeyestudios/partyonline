using UnityEngine;
using UnityEngine.EventSystems;

public class Joystick : MonoBehaviour, IDragHandler, IPointerUpHandler, IPointerDownHandler
{
    private RectTransform background;
    private RectTransform handle;
    private RectTransform highlight; // Highlight nesnesi
    private Vector2 inputVector;
    private bool isActive = true;

    private void Start()
    {
        background = GetComponent<RectTransform>();
        handle = transform.GetChild(1).GetComponent<RectTransform>();
        highlight = transform.GetChild(0).GetComponent<RectTransform>(); // Highlight nesnesini al
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!isActive) return;

        Vector2 pos;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(background, eventData.position, eventData.pressEventCamera, out pos);
        pos.x = (pos.x / background.sizeDelta.x);
        pos.y = (pos.y / background.sizeDelta.y);

        inputVector = new Vector2(pos.x * 2, pos.y * 2);
        inputVector = (inputVector.magnitude > 1.0f) ? inputVector.normalized : inputVector;

        handle.anchoredPosition = new Vector2(inputVector.x * (background.sizeDelta.x / 3), inputVector.y * (background.sizeDelta.y / 3));

        // Highlight'ý güncelle
        UpdateHighlight();
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
        UpdateHighlight(); // Highlight'ý sýfýrla
    }

    public void SetActive(bool active)
    {
        isActive = active;
    }

    private void UpdateHighlight()
    {
        if (inputVector == Vector2.zero)
        {
            highlight.gameObject.SetActive(false); // Joystick ortada ise highlight'ý gizle
            return;
        }

        highlight.gameObject.SetActive(true);

        // Açýyý hesapla
        float angle = Mathf.Atan2(inputVector.y, inputVector.x) * Mathf.Rad2Deg;

        // Highlight'ýn açýsýný güncelle
        highlight.localEulerAngles = new Vector3(0, 0, angle-45);
    }
}
