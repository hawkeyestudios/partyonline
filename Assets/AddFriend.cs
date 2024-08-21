using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using PlayFab;
using PlayFab.ClientModels;
using System.Collections;

public class AddFriend : MonoBehaviour
{
    // Arkadaþ ekleme
    public InputField friendInputField;

    // Arkadaþ isteði kabul ve reddetme
    public GameObject friendRequestPrefab;
    public Transform requestParentPanel;

    // Arkadaþ listesi için
    public GameObject friendListItemPrefab;
    public Transform friendsListContainer;

    private string playerDisplayName;

    private void Start()
    {
        // Oyuncunun profil bilgilerini al ve DisplayName'i sakla
        PlayFabClientAPI.GetAccountInfo(new GetAccountInfoRequest(), result =>
        {
            playerDisplayName = result.AccountInfo.TitleInfo.DisplayName;
            Debug.Log("Player DisplayName: " + playerDisplayName);

            // Arkadaþlýk isteklerini kontrol et
            CheckFriendRequests();
        }, error =>
        {
            Debug.LogError("Error getting account info: " + error.GenerateErrorReport());
        });
    }

    // 1. Arkadaþlýk isteði gönderme
    public void AddFriendByDisplayName()
    {
        string friendDisplayName = friendInputField.text;

        PlayFabClientAPI.GetAccountInfo(new GetAccountInfoRequest { TitleDisplayName = friendDisplayName }, result =>
        {
            string targetPlayerId = result.AccountInfo.PlayFabId;

            var request = new AddFriendRequest
            {
                FriendPlayFabId = targetPlayerId
            };

            PlayFabClientAPI.AddFriend(request, addFriendResult =>
            {
                Debug.Log("Friend request sent successfully to " + friendDisplayName);

                // Arkadaþ listesi periyodik olarak kontrol edilmeye baþlanabilir
                StartCoroutine(CheckIfFriendAccepted(targetPlayerId));

            }, error =>
            {
                Debug.LogError("Error sending friend request: " + error.GenerateErrorReport());
            });

        }, error =>
        {
            Debug.LogError("Error retrieving account info: " + error.GenerateErrorReport());
        });
    }
    private IEnumerator CheckIfFriendAccepted(string friendPlayFabId)
    {
        while (true)
        {
            // Belirli bir süre bekliyoruz (örneðin 5 saniye)
            yield return new WaitForSeconds(5f);

            // Arkadaþ listesini kontrol ediyoruz
            bool friendFound = false;
            PlayFabClientAPI.GetFriendsList(new GetFriendsListRequest(), result =>
            {
                foreach (var friend in result.Friends)
                {
                    if (friend.FriendPlayFabId == friendPlayFabId)
                    {
                        Debug.Log("Friend request accepted by: " + friend.TitleDisplayName);
                        friendFound = true;

                        // Eðer arkadaþlýk kabul edildiyse, her iki taraf da UI'larýný günceller
                        AddFriendToFriendsList(friend.TitleDisplayName);
                        break;
                    }
                }

                if (friendFound)
                {
                    StopCoroutine("CheckIfFriendAccepted"); // Kontrol iþlemi sonlandýrýlýr
                }

            }, error =>
            {
                Debug.LogError("Error retrieving friends list: " + error.GenerateErrorReport());
            });

            if (friendFound)
            {
                yield break; // Eðer arkadaþ bulunduysa, döngüden çýkýyoruz
            }
        }
    }
    // 2. Arkadaþlýk isteklerini kontrol etme
    public void CheckFriendRequests()
    {
        var request = new GetFriendsListRequest(); // Sadece boþ bir istek oluþturun

        PlayFabClientAPI.GetFriendsList(request, result =>
        {
            if (result.Friends != null && result.Friends.Count > 0)
            {
                foreach (var friend in result.Friends)
                {
                    // Arkadaþlýk isteði veya arkadaþ bilgilerini burada iþleyebilirsiniz
                    Debug.Log("Friend: " + friend.TitleDisplayName);
                    OnFriendRequestReceived(friend.TitleDisplayName, friend.FriendPlayFabId);
                }
            }
            else
            {
                Debug.Log("No friends or pending friend requests found.");
            }
        }, error =>
        {
            Debug.LogError("Error retrieving friends list: " + error.GenerateErrorReport());
        });
    }


    // 3. Arkadaþlýk isteði alýndýðýnda UI güncellemesi
    public void OnFriendRequestReceived(string friendDisplayName, string friendPlayFabId)
    {
        // Prefab oluþtur
        GameObject newRequest = Instantiate(friendRequestPrefab, requestParentPanel);

        // Prefab'ýn içindeki Text ve Buton bileþenlerine eriþ
        Text displayNameText = newRequest.transform.Find("FriendNameText").GetComponent<Text>();
        Button acceptButton = newRequest.transform.Find("AcceptButton").GetComponent<Button>();
        Button declineButton = newRequest.transform.Find("DeclineButton").GetComponent<Button>();

        // Oyuncunun ismini Text bileþenine atayýn
        displayNameText.text = friendDisplayName;

        // Butonlara iþlev ekle
        acceptButton.onClick.RemoveAllListeners(); // Mevcut listener'larý kaldýr
        acceptButton.onClick.AddListener(() => AcceptFriendRequest(friendPlayFabId, newRequest));

        declineButton.onClick.RemoveAllListeners(); // Mevcut listener'larý kaldýr
        declineButton.onClick.AddListener(() => DeclineFriendRequest(newRequest));
    }


    // 4. Arkadaþlýk isteðini kabul etme
    public void AcceptFriendRequest(string friendPlayFabId, GameObject requestUI)
    {
        Debug.Log("Accept button clicked for: " + friendPlayFabId);

        PlayFabClientAPI.AddFriend(new AddFriendRequest { FriendPlayFabId = friendPlayFabId }, result =>
        {
            Debug.Log("Friend request accepted for " + friendPlayFabId);

            // Arkadaþ listeye eklendiðinde, arkadaþ listesi UI'ýna ekle
            AddFriendToFriendsList(friendPlayFabId);

            // Arkadaþlýk isteði UI'sini kaldýr
            Destroy(requestUI);
        }, error =>
        {
            Debug.LogError("Error accepting friend request: " + error.GenerateErrorReport());
            Destroy(requestUI);
        });
    }

    // 5. Arkadaþlýk isteðini reddetme
    public void DeclineFriendRequest(GameObject requestUI)
    {
        // Arkadaþlýk isteði UI'sini kaldýr
        Destroy(requestUI);
    }

    // 6. Arkadaþý arkadaþ listesi UI'ýna ekleme
    private void AddFriendToFriendsList(string friendDisplayName)
    {
        GameObject newFriendItem = Instantiate(friendListItemPrefab, friendsListContainer);
        Text friendNameText = newFriendItem.transform.Find("FriendNameText").GetComponent<Text>();
        friendNameText.text = friendDisplayName;
        Debug.Log("Friend " + friendDisplayName + " added to friends list.");
    }

    // 7. Arkadaþ listesini güncelleme (isteðe baðlý)
    public void GetFriendsList()
    {
        var request = new GetFriendsListRequest();

        PlayFabClientAPI.GetFriendsList(request, result =>
        {
            if (result.Friends != null && result.Friends.Count > 0)
            {
                foreach (var friend in result.Friends)
                {
                    Debug.Log("Friend: " + friend.TitleDisplayName);
                    AddFriendToFriendsList(friend.TitleDisplayName);
                }
            }
            else
            {
                Debug.Log("No friends found.");
            }
        }, error =>
        {
            Debug.LogError("Error retrieving friends list: " + error.GenerateErrorReport());
        });
    }
    // 8. Lobiye davet gönderme (isteðe baðlý)
    public void InviteFriendToLobby(string friendDisplayName)
    {
        // RPC veya Photon mesajlaþma sistemi kullanarak arkadaþýnýza bir davet gönderin
        Debug.Log("Invite sent to: " + friendDisplayName);
    }
}
