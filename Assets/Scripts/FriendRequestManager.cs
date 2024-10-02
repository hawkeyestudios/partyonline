using PlayFab;
using PlayFab.ClientModels;
using UnityEngine;
using UnityEngine.UI;

public class FriendRequestManager : MonoBehaviour
{
    public InputField friendInputField;
    public Button sendRequestButton;

    private void Start()
    {
        sendRequestButton.onClick.AddListener(SendFriendRequest);
    }

    private void SendFriendRequest()
    {
        string friendDisplayName = friendInputField.text;
        if (string.IsNullOrEmpty(friendDisplayName))
        {
            Debug.LogWarning("Arkada� ad� bo� olamaz.");
            return;
        }

        var request = new ExecuteCloudScriptRequest
        {
            FunctionName = "SendFriendRequest",
            FunctionParameter = new { FriendDisplayName = friendDisplayName },
            GeneratePlayStreamEvent = true
        };

        PlayFabClientAPI.ExecuteCloudScript(request, OnRequestSuccess, OnRequestFailure);
    }

    private void OnRequestSuccess(ExecuteCloudScriptResult result)
    {
        Debug.Log("Arkada�l�k iste�i g�nderildi.");
    }

    private void OnRequestFailure(PlayFabError error)
    {
        Debug.LogError("Arkada�l�k iste�i g�nderilemedi: " + error.GenerateErrorReport());
    }
}
