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
    private Text speedCountdownText;
    //Revive
    private bool isRevived = false;
    private GameObject reviveIcon;
    private Text reviveCountdownText;
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
           speedCountdownText = speedIcon.transform.Find("SpeedCountdown")?.GetComponent<Text>();
           if (speedCountdownText == null)
           {
               Debug.LogError("CountdownText bulunamadý.");
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
        if (photonView != null && photonView.IsMine && isMovementEnabled)
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

        if (isRevived && photonView.IsMine)
        {
            if (joystick != null)
            {
                joystick.gameObject.SetActive(false);
            }
            if (jumpButton != null)
            {
                jumpButton.gameObject.SetActive(false);
            }
            yield return new WaitForSeconds(3f);

            if (isRevived && photonView.IsMine)
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
                transform.position = currentPosition;
          
                isMovementEnabled = true;
            }
        }
        else
        {
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
            isRevived = true;
            if (reviveIcon != null)
            {
                reviveIcon.SetActive(true);
            }
        }
    }
    private IEnumerator SpeedBoostCountdown(float duration)
    {
        float countdown = duration;

        while (countdown > 0)
        {
            if (speedCountdownText != null)
            {
                speedCountdownText.text = countdown.ToString("F0");  
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

        if (speedCountdownText != null)
        {
            speedCountdownText.text = "0"; 
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