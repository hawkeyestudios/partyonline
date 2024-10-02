using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using PlayFab;
using PlayFab.ClientModels;
using Photon.Realtime;

public class LobbyInviteManager : MonoBehaviourPunCallbacks
{
    // UI Elements
    public InputField friendNicknameInput;
    public Button inviteButton;
    public Text messageText;

    // Room options
    public string defaultRoomName = "DefaultLobby";

    private void Start()
    {
        // Set button click event
        inviteButton.onClick.AddListener(() => SendLobbyInvite());

        // Join or create lobby automatically on start
        CreateOrJoinLobby();
    }

    // Create or join a lobby
    void CreateOrJoinLobby()
    {
        RoomOptions roomOptions = new RoomOptions();
        roomOptions.MaxPlayers = 4; // Lobiye en fazla 4 oyuncu
        PhotonNetwork.JoinOrCreateRoom(defaultRoomName, roomOptions, TypedLobby.Default);
    }

    // Send an invite to the entered nickname
    public void SendLobbyInvite()
    {
        string friendNickname = friendNicknameInput.text;

        if (string.IsNullOrEmpty(friendNickname))
        {
            messageText.text = "Lütfen bir arkadaşın nickname'ini giriniz.";
            return;
        }

        // Check if the friend is already in the lobby
        foreach (Player player in PhotonNetwork.PlayerList)
        {
            if (player.NickName == friendNickname)
            {
                messageText.text = $"{friendNickname} zaten lobide.";
                return;
            }
        }

        // Use PlayFab to get friend PlayFab ID from nickname and send invite
        PlayFabClientAPI.GetFriendsList(new GetFriendsListRequest(), result =>
        {
            var friend = result.Friends.Find(f => f.TitleDisplayName == friendNickname);
            if (friend != null)
            {
                // PlayFab üzerinden push notification veya özel sistemle davet gönderebilirsiniz.
                messageText.text = $"{friendNickname} adlı kişiye davet gönderildi.";
            }
            else
            {
                messageText.text = $"{friendNickname} arkadaş listesinde bulunamadı.";
            }
        }, error =>
        {
            messageText.text = $"Hata: {error.ErrorMessage}";
        });
    }

    // Callback when joined room
    public override void OnJoinedRoom()
    {
        messageText.text = "Lobiye katıldınız: " + PhotonNetwork.CurrentRoom.Name;
    }

    // Callback when someone joins the room
    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        messageText.text = $"{newPlayer.NickName} lobiye katıldı.";
    }
}
