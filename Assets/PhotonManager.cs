using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class PhotonManager : MonoBehaviourPunCallbacks
{
    private const string FirstTimeKey = "FirstTime";
    private const string LastEquippedCharacterKey = "LastEquippedCharacter";
    private string[] spawnPoints = { "SpawnPoint1", "SpawnPoint2", "SpawnPoint3", "SpawnPoint4" };

    public Button playButton;
    public GameObject loadingPanel;
    public Text statusText;  // Maç arama ve geri sayým mesajý için Text
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
            playButton.onClick.AddListener(OnPlayButtonClicked);
        }
        else
        {
            Debug.LogError("Play button is not assigned.");
        }

        if (leaveRoomButton != null)
        {
            leaveRoomButton.onClick.AddListener(OnLeaveRoomButtonClicked);
            leaveRoomButton.gameObject.SetActive(false); // Baþlangýçta butonu pasif yap
        }
        else
        {
            Debug.LogError("Leave room button is not assigned.");
        }
    }

    private void Update()
    {
        if (PhotonNetwork.InRoom && PhotonNetwork.CurrentRoom.PlayerCount == PhotonNetwork.CurrentRoom.MaxPlayers)
        {
            photonView.RPC("ShowLoadingPanel", RpcTarget.AllBuffered);
            enabled = false;
            StartCountdown();
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
        leaveRoomButton.gameObject.SetActive(false); // Odadan çýkýldýðýnda butonu pasif yap
        statusText.text = "You have left the room."; // Oyuncuya odadan çýktýðýný bildir
        statusText.gameObject.SetActive(true); // Mesajý göster

        if (cosmeticsButton != null)
        {
            cosmeticsButton.interactable = true; // Odadan çýkýldýðýnda cosmetics butonunu tekrar aktif yap
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

        if (cosmeticsButton != null)
        {
            cosmeticsButton.interactable = false;
        }
        else
        {
            Debug.LogError("Cosmetics button is not assigned.");
        }

        string characterPrefabName = PlayerPrefs.GetString(LastEquippedCharacterKey, "DefaultCharacter");
        Vector3 spawnPosition = GetSpawnPosition();

        if (!string.IsNullOrEmpty(characterPrefabName) && spawnPosition != Vector3.zero)
        {
            GameObject character = PhotonNetwork.Instantiate(characterPrefabName, spawnPosition, Quaternion.identity);
            if (character != null)
            {
                character.transform.rotation = Quaternion.Euler(0, -115, 0);
                character.transform.localScale = Vector3.one * 0.3f;
            }
            else
            {
                Debug.LogError("Failed to instantiate character.");
            }
        }
        else
        {
            Debug.LogError("Character prefab name is empty or spawn position is invalid.");
        }
    }

    private void StartCountdown()
    {
        if (!isCountingDown)
        {
            isCountingDown = true;
            StartCoroutine(CountdownCoroutine(10));
        }
    }

    private IEnumerator HideStatusTextAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        statusText.gameObject.SetActive(false); // Mesajý gizle
    }

    [PunRPC]
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

        // Geri sayým bittiðinde yükleme panelini göstermek ve sahneyi deðiþtirmek için RPC çaðýr
        photonView.RPC("ShowLoadingPanel", RpcTarget.AllBuffered);

        // Bekleme süresi eklemek gerekebilir (opsiyonel)
        yield return new WaitForSeconds(1);

        // Sahneyi yükle
        PhotonNetwork.LoadLevel("TrapPG");
    }

    private Vector3 GetSpawnPosition()
    {
        List<string> availableSpawnPoints = new List<string>(spawnPoints);
        int playerIndex = PhotonNetwork.LocalPlayer.ActorNumber - 1; // PlayerIndex (0-based)

        if (playerIndex < availableSpawnPoints.Count)
        {
            string spawnPointName = availableSpawnPoints[playerIndex];
            GameObject spawnPoint = GameObject.Find(spawnPointName);

            if (spawnPoint != null)
            {
                if (spawnPoint.transform.childCount == 0)
                {
                    Debug.Log($"Spawn point '{spawnPointName}' is empty and available.");
                    return spawnPoint.transform.position;
                }
                else
                {
                    Debug.LogWarning($"Spawn point '{spawnPointName}' is not empty.");
                }
            }
            else
            {
                Debug.LogError($"Spawn point '{spawnPointName}' not found in the scene.");
            }
        }
        else
        {
            Debug.LogError("Player index is out of bounds.");
        }

        Debug.LogError("No available spawn points.");
        return Vector3.zero;
    }
}
