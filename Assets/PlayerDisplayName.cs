using UnityEngine;
using Photon.Pun;
using TMPro;

public class PlayerDisplayName : MonoBehaviour
{
    public TMP_Text playerDisplayName;

    private void Start()
    {
        // PlayerPrefs'ten DisplayName'i çek
        string displayName = PlayerPrefs.GetString("DISPLAYNAME", "Unknown Player");

        // Photon RPC ile diðer oyunculara adýný gönder
        PhotonView photonView = GetComponent<PhotonView>();
        if (photonView.IsMine)
        {
            photonView.RPC("SetPlayerName", RpcTarget.AllBuffered, displayName);
        }
    }

    [PunRPC]
    public void SetPlayerName(string name)
    {
        // Gelen ismi ekranda göster
        playerDisplayName.text = name;
    }
}
