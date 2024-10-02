using UnityEngine;
using PlayFab;
using PlayFab.ClientModels;
using System.Collections.Generic;
using UnityEngine.UI;
using Photon.Pun;

public class FriendManager : MonoBehaviour
{
    public SceneController sceneController; // UI yöneticisine referans
    public InputField friendNicknameInputField; // UI InputField referansı
    public Button sendInviteButton; // Davet gönderme butonu
    public Text feedbackText; // Geri bildirim metni

    private string currentLobbyID; // Mevcut lobi ID'si

    private void Start()
    {
        sendInviteButton.onClick.AddListener(OnSendInviteButtonClicked);
    }

    // Arkadaş ekleme fonksiyonu (DisplayName ile)
    public void AddFriendByDisplayName(string displayName)
    {
        var request = new AddFriendRequest
        {
            FriendTitleDisplayName = displayName
        };

        PlayFabClientAPI.AddFriend(request, OnAddFriendSuccess, OnError);
    }

    // Arkadaş ekleme fonksiyonu (PlayFabId ile)
    public void SendFriendRequest(string friendPlayFabId)
    {
        Debug.Log($"Arkadaşlık isteği gönderiliyor: {friendPlayFabId}");

        var request = new AddFriendRequest
        {
            FriendPlayFabId = friendPlayFabId
        };

        PlayFabClientAPI.AddFriend(request, OnSendFriendRequestSuccess, OnError);
    }

    private void OnSendFriendRequestSuccess(AddFriendResult result)
    {
        Debug.Log("Arkadaşlık isteği başarıyla gönderildi.");
    }

    // Gelen arkadaşlık isteklerini alma
    public void GetFriendRequests()
    {
        var request = new ExecuteCloudScriptRequest
        {
            FunctionName = "GetFriendRequests"
        };

        PlayFabClientAPI.ExecuteCloudScript(request,
            result =>
            {
                var jsonResult = result.FunctionResult as Dictionary<string, object>;
                if (jsonResult != null && jsonResult.ContainsKey("requests"))
                {
                    var requests = jsonResult["requests"] as List<object>;
                    List<string> friendRequests = new List<string>();

                    foreach (var req in requests)
                    {
                        friendRequests.Add(req.ToString());
                    }
                }
            },
            error => Debug.LogError($"Hata: {error.GenerateErrorReport()}"));
    }

    // Arkadaşlık isteğini kabul etme
    public void AcceptFriendRequest(string friendPlayFabId)
    {
        Debug.Log($"Arkadaşlık isteği kabul ediliyor: {friendPlayFabId}");

        PlayFabClientAPI.ExecuteCloudScript(new ExecuteCloudScriptRequest
        {
            FunctionName = "AcceptFriendRequest",
            FunctionParameter = new { PlayFabId = friendPlayFabId }
        },
        result =>
        {
            Debug.Log($"Arkadaşlık isteği başarıyla kabul edildi: {friendPlayFabId}");
            GetFriendsList(sceneController.DisplayFriends);
        },
        error =>
        {
            Debug.LogError($"Arkadaşlık isteği kabul edilemedi: {error.GenerateErrorReport()}");
        });
    }

    // Arkadaşlık isteğini reddetme
    public void RejectFriendRequest(string friendPlayFabId)
    {
        Debug.Log($"Arkadaşlık isteği reddediliyor: {friendPlayFabId}");

        PlayFabClientAPI.ExecuteCloudScript(new ExecuteCloudScriptRequest
        {
            FunctionName = "RejectFriendRequest",
            FunctionParameter = new { PlayFabId = friendPlayFabId }
        },
        result =>
        {
            Debug.Log($"Arkadaşlık isteği başarıyla reddedildi: {friendPlayFabId}");
            GetFriendsList(sceneController.DisplayFriends);
        },
        error =>
        {
            Debug.LogError($"Arkadaşlık isteği reddedilemedi: {error.GenerateErrorReport()}");
        });
    }

    // Hata mesajları
    private void OnError(PlayFabError error)
    {
        Debug.LogError("Hata: " + error.GenerateErrorReport());
    }

    // Arkadaş eklenince başarılı işlem
    private void OnAddFriendSuccess(AddFriendResult result)
    {
        Debug.Log("Arkadaş başarıyla eklendi!");
        GetFriendsList(sceneController.DisplayFriends);
    }

    // Arkadaşın PlayFab ID'sini almak için metot
    public void GetFriendPlayFabID(string displayName, System.Action<string> callback)
    {
        GetFriendsList((friends) =>
        {
            foreach (var friend in friends)
            {
                if (friend.TitleDisplayName == displayName)
                {
                    callback(friend.FriendPlayFabId);
                    return;
                }
            }
            callback(null); // Arkadaş bulunamazsa
        });
    }

    // Arkadaş listesini al ve callback fonksiyonuna gönder
    public void GetFriendsList(System.Action<List<FriendInfo>> callback)
    {
        PlayFabClientAPI.GetFriendsList(new GetFriendsListRequest(), result =>
        {
            List<FriendInfo> friends = result.Friends;
            callback(friends);
        },
        error =>
        {
            Debug.LogError("Arkadaş listesi alınamadı: " + error.GenerateErrorReport());
            callback(null); // Hata durumunda null döndür
        });
    }

    // Arkadaşlık davetini nickname ile gönderme
    public void SendInviteToFriend(string displayName, string lobbyID)
    {
        // Oda kontrolü
        if (PhotonNetwork.InRoom)
        {
            GetFriendPlayFabID(displayName, (friendPlayFabId) =>
            {
                if (!string.IsNullOrEmpty(friendPlayFabId))
                {
                    Debug.Log($"Davet gönderiliyor: {displayName} ({friendPlayFabId})");

                    // PhotonView bileşenini al
                    PhotonView photonView = GetComponent<PhotonView>();
                    if (photonView != null)
                    {
                        photonView.RPC("ReceiveLobbyInvite", RpcTarget.Others, lobbyID, PhotonNetwork.NickName);
                    }
                    else
                    {
                        Debug.LogError("PhotonView bulunamadı.");
                    }
                }
                else
                {
                    Debug.LogError("Arkadaş bulunamadı.");
                }
            });
        }
        else
        {
            Debug.LogError("Oda bulunamadı. Davet gönderilemedi.");
        }
    }




    // Davet gönderme butonuna tıklandığında
    public void OnSendInviteButtonClicked()
    {
        string friendNickname = friendNicknameInputField.text;
        if (!string.IsNullOrEmpty(friendNickname))
        {
            SendInviteToFriend(friendNickname, currentLobbyID);
            feedbackText.text = $"Davet gönderildi: {friendNickname}";
        }
        else
        {
            feedbackText.text = "Lütfen geçerli bir nickname girin.";
        }
    }
}
