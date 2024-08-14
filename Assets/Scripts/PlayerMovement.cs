using UnityEngine;
using Photon.Pun;
using UnityEngine.UI;

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

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        photonView = GetComponent<PhotonView>();
        animator = GetComponent<Animator>();

        // Sahnedeki joystick ve jump butonunu bul
        joystick = FindObjectOfType<Joystick>();
        jumpButton = GameObject.Find("JumpButton")?.GetComponent<Button>();

        if (jumpButton != null)
        {
            jumpButton.onClick.AddListener(OnJumpButtonClicked);
        }

        if (photonView.IsMine)
        {
            // Sadece yerel oyuncu joystick ve jump butonunu kullanabilir
        }
        if (!photonView.IsMine)
        {
            GetComponent<GhostController>().enabled = false; // Hayalet kontrolünü sadece kendi sahibi olan oyuncu kontrol edebilir
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
                photonView.RPC("SyncWalking", RpcTarget.Others, true);
            }
            else
            {
                animator.SetBool("Walking", false);
                photonView.RPC("SyncWalking", RpcTarget.Others, false);
            }

            rb.velocity = new Vector3(moveDirection.x * moveSpeed, rb.velocity.y, moveDirection.z * moveSpeed);

            if (Input.GetButtonDown("Jump") && isGrounded)
            {
                Jump();
            }
        }
    }
    public void DisableMovement()
    {
        isMovementEnabled = false;
        rb.isKinematic = true; // Fiziði devre dýþý býrak
    }
    public void EnableMovement()
    {
        isMovementEnabled = true;
        rb.isKinematic = false; // Fiziði tekrar etkinleþtir
    }

    private void Jump()
    {
        if (!photonView.IsMine) return; 

        rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        isGrounded = false;
        animator.SetTrigger("Jump");
        photonView.RPC("SyncJump", RpcTarget.Others); 
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            isGrounded = true;
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
