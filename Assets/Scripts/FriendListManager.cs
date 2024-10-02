using PlayFab;
using PlayFab.ClientModels;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FriendListManager : MonoBehaviour
{
    public GameObject friendsPanel;
    public GameObject friendPrefab;

    private void Start()
    {
        GetFriendList();
    }

    private void GetFriendList()
    {
        var request = new GetUserDataRequest
        {
            PlayFabId = PlayFabSettings.staticPlayer.PlayFabId,
            Keys = new List<string> { "FriendsList" }
        };

        PlayFabClientAPI.GetUserData(request, OnDataReceived, OnDataFailure);
    }

    private void OnDataReceived(GetUserDataResult result)
    {
        if (result.Data != null && result.Data.ContainsKey("FriendsList"))
        {
            string[] friends = result.Data["FriendsList"].Value.Split(';');
            foreach (string friend in friends)
            {
                if (!string.IsNullOrEmpty(friend))
                {
                    CreateFriendPrefab(friend);
                }
            }
        }
    }

    private void CreateFriendPrefab(string friendDisplayName)
    {
        GameObject friendObj = Instantiate(friendPrefab, friendsPanel.transform);
        FriendPrefabController controller = friendObj.GetComponent<FriendPrefabController>();
        controller.SetFriendName(friendDisplayName);
    }

    private void OnDataFailure(PlayFabError error)
    {
        Debug.LogError("Arkadaþ listesi alýnamadý: " + error.GenerateErrorReport());
    }
}
