using UnityEngine;
using Photon.Pun;
using System.Collections;
using System.Collections.Generic;

public class GhostController : MonoBehaviourPun
{
    public float speed = 5f;
    public float rotationSpeed = 5f;

    private Transform targetPlayer;
    private bool isWaiting = false;
    private bool isStopped = false;
    public ParticleSystem impactParticle;
    public Animator animator;

    private HashSet<Transform> recentlyAttackedPlayers = new HashSet<Transform>();

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
            animator.SetBool("Walking", true);
            RotateTowardsPlayer();
            MoveTowardsPlayer();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player") && !recentlyAttackedPlayers.Contains(other.transform))
        {
            animator.SetTrigger("Attack");
            StartCoroutine(HandleAttackOnPlayer(other.transform));
            speed += 0.2f;
        }
    }

    IEnumerator HandleAttackOnPlayer(Transform attackedPlayer)
    {
        recentlyAttackedPlayers.Add(attackedPlayer);
        yield return StartCoroutine(StopMovementForSeconds(1f)); 

        
        yield return new WaitForSeconds(5f);
        recentlyAttackedPlayers.Remove(attackedPlayer);
    }

    void FindClosestPlayer()
    {
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        float minDistance = Mathf.Infinity;
        Transform closestPlayer = null;

        foreach (GameObject player in players)
        {
            Transform playerTransform = player.transform;
            if (recentlyAttackedPlayers.Contains(playerTransform))
                continue;

            float distance = Vector3.Distance(transform.position, playerTransform.position);
            if (distance < minDistance)
            {
                minDistance = distance;
                closestPlayer = playerTransform;
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

    public IEnumerator StopMovementForSeconds(float duration)
    {
        isStopped = true;
        animator.SetBool("Walking", false);

        if (animator != null)
        {
            animator.SetTrigger("Impact");
        }
        yield return new WaitForSeconds(0.2f);
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
