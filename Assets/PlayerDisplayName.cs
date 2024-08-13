using UnityEngine;
using Photon.Pun;
using TMPro;

public class PlayerDisplayName : MonoBehaviour
{
    public TMP_Text playerDisplayName;

    private Camera mainCamera; // Ana kamera referansý

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

        // Ana kamerayý bul
        mainCamera = Camera.main;
    }

    private void Update()
    {
        if (mainCamera != null)
        {
            // Metni kameraya doðru döndür
            transform.LookAt(mainCamera.transform);

            // Y eksenindeki dönüþü sýfýrla
            transform.eulerAngles = new Vector3(0, transform.eulerAngles.y + 180f, 0);
        }
    }

    [PunRPC]
    public void SetPlayerName(string name)
    {
        // Gelen ismi ekranda göster
        playerDisplayName.text = name;
    }
}
