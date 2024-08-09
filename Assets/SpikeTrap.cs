using UnityEngine;

public class SpikeTrap : MonoBehaviour
{
    public float riseHeight = 2f; // B��aklar�n yukar� ��kaca�� mesafe
    public float riseSpeed = 2f; // B��aklar�n yukar� ��kma h�z�
    public float retractSpeed = 1f; // B��aklar�n geri �ekilme h�z�
    public float delayTimeUp = 1f; // B��aklar�n yukar� ��kt�ktan sonra bekleyece�i s�re
    public float delayTimeDown = 1f; // B��aklar�n yere indikten sonra bekleyece�i s�re

    private Vector3 startPos;
    private bool isRising = true;
    private bool isWaiting = false;

    void Start()
    {
        startPos = transform.position; // Ba�lang�� pozisyonunu kaydet
    }

    void Update()
    {
        if (isRising)
        {
            // B��aklar� yukar� ��kar
            transform.position += Vector3.up * riseSpeed * Time.deltaTime;
            if (transform.position.y >= startPos.y + riseHeight)
            {
                isRising = false;
                isWaiting = true;
                Invoke("StartRetracting", delayTimeUp); // Yukar� ��kt�ktan sonra bekle
            }
        }
        else if (!isRising && !isWaiting)
        {
            // B��aklar� geri �ek
            transform.position -= Vector3.up * retractSpeed * Time.deltaTime;
            if (transform.position.y <= startPos.y)
            {
                transform.position = startPos; // Pozisyonu d�zelt
                isWaiting = true;
                Invoke("StartRising", delayTimeDown); // Yere indikten sonra bekle
            }
        }
    }

    private void StartRetracting()
    {
        isWaiting = false; // Bekleme durumunu bitir, geri �ekilmeye ba�la
    }

    private void StartRising()
    {
        isWaiting = false; // Bekleme durumunu bitir, tekrar yukar� ��kmaya ba�la
        isRising = true;
    }
}
