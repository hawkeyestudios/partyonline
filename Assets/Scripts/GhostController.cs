using Photon.Pun;
using UnityEngine;

public class GhostController : MonoBehaviourPun
{
    public float speed = 5f;
    public float startDelay = 3f;  // Ghost'un harekete baþlamadan önce bekleyeceði süre

    private Transform targetPlayer;
    private bool canMove = false;

    void Start()
    {
        if (photonView.IsMine)
        {
            Invoke("StartMoving", startDelay); // Ghost'un hareketini geciktir
        }
    }

    void Update()
    {
        if (!photonView.IsMine || !canMove)
            return;

        FindClosestPlayer();
        if (targetPlayer != null)
        {
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

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PhotonView playerPhotonView = other.GetComponent<PhotonView>();
            if (playerPhotonView.IsMine)
            {
                other.gameObject.GetComponent<PlayerController>().BecomeGhost();
            }
        }
    }

    void StartMoving()
    {
        canMove = true; // 3 saniye sonra hareket etmeye baþla
    }
}
