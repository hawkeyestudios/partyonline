using UnityEngine;
using Photon.Pun;

public class CameraIntroController : MonoBehaviour
{
    public Animator cameraAnimator;
    public GameObject gameUI;
    public float animationDuration = 5f;
    public CameraFollow cameraFollow;
    private PlayerMovement[] playerMovements; // PlayerMovement referanslarýný saklayacak dizi
    public GhostController ghostController; // GhostController referansý

    private float timer;
    private bool hasAnimationCompleted = false; // Animasyonun tamamlanýp tamamlanmadýðýný kontrol etmek için

    void Start()
    {
        timer = 0f;

        if (cameraAnimator != null)
        {
            cameraAnimator.SetTrigger("StartGame");
        }

        // Joystick ve Jump butonlarýný devre dýþý býrak ve Rigidbody'yi kýsýtla
        playerMovements = FindObjectsOfType<PlayerMovement>();
        foreach (PlayerMovement pm in playerMovements)
        {
            pm.DisableMovement(); // Hareketi tamamen devre dýþý býrak
        }

        // Ghost'un hareketini baþlangýçta durdur
        if (ghostController != null)
        {
            ghostController.enabled = false;
        }
    }

    void Update()
    {
        if (!hasAnimationCompleted)
        {
            if (cameraAnimator.GetCurrentAnimatorStateInfo(0).IsName("TrapPGStartAnim"))
            {
                timer += Time.deltaTime;
                if (timer >= animationDuration)
                {
                    OnAnimationComplete();
                    hasAnimationCompleted = true; // Animasyon tamamlandýðýnda bunu iþaretle
                }
            }
        }
    }

    public void OnAnimationComplete()
    {
        if (gameUI != null)
        {
            gameUI.SetActive(true);
        }

        if (cameraFollow != null)
        {
            cameraFollow.StartCameraFollow();
        }

        // Joystick ve Jump butonlarýný tekrar aktif hale getir ve hareketi etkinleþtir
        foreach (PlayerMovement pm in playerMovements)
        {
            pm.EnableMovement(); // Hareketi tekrar etkinleþtir
        }

        // Ghost hareketini baþlat
        if (ghostController != null)
        {
            ghostController.enabled = true;
        }

        StartGame();
    }

    void StartGame()
    {
        // Oyun baþlangýç iþlemlerini buraya ekleyin
    }
}
