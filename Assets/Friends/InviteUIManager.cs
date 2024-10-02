using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;

public class InviteUIManager : MonoBehaviourPunCallbacks
{
    public GameObject invitePanelPrefab;
    public Transform canvasTransform;

    private GameObject currentInvitePanel;
    private string pendingRoomName;

    [PunRPC]
    public void ReceiveInvite(string roomName, string inviter)
    {
        if (currentInvitePanel != null)
        {
            Destroy(currentInvitePanel);
        }

        currentInvitePanel = Instantiate(invitePanelPrefab, canvasTransform);

        Text inviteMessageText = currentInvitePanel.transform.Find("inviteMessageText").GetComponent<Text>();
        inviteMessageText.text = $"{inviter} seni lobiye davet etti. Lobi: {roomName}";

        Button acceptButton = currentInvitePanel.transform.Find("acceptButton").GetComponent<Button>();
        Button rejectButton = currentInvitePanel.transform.Find("rejectButton").GetComponent<Button>();

        acceptButton.onClick.AddListener(() => AcceptInvite(roomName));
        rejectButton.onClick.AddListener(RejectInvite);

        pendingRoomName = roomName;
    }

    void AcceptInvite(string roomName)
    {
        if (!string.IsNullOrEmpty(roomName))
        {
            PhotonNetwork.JoinRoom(roomName);
            Destroy(currentInvitePanel);
        }
    }

    void RejectInvite()
    {
        Destroy(currentInvitePanel);
    }

    public override void OnJoinedRoom()
    {
        Debug.Log("Lobiye katıldınız: " + PhotonNetwork.CurrentRoom.Name);
    }

    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        Debug.LogError("Lobiye katılma başarısız: " + message);
    }
}
