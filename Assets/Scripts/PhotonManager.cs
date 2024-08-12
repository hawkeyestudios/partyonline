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

    private void Start()
    {
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
        Debug.Log("Left room successfully.");
        leaveRoomButton.gameObject.SetActive(false);
        playButton.interactable = true;
        statusText.text = "You have left the room.";
        statusText.gameObject.SetActive(true);

        if (cosmeticsButton != null)
        {
            cosmeticsButton.interactable = true;
        }

        StartCoroutine(HideStatusTextAfterDelay(1.5f));

    }

    private void ShowCharacterShop()
    {
        SceneManager.LoadScene("CharacterShop");
    }

    public void ConnectToLobby()
    {
        if (PhotonNetwork.IsConnected)
        {
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

        RoomOptions roomOptions = new RoomOptions { MaxPlayers = 4 };
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

        // Spawn point seçimi ve atama
        string assignedSpawnPoint = GetAssignedSpawnPoint();
        if (!string.IsNullOrEmpty(characterPrefabName) && assignedSpawnPoint != null)
        {
            GameObject spawnPoint = GameObject.Find(assignedSpawnPoint);
            if (spawnPoint != null)
            {
                Vector3 spawnPosition = spawnPoint.transform.position;
                GameObject character = PhotonNetwork.Instantiate(characterPrefabName, spawnPosition, Quaternion.identity);
                if (character != null)
                {
                    character.transform.rotation = Quaternion.Euler(0, 10, 0);

                    playerSpawnPoints[PhotonNetwork.LocalPlayer.ActorNumber] = assignedSpawnPoint;
                    occupiedSpawnPoints.Add(assignedSpawnPoint);
                }
                else
                {
                    Debug.LogError("Failed to instantiate character.");
                }
            }
        }
        else
        {
            Debug.LogError("Character prefab name is empty or no available spawn points.");
        }
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        Debug.Log($"Player {otherPlayer.NickName} has left the room.");

        // Ayrýlan oyuncunun spawn noktasýný serbest býrak
        if (playerSpawnPoints.ContainsKey(otherPlayer.ActorNumber))
        {
            string spawnPointName = playerSpawnPoints[otherPlayer.ActorNumber];

            if (spawnPointName != null && occupiedSpawnPoints.Contains(spawnPointName))
            {
                occupiedSpawnPoints.Remove(spawnPointName);
                playerSpawnPoints.Remove(otherPlayer.ActorNumber);
                Debug.Log($"Spawn point '{spawnPointName}' is now available.");
            }
        }

        // Tüm oyunculara spawn point durumunu güncelle
        photonView.RPC("UpdateSpawnPointsRPC", RpcTarget.AllBuffered, occupiedSpawnPoints.ToArray());
    }

    private string GetAssignedSpawnPoint()
    {
        // Daha önce atanmýþ bir spawn point varsa, onu tekrar kullan
        if (playerSpawnPoints.ContainsKey(PhotonNetwork.LocalPlayer.ActorNumber))
        {
            string previousSpawnPoint = playerSpawnPoints[PhotonNetwork.LocalPlayer.ActorNumber];
            if (!occupiedSpawnPoints.Contains(previousSpawnPoint))
            {
                return previousSpawnPoint;
            }
        }

        // Boþ bir spawn point bul ve ata
        foreach (string spawnPointName in spawnPoints)
        {
            if (!occupiedSpawnPoints.Contains(spawnPointName))
            {
                return spawnPointName;
            }
        }

        Debug.LogError("No available spawn points.");
        return null;
    }

    [PunRPC]
    private void UpdateSpawnPointsRPC(string[] occupiedPoints)
    {
        occupiedSpawnPoints = new HashSet<string>(occupiedPoints);
    }

    private void StartCountdown()
    {
        if (!isCountingDown)
        {
            isCountingDown = true;
            photonView.RPC("StartCountdownRPC", RpcTarget.AllBuffered, 10);
        }
    }

    [PunRPC]
    private void StartCountdownRPC(int seconds)
    {
        StartCoroutine(CountdownCoroutine(seconds));
    }

    private IEnumerator HideStatusTextAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        statusText.gameObject.SetActive(false);
    }

    void ShowLoadingPanel()
    {
        if (loadingPanel != null)
        {
            loadingPanel.SetActive(true);
        }
        else
        {
            Debug.LogError("Loading panel is not assigned.");
        }
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
}
