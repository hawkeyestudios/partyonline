using UnityEngine;

public class DoorController : MonoBehaviour
{
    public Animator doorAnimator; // Kapýyý kontrol etmek için Animator bileþeni
    public float delayBeforeOpening = 10f; // Kapýnýn açýlma gecikme süresi (10 saniye)

    private void Start()
    {
        // Belirtilen süre sonunda kapýyý aç
        Invoke("OpenDoor", delayBeforeOpening);
    }

    private void OpenDoor()
    {
        if (doorAnimator != null)
        {
            // Kapýyý açmak için animasyonu tetikle
            doorAnimator.SetTrigger("OpenDoor");
        }
        else
        {
            Debug.LogError("Door Animator is not assigned.");
        }
    }
}
