using UnityEngine;
using Photon.Pun;

public class FinishLine : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        // Sadece oyuncular tetikleyiciyi aktif edebilir
        if (other.CompareTag("Player"))
        {
            // PhotonView üzerinden oyuncunun bitiþ çizgisine ulaþtýðýný bildir
            PhotonView photonView = other.GetComponent<PhotonView>();

            if (photonView != null && photonView.IsMine)
            {
                GameOverManager gameOverManager = FindObjectOfType<GameOverManager>();

                if (gameOverManager != null)
                {
                    string playerNickname = PlayerPrefs.GetString("DISPLAYNAME");
                    gameOverManager.OnPlayerFinish(playerNickname);
                }
            }
        }
    }
}
