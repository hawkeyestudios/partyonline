using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.TextCore.Text;

public class PhotonManager : MonoBehaviourPunCallbacks
{
    private const string FirstTimeKey = "FirstTime";
    private const string LastEquippedCharacterKey = "LastEquippedCharacter";

    public Button playButton;
    public GameObject loadingPanel;
    public Text statusText;
    public Button leaveRoomButton;
    public Button cosmeticsButton;
    public GridManager gridManager;
    private GameObject currentCharacter;
    public GameObject crownPrefab;
    private GameObject currentCrown; 
    private int countdownTime = 10; 
    private Coroutine countdownCoroutine;
    private bool isCountdownActive = false;
    //private string[] allMaps = { "TrapPG", "GhostPG", "TntPG", "CrownPG", "SumoPG", "RoulettePG"};
    public List<GameObject> loadingPanels;

    public Color[] playerColors = { Color.red, new Color(0.25f, 0.88f, 0.82f), Color.yellow, new Color(0.63f, 0.13f, 0.94f) };
    private void Start()
    {
        //PhotonNetwork.AutomaticallySyncScene = true;
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
        if (currentCharacter != null)
        {
            Debug.Log("Son kullan�lan karakter zaten instantiate edilmi�.");
            return;
        }

        string characterPrefabName = PlayerPrefs.GetString("LastEquippedCharacter", "DefaultCharacter");

        if (!string.IsNullOrEmpty(characterPrefabName))
        {
            GameObject characterPrefab = Resources.Load<GameObject>(characterPrefabName);
            if (characterPrefab != null)
            {
                Vector3 spawnPosition = new Vector3(19.7f, -4.75f, 79.25f);
                currentCharacter = Instantiate(characterPrefab, spawnPosition, Quaternion.identity);

                if (currentCharacter != null)
                {
                    currentCharacter.transform.rotation = Quaternion.Euler(0, 10, 0);
                    Renderer circleRenderer = currentCharacter.transform.Find("Circle").GetComponent<Renderer>();
                    if (circleRenderer != null)
                    {
                        circleRenderer.enabled = false;
                    }
                }
                else
                {
                    Debug.LogError("Karakter instantiate edilemedi.");
                }
            }
            else
            {
                Debug.LogError($"'Resources' klas�r�nde '{characterPrefabName}' adl� karakter prefab'� bulunamad�.");
            }
        }
        else
        {
            Debug.LogError("Karakter prefab ad� bo�.");
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
        SceneManager.LoadScene("Customize");
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
        RoomOptions roomOptions = new RoomOptions { MaxPlayers = 2};                            //Lobi ki�i say�s�
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

        Transform spawnPoint = gridManager.GetRandomSpawnPoint();

        if (spawnPoint != null)
        {
            string characterPrefabName = PlayerPrefs.GetString("LastEquippedCharacter", "DefaultCharacter");

            if (!string.IsNullOrEmpty(characterPrefabName))
            {
                GameObject character = PhotonNetwork.Instantiate(characterPrefabName, spawnPoint.position, spawnPoint.rotation);

                if (character != null)
                {
                    character.transform.rotation = Quaternion.Euler(0, 10, 0);
                    PhotonNetwork.LocalPlayer.TagObject = character;

                    if (PhotonNetwork.IsMasterClient)
                    {
                        AssignCrownToMasterClient(character);
                    }

                    int playerIndex = PhotonNetwork.LocalPlayer.ActorNumber - 1;
                    //Renderer circleRenderer = character.transform.Find("Circle").GetComponent<Renderer>();
                    //if (circleRenderer != null)
                    //{
                        //circleRenderer.enabled = false;
                    //}
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
        }
        else
        {
            Debug.LogError("No available spawn point.");
        }

        Debug.Log($"{PhotonNetwork.CurrentRoom.PlayerCount}");
        if (PhotonNetwork.CurrentRoom.PlayerCount == PhotonNetwork.CurrentRoom.MaxPlayers)
        {
            if (!isCountdownActive)
            {
                leaveRoomButton.interactable = false;
                photonView.RPC("StartGame", RpcTarget.AllBuffered);
                Debug.Log("Geri say�m ba�lad�");
            }
        }
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        Debug.Log($"Player {otherPlayer.NickName} has left the room.");

        GameObject playerObject = (GameObject)otherPlayer.TagObject;
        if (playerObject != null)
        {
            Transform spawnPoint = playerObject.transform;
            gridManager.ReleaseSpawnPoint(spawnPoint);
        }

        if (PhotonNetwork.CurrentRoom.PlayerCount < PhotonNetwork.CurrentRoom.MaxPlayers)
        {
            if (isCountdownActive)
            {
                StopCountdown();
            }
        }
    }

    IEnumerator StartCountdown()
    {
        isCountdownActive = true;
        Debug.Log("Countdown ba�lad�."); 
        while (countdownTime > 0)
        {
            Debug.Log("Countdown: " + countdownTime);
            UpdateCountdown(countdownTime);
            yield return new WaitForSeconds(1f);
            countdownTime--;
        }

        Debug.Log("Countdown finished, starting game...");

        if (PhotonNetwork.IsMasterClient)
        {
            photonView.RPC("StartGameRPC", RpcTarget.All);
        }
    }
    [PunRPC]
    void StartGame()
    {
         StartCoroutine(StartCountdown());
    }

    [PunRPC]
    void StopCountdown()
    {
        if (countdownCoroutine != null)
        {
            StopCoroutine(countdownCoroutine);
        }

        isCountdownActive = false;
        countdownTime = 10;
        photonView.RPC("UpdateCountdown", RpcTarget.All, 0);
    }

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
    void StartGameRPC()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            string[] allMaps = { "TrapPG", "GhostPG", "TntPG", "CrownPG", "SumoPG", "RoulettePG" };
            List<string> selectedMaps = allMaps.OrderBy(x => Random.Range(0, 100)).Take(4).ToList();

            string selectedMapsString = string.Join(",", selectedMaps);
            photonView.RPC("ReceiveMapSelection", RpcTarget.All, selectedMapsString);
        }
    }

    [PunRPC]
    void ReceiveMapSelection(string selectedMapsString)
    {
        List<string> selectedMaps = selectedMapsString.Split(',').ToList();

        PhotonNetwork.LoadLevel(selectedMaps[0]);

        PlayerPrefs.SetString("SelectedMaps", string.Join(",", selectedMaps));
        PlayerPrefs.SetInt("currentMapIndex", 0);
        PlayerPrefs.Save();
    }
    public override void OnMasterClientSwitched(Player newMasterClient)
    {
        Debug.Log($"New Master Client: {newMasterClient.NickName}");

        if (PhotonNetwork.LocalPlayer.ActorNumber == newMasterClient.ActorNumber)
        {
            Debug.Log("I am the new Master Client.");

            AssignCrownToMasterClient((GameObject)newMasterClient.TagObject);

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
            Destroy(currentCrown);
        }

        if (masterClientCharacter != null)
        {
            Vector3 crownPosition = masterClientCharacter.transform.position + new Vector3(0, 2.5f, 0); 
            currentCrown = PhotonNetwork.Instantiate(crownPrefab.name, crownPosition, Quaternion.identity);
            currentCrown.transform.SetParent(masterClientCharacter.transform); 

            PhotonView masterClientPhotonView = masterClientCharacter.GetComponent<PhotonView>();
            if (masterClientPhotonView != null)
            {
                masterClientPhotonView.RPC("SetCrown", RpcTarget.AllBuffered, currentCrown.GetComponent<PhotonView>().ViewID);
            }
        }
    }

    [PunRPC]
    public void SetCrown(int crownViewID)
    {
        PhotonView crownPhotonView = PhotonView.Find(crownViewID);
        if (crownPhotonView != null)
        {
            GameObject crown = crownPhotonView.gameObject;
            if (crown != null)
            {
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
