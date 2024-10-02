using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class CrownCollisionHandler : MonoBehaviourPunCallbacks
{
    private CrownManager crownManager;

    private void Start()
    {
        crownManager = FindObjectOfType<CrownManager>();
    }
    private void OnTriggerEnter(Collider other)
    {
        if (crownManager == null)
        {
            Debug.LogError("CrownManager is not assigned in CrownCollisionHandler.");
            return;
        }

        if (other.CompareTag("Player"))
        {
            PhotonView playerPhotonView = other.GetComponent<PhotonView>();
            if (playerPhotonView != null && playerPhotonView.IsMine)
            {
                Player player = playerPhotonView.Owner;
                Debug.Log("Player " + player.NickName + " triggered crown interaction.");

                crownManager.OnPlayerInteractWithCrown(player);
            }
        }
    }
}

