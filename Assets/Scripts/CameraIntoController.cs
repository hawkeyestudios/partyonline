using UnityEngine;
using Photon.Pun;

public class CameraIntroController : MonoBehaviour
{
    public Animator cameraAnimator;
    public GameObject gameUI;
    public float animationDuration = 5f;
    public CameraFollow cameraFollow;
    private PlayerMovement[] playerMovements; 


    private float timer;
    private bool hasAnimationCompleted = false; 

    void Start()
    {
        timer = 0f;

        if (cameraAnimator != null)
        {
            cameraAnimator.SetTrigger("StartGame");
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
                    hasAnimationCompleted = true;
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

        StartGame();
    }

    void StartGame()
    {

    }
}
