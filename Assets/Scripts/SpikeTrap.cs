using UnityEngine;

public class SpikeTrap : MonoBehaviour
{
    public float riseHeight = 2f; // Býçaklarýn yukarý çýkacaðý mesafe
    public float riseSpeed = 2f; // Býçaklarýn yukarý çýkma hýzý
    public float retractSpeed = 1f; // Býçaklarýn geri çekilme hýzý
    public float delayTimeUp = 1f; // Býçaklarýn yukarý çýktýktan sonra bekleyeceði süre
    public float delayTimeDown = 1f; // Býçaklarýn yere indikten sonra bekleyeceði süre

    private Vector3 startPos;
    private bool isRising = true;
    private bool isWaiting = false;

    void Start()
    {
        startPos = transform.position; // Baþlangýç pozisyonunu kaydet
    }

    void Update()
    {
        if (isRising)
        {
            // Býçaklarý yukarý çýkar
            transform.position += Vector3.up * riseSpeed * Time.deltaTime;
            if (transform.position.y >= startPos.y + riseHeight)
            {
                isRising = false;
                isWaiting = true;
                Invoke("StartRetracting", delayTimeUp); // Yukarý çýktýktan sonra bekle
            }
        }
        else if (!isRising && !isWaiting)
        {
            // Býçaklarý geri çek
            transform.position -= Vector3.up * retractSpeed * Time.deltaTime;
            if (transform.position.y <= startPos.y)
            {
                transform.position = startPos; // Pozisyonu düzelt
                isWaiting = true;
                Invoke("StartRising", delayTimeDown); // Yere indikten sonra bekle
            }
        }
    }

    private void StartRetracting()
    {
        isWaiting = false; // Bekleme durumunu bitir, geri çekilmeye baþla
    }

    private void StartRising()
    {
        isWaiting = false; // Bekleme durumunu bitir, tekrar yukarý çýkmaya baþla
        isRising = true;
    }
}
