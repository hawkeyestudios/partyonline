using UnityEngine;
using Photon.Pun;
using System.Collections;

public class GhostController : MonoBehaviourPun
{
    public float speed = 5f;
    public float rotationSpeed = 5f;

    private Transform targetPlayer;
    private bool isWaiting = false;
    private bool isStopped = false;
    public ParticleSystem impactParticle;
    public Animator animator;

    void Start()
    {
        impactParticle.Stop();
        StartCoroutine(WaitForGhost(10));
    }

    void Update()
    {
        if (isWaiting || !photonView.IsMine)
            return;

        FindClosestPlayer();
        if (targetPlayer != null && !isStopped)
        {
            RotateTowardsPlayer();
            MoveTowardsPlayer();
        }
    }

    void FindClosestPlayer()
    {
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        float minDistance = Mathf.Infinity;
        Transform closestPlayer = null;

        foreach (GameObject player in players)
        {
            float distance = Vector3.Distance(transform.position, player.transform.position);
            if (distance < minDistance)
            {
                minDistance = distance;
                closestPlayer = player.transform;
            }
        }
        targetPlayer = closestPlayer;
    }

    void MoveTowardsPlayer()
    {
        transform.position = Vector3.MoveTowards(transform.position, targetPlayer.position, speed * Time.deltaTime);
    }

    void RotateTowardsPlayer()
    {
        Vector3 direction = (targetPlayer.position - transform.position).normalized;
        Quaternion lookRotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * rotationSpeed);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PhotonView playerPhotonView = other.GetComponent<PhotonView>();
            if (playerPhotonView != null && playerPhotonView.IsMine)
            {
                PlayerMovement playerMovement = other.gameObject.GetComponent<PlayerMovement>();

                if (playerMovement != null)
                {
                    playerMovement.StartCoroutine(playerMovement.BecomeGhost());
                }
                else
                {
                    Debug.LogError("PlayerMovement bileþeni bulunamadý.");
                }
            }
        }
    }

    public IEnumerator StopMovementForSeconds(float duration)
    {
        isStopped = true;
        if (animator != null)
        {
            animator.SetTrigger("Impact");
        }
        if (impactParticle != null)
        {
            impactParticle.Play();
        }
        yield return new WaitForSeconds(duration);
        isStopped = false;
    }

    private IEnumerator WaitForGhost(int seconds)
    {
        isWaiting = true;
        yield return new WaitForSeconds(seconds);
        isWaiting = false;
    }
}
