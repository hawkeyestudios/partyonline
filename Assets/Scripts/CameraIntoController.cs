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

        // Joystick ve Jump butonlarýný devre dýþý býrak
        playerMovements = FindObjectsOfType<PlayerMovement>();
        foreach (PlayerMovement pm in playerMovements)
        {
            pm.SetJoystickActive(false);
            pm.SetJumpButtonActive(false);
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

        // Joystick ve Jump butonlarýný aktif hale getirin
        foreach (PlayerMovement pm in playerMovements)
        {
            pm.SetJoystickActive(true);
            pm.SetJumpButtonActive(true);
        }

        StartGame();
    }

    void StartGame()
    {
        // Oyun baþlangýç iþlemlerini buraya ekleyin
    }
}
