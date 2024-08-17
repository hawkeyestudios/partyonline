using UnityEngine;
using Photon.Pun;
using TMPro;

public class PlayerDisplayName : MonoBehaviour
{
    public TMP_Text playerDisplayName;

    private Camera mainCamera; // Ana kamera referansý

    private void Start()
    {
        string displayName = PlayerPrefs.GetString("DISPLAYNAME", "Guest");
        playerDisplayName.text = displayName;

        PhotonView photonView = GetComponent<PhotonView>();

        if (photonView.IsMine)
        {
            playerDisplayName.text = PhotonNetwork.NickName;
        }
        else
        {
            playerDisplayName.text = photonView.Owner.NickName;
        }

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
}
