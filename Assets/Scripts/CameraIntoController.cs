using UnityEngine;
using Photon.Pun;

public class CameraIntroController : MonoBehaviour
{
    public Animator cameraAnimator;
    public GameObject gameUI;
    public float animationDuration = 5f;
    public CameraFollow cameraFollow;
    private PlayerMovement[] playerMovements; // PlayerMovement referanslarýný saklayacak dizi

    private float timer;

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
    }

    void Update()
    {
        if (cameraAnimator.GetCurrentAnimatorStateInfo(0).IsName("TrapPGStartAnim"))
        {
            timer += Time.deltaTime;
            if (timer >= animationDuration)
            {
                OnAnimationComplete();
            }
        }
    }

    public void OnAnimationComplete()
    {
        if (gameUI != null && !Hazard.IsPlayerDead())
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

        StartGame();
    }

    void StartGame()
    {
        // Oyun baþlangýç iþlemlerini buraya ekleyin
    }
}
