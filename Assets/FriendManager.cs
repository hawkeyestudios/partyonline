using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.UI;
using PlayFab;
using PlayFab.ClientModels;
using System.Collections.Generic;
using Photon.Pun.Demo.Cockpit; // FriendListView ve FriendListCell için gerekli namespace

public class FriendManager : MonoBehaviourPunCallbacks
{
    public InputField friendRequestInput; // Arkadaþlýk isteði göndermek için input field
    public GameObject friendRequestPrefab; // Arkadaþlýk isteði için prefab
    public Transform friendRequestContainer; // Ýsteklerin gösterileceði panel
    public FriendListView friendListView; // Photon'un saðladýðý FriendListView bileþeni

    private List<string> friendsList = new List<string>(); // Takip edilecek arkadaþlarýn listesi

    private void Start()
    {
        // Oyuncu giriþ yaptýðýnda PlayFab'dan istekleri yükle
        LoadFriendRequestsFromPlayFab();
    }

    // Arkadaþlýk isteði gönderme
    public void SendFriendRequest()
    {
        string targetPlayerNickname = friendRequestInput.text; // Hedef oyuncunun adýný alýyoruz
        if (string.IsNullOrEmpty(targetPlayerNickname)) return;

        string senderNickname = PhotonNetwork.NickName; // Gönderen oyuncunun adýný alýyoruz

        PhotonView photonView = PhotonView.Get(this);
        photonView.RPC("ReceiveFriendRequest", RpcTarget.All, senderNickname, targetPlayerNickname);
    }


    // RPC ile arkadaþlýk isteði alma
    [PunRPC]
    public void ReceiveFriendRequest(string senderNickname, string targetNickname)
    {
        if (PhotonNetwork.NickName == targetNickname)
        {
            GameObject requestInstance = Instantiate(friendRequestPrefab, friendRequestContainer);

            // Prefaba doðru gönderici adýný atayalým
            requestInstance.GetComponent<FriendRequestManager>().Initialize(senderNickname, this);
        }
    }

    // Arkadaþlýk isteðini kabul etme
    public void AcceptFriendRequest(string senderNickname)
    {
        // Arkadaþý listeye ekleme iþlemi
        AddFriendToPhotonList(senderNickname);

        // Ýsteði kabul ettikten sonra istek panelinden kaldýrma iþlemi
        RemoveFriendRequest(senderNickname);

        // Arkadaþýn online durumu kontrol edilecek
        StartTrackingFriend(senderNickname);
    }

    private void AddFriendToPhotonList(string friendNickname)
    {
        if (friendListView == null)
        {
            Debug.LogError("FriendListView is not assigned!");
            return;
        }

        if (string.IsNullOrEmpty(friendNickname))
        {
            Debug.LogError("Friend nickname is null or empty!");
            return;
        }

        // Arkadaþlýk listesine ekle
        friendsList.Add(friendNickname);

        // Photon'un Friend List View'ýna arkadaþý ekle
        friendListView.SetFriendDetails(new FriendListView.FriendDetail[]
        {
        new FriendListView.FriendDetail(friendNickname, friendNickname)
        });

        // Arkadaþýn online durumunu takip etmek için Photon'a bildirim gönder
        PhotonNetwork.FindFriends(friendsList.ToArray());
    }


    // Arkadaþlýk isteðini reddetme veya kabul edilmeyen isteði silme
    public void RejectFriendRequest(string senderNickname)
    {
        // Ýsteði sil
        RemoveFriendRequest(senderNickname);
    }

    private void RemoveFriendRequest(string senderNickname)
    {
        // Ýstek listeden silinir ve PlayFab'dan kaldýrýlýr
        foreach (Transform request in friendRequestContainer)
        {
            FriendRequestManager friendRequest = request.GetComponent<FriendRequestManager>();
            if (friendRequest.GetSenderNickname() == senderNickname)
            {
                Destroy(request.gameObject);
                RemoveFriendRequestFromPlayFab(senderNickname);
                break;
            }
        }
    }

    // PlayFab'a arkadaþlýk isteði kaydetme
    public void SaveFriendRequestToPlayFab(string senderId, string receiverId)
    {
        var request = new UpdateUserDataRequest
        {
            Data = new Dictionary<string, string>
            {
                { "FriendRequest_" + senderId, senderId }
            },
            Permission = UserDataPermission.Private
        };

        PlayFabClientAPI.UpdateUserData(request,
            result => Debug.Log("Friend request saved successfully."),
            error => Debug.LogError("Error saving friend request: " + error.GenerateErrorReport()));
    }

    // PlayFab'dan arkadaþlýk isteklerini yükleme
    public void LoadFriendRequestsFromPlayFab()
    {
        PlayFabClientAPI.GetUserData(new GetUserDataRequest(),
            result =>
            {
                if (result.Data != null && result.Data.Count > 0)
                {
                    foreach (var entry in result.Data)
                    {
                        if (entry.Key.StartsWith("FriendRequest_"))
                        {
                            string senderId = entry.Value.Value;
                            // Bu isteði UI'da gösterin
                            ShowFriendRequest(senderId);
                        }
                    }
                }
            },
            error => Debug.LogError("Error loading friend requests: " + error.GenerateErrorReport()));
    }

    // PlayFab'da kaydedilen arkadaþlýk isteðini silme
    public void RemoveFriendRequestFromPlayFab(string senderId)
    {
        var updateDataRequest = new UpdateUserDataRequest
        {
            Data = new Dictionary<string, string>
            {
                { "FriendRequest_" + senderId, null }
            }
        };

        PlayFabClientAPI.UpdateUserData(updateDataRequest,
            result => Debug.Log("Friend request removed successfully."),
            error => Debug.LogError("Error removing friend request: " + error.GenerateErrorReport()));
    }

    private void ShowFriendRequest(string senderId)
    {
        // Arkadaþlýk isteðini UI'da gösterme iþlemini buraya ekleyin.
        GameObject requestInstance = Instantiate(friendRequestPrefab, friendRequestContainer);
        requestInstance.GetComponent<FriendRequestManager>().Initialize(senderId, this);
    }

    // Arkadaþýn durumunu takip etmeye baþla
    private void StartTrackingFriend(string friendNickname)
    {
        if (!friendsList.Contains(friendNickname))
        {
            friendsList.Add(friendNickname);
            PhotonNetwork.FindFriends(friendsList.ToArray());
        }
    }

    // Arkadaþlarýn durumlarýný güncellemek için Photon'dan gelen geri çaðrý
    public override void OnFriendListUpdate(List<Photon.Realtime.FriendInfo> friendList)
    {
        // Photon Friend List View kullanýlarak otomatik olarak güncellenecek
        friendListView.OnFriendListUpdate(friendList);
    }
}
