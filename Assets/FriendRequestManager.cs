using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;

public class FriendRequestManager : MonoBehaviour
{
    private string senderNickname;
    private FriendManager friendManager;
    public Text senderNameText;

    public void Initialize(string senderNickname, FriendManager friendManager)
    {
        senderNameText.text = senderNickname;
    }

    // Ýstek kabul edildiyse
    public void OnAcceptRequest()
    {
        friendManager.AcceptFriendRequest(senderNickname);
        Destroy(gameObject);
    }

    // Ýstek reddedildiyse
    public void OnRejectRequest()
    {
        friendManager.RejectFriendRequest(senderNickname);
        Destroy(gameObject);
    }

    public string GetSenderNickname()
    {
        return senderNickname;
    }
}
