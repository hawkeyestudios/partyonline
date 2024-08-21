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
    private bool isMovementEnabled = false;

    private Transform deadPoint;


    private RectTransform joystickHandle;
    public GameObject boomEffect;

    public GameObject ghostPrefab;

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

        isMovementEnabled = false;
        StartCoroutine(EnableMovementAfterDelay(10f));
    }
    IEnumerator EnableMovementAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        isMovementEnabled = true; // 10 saniye sonra hareketi etkinleþtir
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

            if (Input.GetButtonDown("Jump") && isGrounded && isMovementEnabled)
            {
                Jump();
            }
        }
    }
    public void BecomeGhost()
    {
        Vector3 currentPosition = transform.position;
        Quaternion currentRotation = transform.rotation;
        
        animator.SetTrigger("Die");

        GameObject ghost = PhotonNetwork.Instantiate(ghostPrefab.name, currentPosition, currentRotation);
    }

    private void Jump()
    {
        if (!photonView.IsMine) return;

        rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        isGrounded = false;
        animator.SetTrigger("Jump");

        //photonView.RPC("SyncJump", RpcTarget.Others);
    }
    IEnumerator WaitForSeconds(int seconds)
    {
        yield return new WaitForSeconds(seconds);
        isMovementEnabled = true;
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