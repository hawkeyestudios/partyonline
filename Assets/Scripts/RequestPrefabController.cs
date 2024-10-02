using PlayFab;
using PlayFab.ClientModels;
using UnityEngine;
using UnityEngine.UI;

public class RequestPrefabController : MonoBehaviour
{
    public Text senderNameText;
    public Button acceptButton;
    public Button rejectButton;

    private string senderDisplayName;

    private void Start()
    {
        acceptButton.onClick.AddListener(OnAcceptClicked);
        rejectButton.onClick.AddListener(OnRejectClicked);
    }

    public void SetSenderName(string senderName)
    {
        senderNameText.text = senderName;
        senderDisplayName = senderName;
    }

    private void OnAcceptClicked()
    {
        ManageFriendRequest(senderDisplayName, true);
    }

    private void OnRejectClicked()
    {
        ManageFriendRequest(senderDisplayName, false);
    }

    private void ManageFriendRequest(string friendDisplayName, bool accept)
    {
        var request = new ExecuteCloudScriptRequest
        {
            FunctionName = accept ? "AcceptFriendRequest" : "RejectFriendRequest",
            FunctionParameter = new { FriendDisplayName = friendDisplayName },
            GeneratePlayStreamEvent = true
        };

        PlayFabClientAPI.ExecuteCloudScript(request, OnRequestSuccess, OnRequestFailure);
    }

    private void OnRequestSuccess(ExecuteCloudScriptResult result)
    {
        Debug.Log("Arkadaþlýk isteði yönetildi.");
        Destroy(gameObject); // Ýsteði kapat
    }

    private void OnRequestFailure(PlayFabError error)
    {
        Debug.LogError("Arkadaþlýk isteði yönetilemedi: " + error.GenerateErrorReport());
    }
}
