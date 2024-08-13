using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class PhotonManager : MonoBehaviourPunCallbacks
{
    private const string FirstTimeKey = "FirstTime";
    private const string LastEquippedCharacterKey = "LastEquippedCharacter";
    private string[] spawnPoints = { "SpawnPoint1", "SpawnPoint2", "SpawnPoint3", "SpawnPoint4" };
    private Dictionary<int, string> playerSpawnPoints = new Dictionary<int, string>();
    private HashSet<string> occupiedSpawnPoints = new HashSet<string>();

    public Button playButton;
    public GameObject loadingPanel;
    public Text statusText;
    private bool isCountingDown = false;
    public Button leaveRoomButton;
    public Button cosmeticsButton;
    public GridManager gridManager; // GridManager referansý
    private GameObject currentCharacter;

    private void Start()
    {
        ShowLastEquippedCharacter();

        if (!PhotonNetwork.IsConnected)
        {
            PhotonNetwork.ConnectUsingSettings();
        }
        else
        {
            Debug.Log("Already connected to Photon.");
        }

        if (playButton != null)
        {
            playButton.interactable = true;
            playButton.onClick.AddListener(OnPlayButtonClicked);
        }
        else
        {
            Debug.LogError("Play button is not assigned.");
        }

        if (leaveRoomButton != null)
        {
            leaveRoomButton.onClick.AddListener(OnLeaveRoomButtonClicked);
            leaveRoomButton.gameObject.SetActive(false);
        }
        else
        {
            Debug.LogError("Leave room button is not assigned.");
        }
    }
    private void ShowLastEquippedCharacter()
    {
        string characterPrefabName = PlayerPrefs.GetString(LastEquippedCharacterKey, "DefaultCharacter");

        if (!string.IsNullOrEmpty(characterPrefabName))
        {
            GameObject characterPrefab = Resources.Load<GameObject>(characterPrefabName);
            if (characterPrefab != null)
            {
                Vector3 spawnPosition = new Vector3(19.7f, -4.75f, 79.25f); // Sahneye göre pozisyonu ayarlayýn
                currentCharacter = Instantiate(characterPrefab, spawnPosition, Quaternion.identity);

                if (currentCharacter != null)
                {
                    currentCharacter.transform.rotation = Quaternion.Euler(0, 10, 0);

                    // Eðer bu oyunda oyuncu karakteri için özel ayarlar yapmanýz gerekiyorsa, buraya ekleyin
                }
                else
                {
                    Debug.LogError("Failed to instantiate character.");
                }
            }
            else
            {
                Debug.LogError("Character prefab not found in Resources.");
            }
        }
        else
        {
            Debug.LogError("Character prefab name is empty.");
        }
    }

    private void OnPlayButtonClicked()
    {
        Play();
    }

    public void Play()
    {
        if (PlayerPrefs.GetInt(FirstTimeKey, 1) == 1)
        {
            PlayerPrefs.SetInt(FirstTimeKey, 0);
            ShowCharacterShop();
        }
        else
        {
            ConnectToLobby();
        }
    }

    private void OnLeaveRoomButtonClicked()
    {
        LeaveRoom();
    }

    public void LeaveRoom()
    {
        if (PhotonNetwork.InRoom)
        {
            PhotonNetwork.LeaveRoom();
        }
        else
        {
            Debug.LogWarning("Not currently in a room to leave.");
        }
    }

    public override void OnLeftRoom()
    {
        ShowLastEquippedCharacter();
        Debug.Log("Left room successfully.");
        leaveRoomButton.gameObject.SetActive(false);
        playButton.interactable = true;
        statusText.text = "You have left the room.";
        statusText.gameObject.SetActive(true);

        if (cosmeticsButton != null)
        {
            cosmeticsButton.interactable = true;
        }

        StartCoroutine(HideStatusTextAfterDelay(1f));
    }

    private void ShowCharacterShop()
    {
        SceneManager.LoadScene("CharacterShop");
    }

    public void ConnectToLobby()
    {
        if (PhotonNetwork.IsConnected)
        {
            if (currentCharacter != null)
            {
                Destroy(currentCharacter);
            }
            PhotonNetwork.JoinLobby();
            statusText.text = "Searching for matchmaking...";
            statusText.gameObject.SetActive(true);
        }
        else
        {
            Debug.LogWarning("Photon is not connected.");
        }
    }

    public override void OnConnectedToMaster()
    {
        Debug.Log("Connected to Photon Master Server.");
    }

    public override void OnJoinedLobby()
    {
        Debug.Log("Joined Lobby.");

        RoomOptions roomOptions = new RoomOptions { MaxPlayers = 2 };
        PhotonNetwork.JoinOrCreateRoom("LobbyRoom", roomOptions, TypedLobby.Default);
    }

    public override void OnJoinedRoom()
    {
        Debug.Log("Joined Room.");
        leaveRoomButton.gameObject.SetActive(true);
        playButton.interactable = false;

        if (cosmeticsButton != null)
        {
            cosmeticsButton.interactable = false;
        }
        else
        {
            Debug.LogError("Cosmetics button is not assigned.");
        }

        string characterPrefabName = PlayerPrefs.GetString(LastEquippedCharacterKey, "DefaultCharacter");

        if (!string.IsNullOrEmpty(characterPrefabName))
        {
            // GridManager'dan spawn pozisyonu al
            Vector3 spawnPosition = gridManager.GetNextSpawnPosition(PhotonNetwork.LocalPlayer.ActorNumber - 1);

            GameObject character = PhotonNetwork.Instantiate(characterPrefabName, spawnPosition, Quaternion.identity);

            if (character != null)
            {
                character.transform.rotation = Quaternion.Euler(0, 10, 0);

                if (PhotonNetwork.IsMasterClient)
                {
                    Transform crown = character.transform.Find("Crown"); // Tacýn adýný burada belirttiðiniz adla deðiþtirin
                    if (crown != null)
                    {
                        crown.gameObject.SetActive(true);
                    }
                    else
                    {
                        Debug.LogError("Crown object not found on character.");
                    }
                }
            }
            else
            {
                Debug.LogError("Failed to instantiate character.");
            }
        }
        else
        {
            Debug.LogError("Character prefab name is empty.");
        }

        // Oyun için geri sayým baþlat
        if (PhotonNetwork.CurrentRoom.PlayerCount == PhotonNetwork.CurrentRoom.MaxPlayers)
        {
            StartCountdown();
        }
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        Debug.Log($"Player {otherPlayer.NickName} has left the room.");

        // Ayrýlan oyuncunun spawn noktasýný serbest býrak
        if (playerSpawnPoints.ContainsKey(otherPlayer.ActorNumber))
        {
            string spawnPointName = playerSpawnPoints[otherPlayer.ActorNumber];

            if (!string.IsNullOrEmpty(spawnPointName) && occupiedSpawnPoints.Contains(spawnPointName))
            {
                occupiedSpawnPoints.Remove(spawnPointName);
                playerSpawnPoints.Remove(otherPlayer.ActorNumber);
                Debug.Log($"Spawn point '{spawnPointName}' is now available.");
            }
        }

        // Tüm oyunculara spawn point durumunu güncelle
        photonView.RPC("UpdateSpawnPointsRPC", RpcTarget.AllBuffered, string.Join(",", occupiedSpawnPoints.ToArray()));

        // Eðer geri sayým baþladýysa, geri sayýmý durdur
        if (isCountingDown && PhotonNetwork.CurrentRoom.PlayerCount < PhotonNetwork.CurrentRoom.MaxPlayers)
        {
            StopCountdown();
        }
    }

    [PunRPC]
    private void UpdateSpawnPointsRPC(string occupiedPoints)
    {
        occupiedSpawnPoints = new HashSet<string>(occupiedPoints.Split(','));
    }

    private void StartCountdown()
    {
        if (!isCountingDown)
        {
            isCountingDown = true;
            photonView.RPC("StartCountdownRPC", RpcTarget.AllBuffered, 10);
        }
    }

    private void StopCountdown()
    {
        isCountingDown = false;
        StopAllCoroutines();
        statusText.text = "Waiting for more players...";
    }

    [PunRPC]
    private void StartCountdownRPC(int seconds)
    {
        StartCoroutine(CountdownCoroutine(seconds));
    }

    private IEnumerator CountdownCoroutine(int seconds)
    {
        while (seconds > 0)
        {
            statusText.text = $"Match found! Starting in {seconds}...";
            yield return new WaitForSeconds(1);
            seconds--;
        }

        ShowLoadingPanel();
        yield return new WaitForSeconds(3);
        PhotonNetwork.LoadLevel("TrapPG");
    }

    private IEnumerator HideStatusTextAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        statusText.gameObject.SetActive(false);
    }

    void ShowLoadingPanel()
    {
        loadingPanel.SetActive(true);
        statusText.gameObject.SetActive(false);
    }
    
}
