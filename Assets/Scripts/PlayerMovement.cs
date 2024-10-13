using UnityEngine;
using Photon.Pun;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.SceneManagement;

public class PlayerMovement : MonoBehaviour
{
    public float moveSpeed = 3f;
    public float jumpForce = 12f;
    //Speed
    private bool isSpeedBoosted = false;
    private GameObject speedIcon;
    private Image speedYellowBar;
    private Text speedCountdownText;

    //Revive
    private bool isRevived = false;
    private int reviveCount = 1;
    private const int maxReviveCount = 3;
    private GameObject reviveIcon;
    private Text reviveCountdownText;
    private Image[] heartImages = new Image[3];

    //Attack
    private GameObject attackButton;
    private Text attackCountText;
    private int attackCount = 0;
    public ParticleSystem attackParticle;
    private Transform targetGhost;

    private Joystick joystick;
    private Button jumpButton;
    private Rigidbody rb;
    private bool isGrounded;
    private bool canAnim = true;
    private bool hasCollided;
    private bool hasJumped = false;

    private PhotonView photonView;
    private Animator animator;
    private bool isMovementEnabled = false;

    private Transform deadPoint;

    private RectTransform joystickHandle;
    public GameObject boomEffect;
    public GameObject ghostPrefab;
    public ParticleSystem stepParticle;

    //Sumo Map i�in eklenenler
    [HideInInspector]
    public GameObject ball;

    public float maxSpeed = 6f;
    public float speedIncreaseFactor = 0.25f;
    public float rotationSpeed = 100f;
    private float currentSpeed;
    private Image momentumImage;

    private void Start()
    {
        stepParticle.Stop();
        currentSpeed = moveSpeed;
        rb = GetComponent<Rigidbody>();
        photonView = GetComponent<PhotonView>();
        animator = GetComponent<Animator>();

        if (SceneManager.GetActiveScene().name == "CrownPG")
        {
            moveSpeed = 6;
        }

        if (SceneManager.GetActiveScene().name == "TntPG")
        {
            jumpForce = 15;
        }
        if (SceneManager.GetActiveScene().name == "SumoPG")
        {
            momentumImage = GameObject.Find("Momentum")?.GetComponent<Image>();
            if (momentumImage == null)
            {
                Debug.LogError("momentumImage bulunamad�.");
            }

            if (photonView.IsMine)
            {
                if (ball == null)
                {
                    Vector3 spawnPosition = transform.position + transform.forward * 2f;

                    GameObject instantiatedBall = PhotonNetwork.Instantiate("Ball", spawnPosition, Quaternion.identity);

                    ball = instantiatedBall;

                    int ballViewID = instantiatedBall.GetComponent<PhotonView>().ViewID;
                    int playerViewID = photonView.ViewID;

                    photonView.RPC("SetBallParent_RPC", RpcTarget.AllBuffered, ballViewID, playerViewID);
                }
            }
        }

        //Speed �zelli�i
        if (SceneManager.GetActiveScene().name == "GhostPG")
        {
            speedIcon = GameObject.Find("Speed");

            if (speedIcon != null)
            {
                speedIcon.SetActive(false);
            }
            speedYellowBar = speedIcon.transform.Find("YellowBar").GetComponent<Image>();
            if (speedYellowBar == null)
            {
                Debug.LogError("YellowBar bulunamad�.");
            }

            //Revive �zelli�i
            isRevived = false;
            reviveIcon = GameObject.Find("Revive");
            heartImages[0] = reviveIcon?.transform.Find("Heart1").GetComponent<Image>();
            heartImages[1] = reviveIcon?.transform.Find("Heart2").GetComponent<Image>();
            heartImages[2] = reviveIcon?.transform.Find("Heart3").GetComponent<Image>();

            heartImages[0].enabled = true;
            heartImages[1].enabled = false;
            heartImages[2].enabled = false;

            //Attack �zelli�i
            attackButton = GameObject.Find("AttackButton");
            if (attackButton != null)
            {
                attackButton.SetActive(false);
                attackButton.GetComponent<Button>().onClick.AddListener(OnAttackButtonClicked);
            }
            attackCountText = attackButton.transform.Find("AttackCountText").GetComponent<Text>();
            if (attackCountText != null)
            {
                attackCountText.text = attackCount.ToString();
            }
        }

        if (SceneManager.GetActiveScene().name == "TrapPG")
        {
            deadPoint = GameObject.Find("DeadPoint").transform;
        }

        string currentSceneName = SceneManager.GetActiveScene().name;
        if (currentSceneName != "MainMenu" && currentSceneName != "Customize" && currentSceneName != "RoulettePG")
        {
            joystick = FindObjectOfType<Joystick>();
            jumpButton = GameObject.Find("JumpButton").GetComponent<Button>();
        }
        if (joystick != null)
        {
            joystickHandle = joystick.transform.Find("Handle").GetComponent<RectTransform>();
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
        isMovementEnabled = true;
    }

    private void Update()
    {
        if (photonView.IsMine && isMovementEnabled)
        {
            HandleMovement();
        }
    }

    private void HandleMovement()
    {
        if (joystick != null && SceneManager.GetActiveScene().name != "MainMenu")
        {
            Vector3 moveDirection = new Vector3(joystick.Horizontal(), 0, joystick.Vertical()).normalized;

            if (moveDirection != Vector3.zero)
            {
                Quaternion toRotation = Quaternion.LookRotation(moveDirection, Vector3.up);
                transform.rotation = Quaternion.Lerp(transform.rotation, toRotation, Time.deltaTime * 10f);
                animator.SetBool("Walking", true);
                if (!stepParticle.isPlaying && isGrounded)
                {
                    stepParticle.Play();
                }

                if (ball != null)
                {
                    float moveInputMagnitude = new Vector2(joystick.Horizontal(), joystick.Vertical()).magnitude;
                    float rotationAmount = rotationSpeed * moveInputMagnitude * Time.deltaTime;
                    ball.transform.Rotate(Vector3.right, rotationAmount);

                    // H�z� art�r
                    currentSpeed += speedIncreaseFactor * moveInputMagnitude * Time.deltaTime;
                    currentSpeed = Mathf.Clamp(currentSpeed, moveSpeed, maxSpeed);
                }
            }
            else
            {
                animator.SetBool("Walking", false);
                if (!stepParticle.isPlaying)
                {
                    stepParticle.Stop();
                }

                currentSpeed = moveSpeed;
            }

            if (rb != null)
            {
                rb.velocity = new Vector3(moveDirection.x * moveSpeed, rb.velocity.y, moveDirection.z * moveSpeed);
            }

            if (momentumImage != null)
            {
                float extraSpeed = currentSpeed - moveSpeed;
                float maxExtraSpeed = maxSpeed - moveSpeed;
                float fillAmount = extraSpeed / maxExtraSpeed;
                fillAmount = Mathf.Clamp01(fillAmount);
                momentumImage.fillAmount = fillAmount;
            }  
            if (Input.GetButtonDown("Jump") && isGrounded && isMovementEnabled)
            {
                Jump();
            }

            if (!isGrounded && rb.velocity.y < 0)
            {
                if (!hasJumped) 
                {
                    animator.SetBool("isGliding", true);
                }
            }
            else
            {
                animator.SetBool("isGliding", false);
            }
        }
        else
        {
            Debug.LogError("Joystick referans� atanmad�.");
        }
    }

    private void Jump()
    {
        if (!photonView.IsMine) return;

        rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        isGrounded = false;
        hasJumped = true;
        if (animator != null)
        {
            animator.SetTrigger("Jump");
        }
        animator.SetBool("isGliding", false);
        if (!stepParticle.isPlaying)
        {
            stepParticle.Stop();
        }
    }

    private IEnumerator OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Ground") && !isGrounded)
        {
            isGrounded = true;
            hasJumped = false;
            animator.SetBool("isGliding", false);
        }
        else if (other.CompareTag("SpeedOzellik") && !isSpeedBoosted)
        {
            isSpeedBoosted = true;
            moveSpeed *= 2f;

            if (speedIcon != null)
            {
                speedIcon.SetActive(true);
            }

            StartCoroutine(SpeedBoostCountdown(10f));
        }
        else if (other.CompareTag("ReviveOzellik"))
        {
            reviveIcon.SetActive(true);
            if (reviveCount < maxReviveCount)
            {
                reviveCount++;
                heartImages[reviveCount - 1].enabled = true;
                Debug.Log("Kalp al�nd�, toplam kalp: " + reviveCount);
            }
        }
        else if (other.CompareTag("AttackOzellik"))
        {
            attackButton.SetActive(true);
            attackCount++;
            attackCountText.text = attackCount.ToString();
        }
        else if (other.gameObject.CompareTag("Ghost")) //GhostPG i�in
        {
            Vector3 currentPosition = transform.position;
            Quaternion currentRotation = transform.rotation;
            if (!stepParticle.isPlaying)
            {
                stepParticle.Stop();
            }
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
            yield return new WaitForSeconds(0.8f);

            if (boomEffect != null)
            {
                if(!boomEffect.GetComponent<ParticleSystem>().isPlaying)
                {
                    boomEffect.GetComponent<ParticleSystem>().Play();
                }
                
            }
            canAnim = true;
            if (canAnim)
            {
                animator.SetTrigger("Die");
            }

            if (reviveCount > 1)
            {
                reviveCount--;
                heartImages[reviveCount].enabled = false;
                Debug.Log("Bir kalp kaybedildi, kalan kalp: " + reviveCount);

                yield return new WaitForSeconds(2.2f);

                canAnim = false;
                isMovementEnabled = true;
                if(photonView.IsMine)
                {
                    if (joystick != null)
                    {
                        joystick.ResetHandlePosition();
                        joystick.gameObject.SetActive(true);
                    }
                    if (jumpButton != null)
                    {
                        jumpButton.gameObject.SetActive(true);
                    }
                }

            }
            else
            {
                GhostManager ghostManager = FindObjectOfType<GhostManager>();
                ghostManager.MarkPlayerAsFinished(photonView.Owner);

                reviveCount--;
                heartImages[reviveCount].enabled = false;
                Debug.Log("Yeterli kalp yok, yeniden do�ma ba�ar�s�z.");
                yield return new WaitForSeconds(2.2f);

                PhotonNetwork.Destroy(gameObject);
            }

            if (other.CompareTag("Sumo"))
            {
                canAnim = true;
                if (canAnim)
                {
                    animator.SetTrigger("Die");
                }

                if (!stepParticle.isPlaying)
                {
                    stepParticle.Stop();
                }

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

                if (photonView.IsMine)
                {
                    SumoManager sumoManager = FindObjectOfType<SumoManager>();
                    sumoManager.photonView.RPC("PlayerEliminated_RPC", RpcTarget.All, photonView.Owner.ActorNumber);
                }
                yield return new WaitForSeconds(2.2f);

                Destroy(gameObject);
            }
        }
    }
    private IEnumerator SpeedBoostCountdown(float duration)
    {
        float countdown = duration;
        float initialFillAmount = speedYellowBar.fillAmount;
        float targetFillAmount = 0f;
        float elapsedTime = 0f;

        while (countdown > 0)
        {
            elapsedTime += Time.deltaTime;

            if (speedYellowBar != null)
            {
                speedYellowBar.fillAmount = Mathf.Lerp(initialFillAmount, targetFillAmount, elapsedTime / duration);
            }

            yield return null;
            countdown -= Time.deltaTime;
        }

        if (speedYellowBar != null)
        {
            speedYellowBar.fillAmount = 0f;
        }

        moveSpeed /= 2f;
        isSpeedBoosted = false;

        if (speedIcon != null)
        {
            speedIcon.SetActive(false);
        }
    }

    private void OnAttackButtonClicked()
    {
        if (attackCount > 0)
        {
            FindClosestGhost();
            if (targetGhost != null)
            {
                StartCoroutine(SendParticleToGhost());
                attackCount--;
                attackCountText.text = attackCount.ToString();
                if (attackCount == 0)
                {
                    attackButton.SetActive(false);
                }
            }
        }
    }

    private void FindClosestGhost()
    {
        GameObject[] ghosts = GameObject.FindGameObjectsWithTag("Ghost");
        float minDistance = Mathf.Infinity;
        Transform closestGhost = null;

        foreach (GameObject ghost in ghosts)
        {
            float distance = Vector3.Distance(transform.position, ghost.transform.position);
            if (distance < minDistance)
            {
                minDistance = distance;
                closestGhost = ghost.transform;
            }
        }

        targetGhost = closestGhost;
    }

    private IEnumerator SendParticleToGhost()
    {
        Vector3 spawnPosition = transform.position + Vector3.up * 1.5f;

        ParticleSystem particle = Instantiate(attackParticle, spawnPosition, Quaternion.identity);
        particle.Play();

        while (Vector3.Distance(particle.transform.position, targetGhost.position) > 0.1f)
        {
            if (!particle.isPlaying)
            {
                particle.Play();
            }

            particle.transform.position = Vector3.MoveTowards(particle.transform.position, targetGhost.position, 5f * Time.deltaTime);
            yield return null;
        }

        particle.Stop();
        Destroy(particle.gameObject);

        GhostController ghostController = targetGhost.GetComponent<GhostController>();
        if (ghostController != null)
        {
            StartCoroutine(ghostController.StopMovementForSeconds(3f));
        }
    }

    private IEnumerator OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ground") && !isGrounded)
        {
            isGrounded = true;
            animator.SetBool("isGliding", false);
        }

        if (SceneManager.GetActiveScene().name == "TrapPG")   // TrapPG Map i�in
        {
            if (collision.gameObject.CompareTag("Obstacle") && !hasCollided)
            {
                hasCollided = true;
                canAnim = true;
                if (canAnim)
                {
                    animator.SetTrigger("Die");
                }
                if (!stepParticle.isPlaying)
                {
                    stepParticle.Stop();
                }
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

                StartCoroutine(RespawnAfterDelay(2.2f));
            }

        }

        if (SceneManager.GetActiveScene().name == "TntPG") //Barrel Map i�in
        {
            if (collision.gameObject.CompareTag("Barrel"))
            {
                canAnim = true;
                if (canAnim)
                {
                    animator.SetTrigger("Die");
                }

                if (!stepParticle.isPlaying)
                {
                    stepParticle.Stop();
                }

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

                if (photonView.IsMine)
                {
                    BarrelManager barrelManager = FindObjectOfType<BarrelManager>();
                    barrelManager.OnPlayerDeath(photonView.Owner);
                }
                yield return new WaitForSeconds(2.2f);

                Destroy(gameObject);
            }
        }
        if (SceneManager.GetActiveScene().name == "SumoPG") //Sumo Map i�in
        {
            if (collision.gameObject.CompareTag("Player"))
            {
                if (!photonView.IsMine)
                    yield break;

                PhotonView targetPhotonView = collision.gameObject.GetComponent<PhotonView>();

                if (targetPhotonView != null)
                {
                    Vector3 forceDirection = collision.transform.position - transform.position;
                    forceDirection.y = 0;
                    forceDirection.Normalize();

                    float forceMagnitude = currentSpeed * 1.3f;
                    Vector3 force = forceDirection * forceMagnitude;

                    targetPhotonView.RPC("ApplyForce", RpcTarget.All, force);

                    Debug.Log("Oyuncuya �arp�ld�: " + collision.gameObject.name + " Kuvvet: " + forceMagnitude);
                }
            }
        }
    }
    [PunRPC]
    public void SetBallParent_RPC(int ballViewID, int playerViewID)
    {
        PhotonView ballView = PhotonView.Find(ballViewID);
        PhotonView playerView = PhotonView.Find(playerViewID);

        if (ballView != null && playerView != null)
        {
            ballView.transform.SetParent(playerView.transform);
            ballView.transform.localPosition = Vector3.zero; 
        }
    }
    [PunRPC]
    public void ApplyForce(Vector3 force)
    {
        if (rb != null)
        {
            rb.AddForce(force, ForceMode.Impulse);
        }
    }
    private IEnumerator RespawnAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        canAnim = false;
        hasCollided = false;

        if (photonView.IsMine)
        {
            animator.ResetTrigger("Die");
            animator.SetBool("isGliding", false);
            animator.SetBool("Walking", false);

            isGrounded = false;

            if (joystick != null)
            {
                joystick.gameObject.SetActive(true);
                joystick.ResetHandlePosition();
            }
            if (jumpButton != null)
            {
                jumpButton.gameObject.SetActive(true);
            }

            Respawn();

            if (!isMovementEnabled)
            {
                isMovementEnabled = true;
            }
        }
    }

    private void Respawn()
    {
        if (deadPoint != null)
        {
            transform.position = deadPoint.position;
            photonView.RPC("ResetCharacterState_RPC", RpcTarget.AllBuffered);
        }
    }

    [PunRPC]
    private void ResetCharacterState_RPC()
    {
        animator.ResetTrigger("Die");
        canAnim = false;
        if (boomEffect != null)
        {
            boomEffect.GetComponent<ParticleSystem>().Stop();
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
