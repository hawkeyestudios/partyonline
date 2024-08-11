using UnityEngine;
using Photon.Pun;

public class CameraIntroController : MonoBehaviour
{
    public Animator cameraAnimator;
    public GameObject gameUI;
    public float animationDuration = 5f;
    public CameraFollow cameraFollow;

    private float timer;

    void Start()
    {
        gameUI.SetActive(false);
        timer = 0f;

        if (cameraAnimator != null)
        {
            cameraAnimator.SetTrigger("StartGame");
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
        // Hazard.IsPlayerDead() methodunu kontrol ederek gameUI'yi aktif hale getir
        if (gameUI != null && !Hazard.IsPlayerDead())
        {
            gameUI.SetActive(true);
        }

        if (cameraFollow != null)
        {
            cameraFollow.StartCameraFollow();
        }

        StartGame();
    }

    void StartGame()
    {
        // Oyun baþlangýç iþlemlerini buraya ekleyin
    }
}
