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
    public Transform[] spawnPoints;  // 4 adet spawn noktasý
    private bool[] occupiedSpawns;
    private Dictionary<int, int> playerSpawnMap;


    public Button playButton;
    public GameObject loadingPanel;
    public Text statusText;
    public Button leaveRoomButton;
    public Button cosmeticsButton;
    public GridManager gridManager; // GridManager referansý
    private GameObject currentCharacter;
    public GameObject crownPrefab;
    private GameObject currentCrown; // Taç objesini takip etmek için
    private int countdownTime = 10; // Geri sayým süresi
    private Coroutine countdownCoroutine;
    private bool isCountdownActive = false;

    private void Start()
    {
        occupiedSpawns = new bool[spawnPoints.Length]; // Baþlangýçta tüm spawn noktalarý boþ
        playerSpawnMap = new Dictionary<int, int>(); // Oyuncularýn spawn noktalarýný tutan dictionary
        SpawnExistingPlayers();

        PhotonNetwork.AutomaticallySyncScene = true;
        Debug.Log($"{PhotonNetwork.NickName}");
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

        RoomOptions roomOptions = new RoomOptions { MaxPlayers = 4 };
        PhotonNetwork.JoinOrCreateRoom("LobbyRoom", roomOptions, TypedLobby.Default);
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
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
            Vector3 spawnPosition = GetNextAvailableSpawnPoint(newPlayer.ActorNumber);
            GameObject character = PhotonNetwork.Instantiate(characterPrefabName, spawnPosition, Quaternion.identity);

            if (character != null)
            {
                character.transform.rotation = Quaternion.Euler(0, 10, 0);
                PhotonNetwork.LocalPlayer.TagObject = character;

                if (PhotonNetwork.IsMasterClient)
                {
                    AssignCrownToMasterClient(character);
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

        Debug.Log($"{PhotonNetwork.CurrentRoom.PlayerCount}");
        if (PhotonNetwork.CurrentRoom.PlayerCount == PhotonNetwork.CurrentRoom.MaxPlayers)
        {
            if (!isCountdownActive)
            {
                leaveRoomButton.interactable = false;
                countdownCoroutine = StartCoroutine(StartCountdown());
            }

        }
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        Debug.Log($"Player {otherPlayer.NickName} has left the room.");
        ReleaseSpawnPoint(otherPlayer.ActorNumber);
        if (PhotonNetwork.CurrentRoom.PlayerCount < PhotonNetwork.CurrentRoom.MaxPlayers)
        {
            if (isCountdownActive && PhotonNetwork.IsMasterClient)
            {
                StopCountdown();
            }
        }
    }
    Vector3 GetNextAvailableSpawnPoint(int playerID)
    {
        // Boþ bir spawn noktasý bul ve onu iþgal et
        for (int i = 0; i < spawnPoints.Length; i++)
        {
            if (!occupiedSpawns[i])
            {
                occupiedSpawns[i] = true;
                playerSpawnMap[playerID] = i; // Oyuncu ID'si ile spawn noktasý eþleþmesini sakla
                return spawnPoints[i].position;
            }
        }
        return Vector3.zero; // Eðer boþ bir spawn noktasý yoksa
    }
    void ReleaseSpawnPoint(int playerID)
    {
        // Oyuncunun spawn noktasýný boþalt
        if (playerSpawnMap.ContainsKey(playerID))
        {
            int spawnIndex = playerSpawnMap[playerID]; // Oyuncunun hangi spawn noktasýnda olduðunu bul
            occupiedSpawns[spawnIndex] = false; // O spawn noktasýný boþalt
            playerSpawnMap.Remove(playerID); // Oyuncuyu dictionary'den kaldýr
        }
    }
    void SpawnExistingPlayers()
    {
        // Zaten lobide olan oyuncularý spawn et
        foreach (Player player in PhotonNetwork.PlayerList)
        {
            string characterPrefabName = PlayerPrefs.GetString(LastEquippedCharacterKey, "DefaultCharacter");

            if (player != PhotonNetwork.LocalPlayer && !string.IsNullOrEmpty(characterPrefabName)) // Kendini iki kere spawn etmemek için kontrol
            {
                Vector3 spawnPosition = GetNextAvailableSpawnPoint(player.ActorNumber);
                GameObject character = PhotonNetwork.Instantiate(characterPrefabName, spawnPosition, Quaternion.identity);
            }
        }
    }
    int GetPlayerSpawnIndex(Player player)
    {
        // Oyuncunun hangi spawn noktasýna yerleþtirildiðini bul
        // Bu fonksiyonu her oyuncu için spawn indexlerini saklayarak yapabilirsin
        // Burada yerel veri yapýlarýyla bunu yapman önerilir (örneðin bir dictionary ile)
        // Bu örnekte basitlik adýna false dönüþ yapýyoruz.
        return -1;
    }
    IEnumerator StartCountdown()
    {
        isCountdownActive = true;
        // Geri sayýmý baþlat
        while (countdownTime > 0)
        {
            // Geri sayým süresini tüm oyunculara gönder
            photonView.RPC("UpdateCountdown", RpcTarget.All, countdownTime);

            yield return new WaitForSeconds(1f);
            countdownTime--;
        }

        photonView.RPC("ShowLoadingPanel", RpcTarget.All);

        yield return new WaitForSeconds(3f);
        // Geri sayým bittiðinde oyunu baþlat
        photonView.RPC("StartGame", RpcTarget.All);
    }

    [PunRPC]
    void StopCountdown()
    {
        // Geri sayýmý durdur
        if (countdownCoroutine != null)
        {
            StopCoroutine(countdownCoroutine);
        }

        // Geri sayým durumunu ve zamanlayýcýyý sýfýrla
        isCountdownActive = false;
        countdownTime = 10;

        // Geri sayýmý durdurduðunuzu tüm oyunculara bildirin
        photonView.RPC("UpdateCountdown", RpcTarget.All, 0);
    }

    [PunRPC]
    void UpdateCountdown(int time)
    {
        if (time > 0)
        {
            leaveRoomButton.interactable = false;
            statusText.text = $"Starting game in {time} seconds...";
        }
        else
        {
            leaveRoomButton.interactable=true;
            statusText.text = "Waiting for more players...";
        }
    }

    [PunRPC]
    void ShowLoadingPanel()
    {
        if (loadingPanel != null)
        {
            loadingPanel.SetActive(true);
        }
    }

    [PunRPC]
    void StartGame()
    {
        PhotonNetwork.LoadLevel("TrapPG"); // Oyununuzu baþlatacak sahneyi yükleyin
    }

    public override void OnMasterClientSwitched(Player newMasterClient)
    {
        Debug.Log($"New Master Client: {newMasterClient.NickName}");

        if (PhotonNetwork.LocalPlayer.ActorNumber == newMasterClient.ActorNumber)
        {
            // Yeni lobi kurucusu (Master Client) siz oldunuz
            Debug.Log("I am the new Master Client.");

            // Tacý yeni lobi kurucusuna ata
            AssignCrownToMasterClient((GameObject)newMasterClient.TagObject);

            // Geri sayým baþlatýlmamýþsa ve odadaki oyuncu sayýsý maksimum kapasiteye ulaþmýþsa geri sayýmý baþlat
            if (PhotonNetwork.CurrentRoom.PlayerCount == PhotonNetwork.CurrentRoom.MaxPlayers)
            {
                if (!isCountdownActive)
                {
                    leaveRoomButton.interactable = false;
                    countdownCoroutine = StartCoroutine(StartCountdown());
                }
            }
            else
            {
                leaveRoomButton.interactable = true;
                StopCountdown();
            }
        }
    }

    private void AssignCrownToMasterClient(GameObject masterClientCharacter)
    {
        if (currentCrown != null)
        {
            Destroy(currentCrown); // Mevcut tacý yok et
        }

        if (masterClientCharacter != null)
        {
            // Taç objesini tüm oyunculara görünür hale getirin
            Vector3 crownPosition = masterClientCharacter.transform.position + new Vector3(0, 2.5f, 0); // Tacý karakterin üzerinde konumlandýr
            currentCrown = PhotonNetwork.Instantiate(crownPrefab.name, crownPosition, Quaternion.identity);
            currentCrown.transform.SetParent(masterClientCharacter.transform); // Tacý karaktere baðla

            // Tacý master client karakterinin PhotonView'ine senkronize et
            PhotonView masterClientPhotonView = masterClientCharacter.GetComponent<PhotonView>();
            if (masterClientPhotonView != null)
            {
                masterClientPhotonView.RPC("SetCrown", RpcTarget.AllBuffered, currentCrown.GetComponent<PhotonView>().ViewID);
            }
        }
    }
    [PunRPC]
    void SetCrown(int crownViewID)
    {
        PhotonView crownPhotonView = PhotonView.Find(crownViewID);
        if (crownPhotonView != null)
        {
            GameObject crown = crownPhotonView.gameObject;
            if (crown != null)
            {
                // Taçý oyuncunun karakterine baðla
                GameObject playerCharacter = (GameObject)PhotonNetwork.LocalPlayer.TagObject;
                if (playerCharacter != null)
                {
                    crown.transform.SetParent(playerCharacter.transform);
                }
            }
        }
    }

    IEnumerator HideStatusTextAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        statusText.gameObject.SetActive(false);
    }
}
