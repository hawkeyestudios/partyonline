using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PhotonManager : MonoBehaviourPunCallbacks
{
    private const string FirstTimeKey = "FirstTime";
    private const string LastEquippedCharacterKey = "LastEquippedCharacter";
    private string[] spawnPoints = { "SpawnPoint1", "SpawnPoint2", "SpawnPoint3", "SpawnPoint4" };

    public Button playButton;
    public GameObject loadingPanel;
    public Text statusText;  // Maç arama ve geri sayým mesajý için Text
    private bool isCountingDown = false;

    private void Start()
    {
        if (!PhotonNetwork.IsConnected)
        {
            PhotonNetwork.ConnectUsingSettings();
        }
        else
        {
            Debug.Log("Already connected to Photon.");
            ConnectToLobby();
            OnJoinedRoom();
        }

        if (playButton != null)
        {
            playButton.onClick.AddListener(OnPlayButtonClicked);
        }
        else
        {
            Debug.LogError("Play button is not assigned.");
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

        if (PhotonNetwork.CurrentRoom.PlayerCount == PhotonNetwork.CurrentRoom.MaxPlayers)
        {
            Debug.Log("Room is full. Starting game...");
            loadingPanel.SetActive(true);
            StartCountdown();
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

    private System.Collections.IEnumerator CountdownCoroutine(int seconds)
    {
        while (seconds > 0)
        {
            statusText.text = $"Match found! Starting in {seconds}...";
            yield return new WaitForSeconds(1);
            seconds--;
        }
        PhotonNetwork.LoadLevel("TrapPG");
    }

    private Vector3 GetSpawnPosition()
    {
        int playerIndex = PhotonNetwork.LocalPlayer.ActorNumber - 1;

        if (playerIndex >= 0 && playerIndex < spawnPoints.Length)
        {
            string spawnPointName = spawnPoints[playerIndex];
            GameObject spawnPoint = GameObject.Find(spawnPointName);
            if (spawnPoint != null && spawnPoint.transform.childCount == 0)
            {
                return spawnPoint.transform.position;
            }
        }

        Debug.LogError("No available spawn points.");
        return Vector3.zero;
    }
}
