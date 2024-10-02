using PlayFab;
using PlayFab.ClientModels;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FriendRequestReceiver : MonoBehaviour
{
    public GameObject requestPanel;
    public GameObject requestPrefab;

    private void Start()
    {
        GetFriendRequests();
    }

    private void GetFriendRequests()
    {
        var request = new GetUserDataRequest
        {
            PlayFabId = PlayFabSettings.staticPlayer.PlayFabId,
            Keys = new List<string> { "FriendRequests" }
        };

        PlayFabClientAPI.GetUserData(request, OnDataReceived, OnDataFailure);
    }

    private void OnDataReceived(GetUserDataResult result)
    {
        if (result.Data != null && result.Data.ContainsKey("FriendRequests"))
        {
            string[] requests = result.Data["FriendRequests"].Value.Split(';');
            foreach (string request in requests)
            {
                if (!string.IsNullOrEmpty(request))
                {
                    CreateRequestPrefab(request);
                }
            }
        }
    }

    private void CreateRequestPrefab(string senderDisplayName)
    {
        GameObject requestObj = Instantiate(requestPrefab, requestPanel.transform);
        RequestPrefabController controller = requestObj.GetComponent<RequestPrefabController>();
        controller.SetSenderName(senderDisplayName);
    }

    private void OnDataFailure(PlayFabError error)
    {
        Debug.LogError("Arkadaþlýk istekleri alýnamadý: " + error.GenerateErrorReport());
    }
}
