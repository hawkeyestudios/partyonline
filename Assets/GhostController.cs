using Photon.Pun;
using UnityEngine;

public class GhostController : MonoBehaviourPun
{
    public float speed = 5f;
    private Transform targetPlayer;

    void Update()
    {
        if (!photonView.IsMine)
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
            // Burada oyuncuyu hayalete çevirme iþlemi yapýlacak
            PhotonView playerPhotonView = other.GetComponent<PhotonView>();
            if (playerPhotonView.IsMine)
            {
                // Hayalete dönüþme iþlemi
                other.gameObject.GetComponent<PlayerController>().BecomeGhost();
            }
        }
    }
}
