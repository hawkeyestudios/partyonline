using UnityEngine;
using Photon.Pun;
using UnityEngine.UI;
using System.Collections;
using Unity.VisualScripting;
using UnityEngine.SceneManagement;

public class PlayerMovement : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float jumpForce = 7f;
    //Speed
    private bool isSpeedBoosted = false;
    private GameObject speedIcon;
    private Image speedYellowBar;
    private Text speedCountdownText;
    //Revive
    private bool isRevived = false;
    private int reviveCount = 0; // Toplanan kalp sayýsý
    private const int maxReviveCount = 3; // Maksimum kalp sayýsý
    private GameObject reviveIcon;
    private Text reviveCountdownText;
    private Image[] heartImages = new Image[3];
    //Attack
    private GameObject attackIcon;
    private Text attackCountdownText;

    private Joystick joystick;
    private Button jumpButton;
    private Rigidbody rb;
    private bool isGrounded;

    private PhotonView photonView;
    private Animator animator;
    private bool isMovementEnabled = false;

    private Transform deadPoint;

    private RectTransform joystickHandle;
    public GameObject boomEffect;
    public GameObject ghostPrefab;


    private void Start()
    {
        //Speed Özelliði
        if (SceneManager.GetActiveScene().name == "GhostPG")
        {
            speedIcon = GameObject.Find("Speed");
           if (speedIcon != null)
           {
               speedIcon.SetActive(false);
           }
            speedYellowBar = speedIcon.transform.Find("YellowBar")?.GetComponent<Image>();
            if (speedYellowBar == null)
            {
                Debug.LogError("YellowBar bulunamadý.");
            }

            //Revive Özelliði
            isRevived = false;
           reviveIcon = GameObject.Find("Revive");
           if (reviveIcon != null)
           {
               reviveIcon.SetActive(false);
           }
           reviveCountdownText = reviveIcon.transform.Find("ReviveCountdown")?.GetComponent<Text>();
           if (reviveCountdownText == null)
           {
               Debug.LogError("CountdownText bulunamadý.");
           }
            heartImages[0] = reviveIcon.transform.Find("Heart1").GetComponent<Image>();
            heartImages[1] = reviveIcon.transform.Find("Heart2").GetComponent<Image>();
            heartImages[2] = reviveIcon.transform.Find("Heart3").GetComponent<Image>();
            foreach (var heart in heartImages)
            {
                heart.enabled = false;
            }
            //Attack Özelliði
            attackIcon = GameObject.Find("Attack");
           if (attackIcon != null)
           {
               attackIcon.SetActive(false);
           }
           attackCountdownText = attackIcon.transform.Find("AttackCountdown")?.GetComponent<Text>();
           if (attackCountdownText == null)
           {
               Debug.LogError("CountdownText bulunamadý.");
           }
        }

        rb = GetComponent<Rigidbody>();
        photonView = GetComponent<PhotonView>();
        animator = GetComponent<Animator>();

        deadPoint = GameObject.Find("DeadPoint")?.transform;
        joystick = FindObjectOfType<Joystick>();
        jumpButton = GameObject.Find("JumpButton")?.GetComponent<Button>();

        if (joystick != null)
        {
            joystickHandle = joystick.transform.Find("Handle")?.GetComponent<RectTransform>();
        }

        if (jumpButton != null)
        {
            jumpButton.onClick.AddListener(OnJumpButtonClicked);
        }

        isMovementEnabled = false;
        Debug.Log("Baþlangýçta hareket devre dýþý.");
        StartCoroutine(EnableMovementAfterDelay(10f));
    }
    IEnumerator EnableMovementAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        Debug.Log("10 saniye sonra hareket aktif.");
        isMovementEnabled = true; // 10 saniye sonra hareketi etkinleþtir
    }

    private void Update()
    {
        if (photonView.IsMine && isMovementEnabled)
        {
            if (joystick != null)
            {
                Vector3 moveDirection = new Vector3(joystick.Horizontal(), 0, joystick.Vertical()).normalized;

                if (moveDirection != Vector3.zero)
                {
                    Quaternion toRotation = Quaternion.LookRotation(moveDirection, Vector3.up);
                    transform.rotation = Quaternion.Lerp(transform.rotation, toRotation, Time.deltaTime * 10f);
                    animator.SetBool("Walking", true);
                }
                else
                {
                    animator.SetBool("Walking", false);
                }

                if (rb != null)
                {
                    rb.velocity = new Vector3(moveDirection.x * moveSpeed, rb.velocity.y, moveDirection.z * moveSpeed);
                }

                if (Input.GetButtonDown("Jump") && isGrounded && isMovementEnabled)
                {
                    Jump();
                }
            }
            else
            {
                Debug.LogError("Joystick referansý atanmadý.");
            }
        }
    }

    public IEnumerator BecomeGhost()
    {
        Vector3 currentPosition = transform.position;
        Quaternion currentRotation = transform.rotation;
        animator.SetTrigger("Die");
        isMovementEnabled = false;
        gameObject.tag = "Immune";

        if (reviveCount > 0)
        {
            reviveCount--;
            heartImages[reviveCount].enabled = false;
            Debug.Log("Bir kalp kaybedildi, kalan kalp: " + reviveCount);

            yield return new WaitForSeconds(3f);

            isMovementEnabled = true;
            gameObject.tag = "Player";
        }
        else
        {
            Debug.Log("Yeterli kalp yok, yeniden doðma baþarýsýz.");
            Destroy(gameObject);
            // Eðer hiç kalp kalmamýþsa oyuncu artýk yeniden doðamaz.
            GameObject ghost = PhotonNetwork.Instantiate(ghostPrefab.name, currentPosition, currentRotation);  
        }
    }

    private void Jump()
    {
        if (!photonView.IsMine) return;

        rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        isGrounded = false;
        animator.SetTrigger("Jump");
    }
    IEnumerator WaitForSeconds(int seconds)
    {
        yield return new WaitForSeconds(seconds);
        isMovementEnabled = true;
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("SpeedOzellik") && !isSpeedBoosted)
        {
            isSpeedBoosted = true;
            moveSpeed *= 2f;

            if (speedIcon != null)
            {
                speedIcon.SetActive(true);
            }

            StartCoroutine(SpeedBoostCountdown(10f));
        }
        else if(other.CompareTag("ReviveOzellik"))
        {
            reviveIcon.SetActive(true);
            if (reviveCount < maxReviveCount)
            {
                reviveCount++;
                heartImages[reviveCount - 1].enabled = true; // Alýnan kalbi aktif hale getir
                Debug.Log("Kalp alýndý, toplam kalp: " + reviveCount);
            }
        }
    }
    private IEnumerator SpeedBoostCountdown(float duration)
    {
        float countdown = duration;

        while (countdown > 0)
        {
            if (speedYellowBar != null)
            {
                speedYellowBar.fillAmount = countdown / duration; 
            }

            yield return new WaitForSeconds(1f); 
            countdown--;
        }
        moveSpeed /= 2f;
        isSpeedBoosted = false;

        if (speedIcon != null)
        {
            speedIcon.SetActive(false);
        }

        if (speedYellowBar != null)
        {
            speedYellowBar.fillAmount = 0; 
        }
    }

    private IEnumerator OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            isGrounded = true;
        }
        else if (collision.gameObject.CompareTag("Obstacle"))
        {
            animator.SetTrigger("Die");

            isMovementEnabled = false;

            if (photonView.IsMine)
            {
                if (joystick != null)
                {
                    joystick.gameObject.SetActive(false);
                }
                if (jumpButton != null)
                {
                    jumpButton.gameObject.SetActive(false);
                }
            }

            if (boomEffect != null)
            {
                boomEffect.GetComponent<ParticleSystem>().Play();
            }

            yield return new WaitForSeconds(2.2f);

            if (photonView.IsMine)
            {
                // Joystick'i tekrar görünür yap
                if (joystick != null)
                {
                    joystick.gameObject.SetActive(true);
                    // Joystick handle pozisyonunu sýfýrla
                    joystick.ResetHandlePosition();
                }
                if (jumpButton != null)
                {
                    jumpButton.gameObject.SetActive(true);
                }

                Respawn(); // Oyuncuyu geri doður
                isMovementEnabled = true;
            }
        }
    }

    private void Respawn()
    {
        if (deadPoint != null)
        {
            transform.position = deadPoint.position;
        }
    }

    private void OnJumpButtonClicked()
    {
        if (photonView.IsMine && isGrounded)
        {
            Jump();
        }
    }

}