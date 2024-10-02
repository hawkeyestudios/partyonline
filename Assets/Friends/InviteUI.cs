using Photon.Pun;
using UnityEngine;
using UnityEngine.UI;

public class InviteUI : MonoBehaviour
{
    public Text inviteText;
    public Button acceptButton;

    private string friendPlayFabId;

    public void Initialize(string playFabId)
    {
        friendPlayFabId = playFabId;
        inviteText.text = "Davet aldınız: " + playFabId;

        acceptButton.onClick.AddListener(AcceptInvite);
    }

    private void AcceptInvite()
    {
        // Daveti kabul et
        Debug.Log(friendPlayFabId + " ile davet kabul edildi.");
        // Photon lobiye katıl
        PhotonNetwork.JoinLobby();
        // Diğer işlemler
        Destroy(gameObject); // UI'yi kaldır
    }
}
