using UnityEngine;
using Photon.Pun;
using UnityEngine.UI;
using System.Collections;
using Unity.VisualScripting;

public class PlayerMovement : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float jumpForce = 7f;

    private Joystick joystick;
    private Button jumpButton;
    private Rigidbody rb;
    private bool isGrounded;

    private PhotonView photonView;
    private Animator animator;
    private bool isMovementEnabled = true;

    private Transform deadPoint;


    private RectTransform joystickHandle;
    public GameObject boomEffect;
    private bool hasFinished = false;

    public GameObject ghostPrefab;
    private bool isGhost = false;
    private GhostController ghostController;

    private void Start()
    {
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

        if (!photonView.IsMine)
        {
            GetComponent<GhostController>().enabled = false;
        }
    }

    private void Update()
    {
        if (photonView.IsMine && isMovementEnabled)
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

            rb.velocity = new Vector3(moveDirection.x * moveSpeed, rb.velocity.y, moveDirection.z * moveSpeed);

            if (Input.GetButtonDown("Jump") && isGrounded)
            {
                Jump();
            }
        }
    }
    public void BecomeGhost()
    {
        Vector3 currentPosition = transform.position;
        Quaternion currentRotation = transform.rotation;


        GameObject ghost = PhotonNetwork.Instantiate(ghostPrefab.name, currentPosition, currentRotation);


        PhotonNetwork.Destroy(gameObject);
    }
    public void DisableMovement()
    {
        isMovementEnabled = false;
        rb.velocity = Vector3.zero;
        rb.isKinematic = true;
    }

    public void EnableMovement()
    {
        isMovementEnabled = true;
        rb.isKinematic = false;
    }

    private void Jump()
    {
        if (!photonView.IsMine) return;

        rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        isGrounded = false;
        animator.SetTrigger("Jump");

        // Diðer oyunculara zýplama bilgisini ilet
        photonView.RPC("SyncJump", RpcTarget.Others);
    }
    IEnumerator WaitForSeconds(int seconds)
    {
        yield return new WaitForSeconds(seconds);
    }
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            isGrounded = true;
        }
        else if (collision.gameObject.CompareTag("Obstacle"))
        {

            photonView.RPC("HandleDeathRPC", RpcTarget.All);
        }
    }

    [PunRPC]
    private void HandleDeathRPC()
    {
        StartCoroutine(HandleDeath());
    }

    private IEnumerator HandleDeath()
    {
        DisableMovement(); // Hareketi durdur

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

        animator.SetTrigger("Die"); // Die animasyonunu oynat

        if (boomEffect != null)
        {
            boomEffect.GetComponent<ParticleSystem>().Play();
        }

        // Die animasyonu süresince bekle
        yield return new WaitForSeconds(animator.GetCurrentAnimatorStateInfo(0).length + 1.5f);

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
        }
    }

    private void Respawn()
    {
        if (deadPoint != null)
        {
            transform.position = deadPoint.position;
            EnableMovement();
        }
    }

    private void OnJumpButtonClicked()
    {
        if (photonView.IsMine && isGrounded)
        {
            Jump();
        }
    }

    [PunRPC]
    private void SyncWalking(bool isWalking)
    {
        animator.SetBool("Walking", isWalking);
    }

    [PunRPC]
    private void SyncJump()
    {
        animator.SetTrigger("Jump");
    }
}