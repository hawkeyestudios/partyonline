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
    private bool isDead = false;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        photonView = GetComponent<PhotonView>();
        animator = GetComponent<Animator>();

        // Sahnedeki joystick ve jump butonunu bul
        joystick = FindObjectOfType<Joystick>();
        jumpButton = GameObject.Find("JumpButton")?.GetComponent<Button>();

        // Jump butonuna týklama olayýný baðla
        if (jumpButton != null)
        {
            jumpButton.onClick.AddListener(OnJumpButtonClicked);
        }
        else
        {
            Debug.LogError("JumpButton not found in the scene.");
        }
    }

    private void Update()
    {
        // Sadece kendi karakterimiz için kontrolleri yap
        if (photonView.IsMine)
        {
            if (joystick != null)
            {
                // Karakterin yönünü smooth þekilde döndür
                Vector3 moveDirection = new Vector3(joystick.Horizontal(), 0, joystick.Vertical()).normalized;
                if (moveDirection != Vector3.zero)
                {
                    // Karakterin yönünü döndür
                    Quaternion toRotation = Quaternion.LookRotation(moveDirection, Vector3.up);
                    transform.rotation = Quaternion.Lerp(transform.rotation, toRotation, Time.deltaTime * 10f);

                    // Yürüyüþ animasyonunu tetikle
                    animator.SetBool("Walking", true);
                }
                else
                {
                    // Yürüyüþ animasyonunu durdur
                    animator.SetBool("Walking", false);
                }

                rb.velocity = new Vector3(moveDirection.x * moveSpeed, rb.velocity.y, moveDirection.z * moveSpeed);
            }

            // Jump butonuna týklanýp týklanmadýðýný kontrol et
            if (Input.GetButtonDown("Jump") && isGrounded)
            {
                Jump();
            }
        }
    }

    private void Jump()
    {
        rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        isGrounded = false;

        // Zýplama animasyonunu tetikle
        animator.SetTrigger("Jump");
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            isGrounded = true;
        }
    }

    // Jump butonuna týklama iþlemini iþleyen metod
    private void OnJumpButtonClicked()
    {
        if (isGrounded)
        {
            Jump();
        }
    }
}
