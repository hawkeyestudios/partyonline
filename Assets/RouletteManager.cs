using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Collections;
using System.Linq;

public class RouletteManager : MonoBehaviourPunCallbacks
{
    public Transform[] spawnPoints;
    public Transform gunSpawnPointObject;
    public Image[] profileImages;
    public Text[] rewardTexts;
    public Text[] nickNames;
    public Text[] gameOverNickNames;
    public GameObject gameOverPanel;
    public Button[] playerButtons;

    private List<Player> finishOrder = new List<Player>();
    private bool raceFinished = false;

    private int currentPlayerIndex = 0;
    private int[] bullets = new int[6];
    private int bulletIndex;
    private int totalPlayers;

    public Color[] playerColors = { Color.red, new Color(0.25f, 0.88f, 0.82f), Color.yellow, new Color(0.63f, 0.13f, 0.94f) };
    public GameObject gun;
    public Image bulletImage;
    public Text bulletCountText;
    public Text countdownText; 
    private Coroutine countdownCoroutine;

    void Start()
    {
        totalPlayers = PhotonNetwork.PlayerList.Length;
        if (PhotonNetwork.IsMasterClient)
        {
            InitializeBullets();
            bulletIndex = Random.Range(0, 6);
            bullets[bulletIndex] = 1;
            photonView.RPC("SyncBulletStatus", RpcTarget.All, bulletIndex);
        }
        UpdateBulletCountText();
        PhotonNetwork.AutomaticallySyncScene = true;
        gameOverPanel.SetActive(false);
        if (PhotonNetwork.IsMasterClient)
        {
            SpawnPlayersForAll();
        }
        SetPlayerProfileImage();
        SetPlayerNames();
        SetupPlayerButtons();
        GeriSayim.OnGameOver += GameOver_RPC;
        SetGunToPlayerHand(currentPlayerIndex);
        UpdateButtonInteractivity();
    }
    [PunRPC]
    private void SyncBulletStatus(int newBulletIndex)
    {
        InitializeBullets();
        bullets[newBulletIndex] = 1;
        bulletIndex = newBulletIndex;
    }
    private void ResetAndReloadBullets()
    {
        bulletImage.enabled = true;
        for (int i = 0; i < bullets.Length; i++)
        {
            bullets[i] = 0;
        }

        for (int i = 0; i < 2; i++)
        {
            int newBulletIndex;
            do
            {
                newBulletIndex = Random.Range(0, bullets.Length);
            } while (bullets[newBulletIndex] == 1);

            bullets[newBulletIndex] = 1;
        }

        bulletIndex = Random.Range(0, bullets.Length);
        UpdateBulletCountText();
    }
    private void SetupPlayerButtons()
    {
        for (int i = 0; i < playerButtons.Length; i++)
        {
            int index = i;
            playerButtons[i].onClick.AddListener(() => SelectPlayerAndShoot(index));
            playerButtons[i].interactable = false; 
        }
    }
    private void SetPlayerButtonsInteractable()
    {
        for (int i = 0; i < playerButtons.Length; i++)
        {
            playerButtons[i].interactable = (currentPlayerIndex == i);
        }
    }
    private void SelectPlayerAndShoot(int targetPlayerIndex)
    {
        if (currentPlayerIndex == targetPlayerIndex) return;

        // Ateþ etme iþlemi
        bool isBullet = (bullets[bulletIndex] == 1); // Eðer dolu mermi ise

        if (isBullet)
        {
            photonView.RPC("PlayerKilled_RPC", RpcTarget.All, targetPlayerIndex);
            if (PhotonNetwork.IsMasterClient) 
            {
                ResetAndReloadBullets();
                photonView.RPC("SyncBulletStatus", RpcTarget.All, bulletIndex);
            }
            UpdateBulletCountText();
        }
        UpdateCurrentPlayerIndex();
        UpdateButtonInteractivity();
        StartCountdown();
    }
    private void StartCountdown()
    {
        if (countdownCoroutine != null)
        {
            StopCoroutine(countdownCoroutine);
        }
        countdownCoroutine = StartCoroutine(CountdownCoroutine());
    }
    private IEnumerator CountdownCoroutine()
    {
        float countdownTime = 20f;

        while (countdownTime > 0)
        {
            countdownText.text = Mathf.Ceil(countdownTime).ToString();

           
            if (countdownTime <= 5)
            {
                countdownText.color = Color.red;
            }
            else
            {

            }

            countdownTime -= Time.deltaTime;
            yield return null;
        }

        photonView.RPC("PlayerKilled_RPC", RpcTarget.All, currentPlayerIndex);
    }
    private void UpdateBulletCountText()
    {
        int totalBullets = bullets.Length;
        int filledBullets = 0;

        foreach (var bullet in bullets)
        {
            if (bullet == 1) filledBullets++;
        }

        bulletCountText.text = $"{filledBullets}/{totalBullets}";
    }
    private void InitializeBullets()
    {
        for (int i = 0; i < bullets.Length; i++)
        {
            bullets[i] = 0;
        }
        bulletIndex = Random.Range(0, 6);
        bullets[bulletIndex] = 1;
    }
    private void UpdateCurrentPlayerIndex()
    {
        do
        {
            currentPlayerIndex = (currentPlayerIndex + 1) % totalPlayers;
        } while (IsPlayerDead(currentPlayerIndex));
    }

    private bool IsPlayerDead(int playerIndex)
    {
        Player player = PhotonNetwork.PlayerList[playerIndex];
        return finishOrder.Contains(player);
    }
    private void UpdateButtonInteractivity()
    {
        bool isCurrentPlayerTurn = (currentPlayerIndex == PhotonNetwork.LocalPlayer.ActorNumber - 1);

        for (int i = 0; i < playerButtons.Length; i++)
        {
            playerButtons[i].interactable = isCurrentPlayerTurn;
        }
    }
    [PunRPC]
    private void PlayerKilled_RPC(int playerIndex)
    {
        Player player = PhotonNetwork.PlayerList[playerIndex];
        finishOrder.Add(player);

        int reward = 250 * finishOrder.Count;
        UpdatePlayerUI_RPC(player.ActorNumber, reward);

        if (player.TagObject != null)
        {
            PhotonNetwork.Destroy(player.TagObject as GameObject);
        }

        if (playerIndex < playerButtons.Length)
        {
            playerButtons[playerIndex].gameObject.SetActive(false);
        }

        UpdateCurrentPlayerIndex();

        if (finishOrder.Count == totalPlayers - 1)
        {
            GameOver_RPC();
        }
    }
    private void SpawnPlayersForAll()
    {
        foreach (var player in PhotonNetwork.PlayerList)
        {
            Transform gunSpawnPoint = gunSpawnPointObject;
            Vector3 gunSpawnPosition = gunSpawnPointObject.position;
            Quaternion gunSpawnRotation = gunSpawnPointObject.rotation;
            gun = PhotonNetwork.Instantiate(gun.name, gunSpawnPosition, gunSpawnRotation);

            SpawnPlayerForAll(player.ActorNumber);
        }
    }

    private void SpawnPlayerForAll(int playerActorNumber)
    {
        Player player = PhotonNetwork.PlayerList.FirstOrDefault(p => p.ActorNumber == playerActorNumber);

        if (player != null)
        {
            int spawnPointIndex = (player.ActorNumber - 1) % spawnPoints.Length;
            Transform spawnPoint = spawnPoints[spawnPointIndex];
            Vector3 spawnPosition = spawnPoint.position;
            Quaternion spawnRotation = spawnPoint.rotation;

            string characterPrefabName = PlayerPrefs.GetString("LastEquippedCharacter", "DefaultCharacter");

            if (!string.IsNullOrEmpty(characterPrefabName))
            {
                GameObject character = PhotonNetwork.Instantiate(characterPrefabName, spawnPosition, spawnRotation, 0);

                if (character != null)
                {
                    int playerIndex = player.ActorNumber - 1;
                    Renderer circleRenderer = character.transform.Find("Circle").GetComponent<Renderer>();
                    if (circleRenderer != null && playerIndex >= 0 && playerIndex < playerColors.Length)
                    {
                        circleRenderer.material.color = playerColors[playerIndex];
                    }
                    Debug.Log("Character successfully spawned.");
                }
                else
                {
                    Debug.LogError("Failed to instantiate character.");
                }
            }
            else
            {
                Debug.LogError("Character prefab not found in PlayerPrefs.");
            }
        }
    }

    private void SetPlayerProfileImage()
    {
        string playerImageName = PlayerPrefs.GetString("LastEquippedCharacter");
        ExitGames.Client.Photon.Hashtable playerProperties = new ExitGames.Client.Photon.Hashtable();
        playerProperties["profileImage"] = playerImageName;
        PhotonNetwork.LocalPlayer.SetCustomProperties(playerProperties);
    }

    private void SetPlayerNames()
    {
        for (int i = 0; i < PhotonNetwork.PlayerList.Length; i++)
        {
            nickNames[i].text = PhotonNetwork.PlayerList[i].NickName;
        }
    }

    private void SetGunToPlayerHand(int playerIndex)
    {
        Player player = PhotonNetwork.PlayerList[playerIndex];
        GameObject playerObject = player.TagObject as GameObject;

        if (playerObject != null)
        {
            Transform handTransform = playerObject.transform.Find("Hand");

            if (handTransform != null)
            {
                gun.transform.position = handTransform.position;
                gun.transform.rotation = handTransform.rotation;
                Debug.Log($"Gun moved to player {playerIndex}'s hand");
            }
            else
            {
                Debug.LogWarning("Hand object not found on player!");
            }
        }
    }
    private void UpdatePlayerRanking()
    {
        finishOrder.Sort((p1, p2) => finishOrder.IndexOf(p1).CompareTo(finishOrder.IndexOf(p2)));
    }
    [PunRPC]
    private void UpdatePlayerUI_RPC(int playerActorNumber, int reward)
    {
        Player player = PhotonNetwork.CurrentRoom.GetPlayer(playerActorNumber);
        if (player == null) return;

        int playerIndex = finishOrder.IndexOf(player);
        if (playerIndex >= 0 && playerIndex < profileImages.Length)
        {
            profileImages[playerIndex].sprite = GetProfileSprite(player);
            rewardTexts[playerIndex].text = $"{reward}";
            nickNames[playerIndex].text = player.NickName;
        }

        if (playerIndex >= 0 && playerIndex < gameOverNickNames.Length)
        {
            gameOverNickNames[playerIndex].text = player.NickName;
        }
    }
    private Sprite GetProfileSprite(Player player)
    {
        if (player.CustomProperties.TryGetValue("profileImage", out object playerImageName))
        {
            Sprite profileSprite = Resources.Load<Sprite>("ProfileImages/" + playerImageName.ToString());

            if (profileSprite == null)
            {
                profileSprite = Resources.Load<Sprite>("ProfileImages/defaultProfileImage");
            }

            return profileSprite;
        }
        else
        {
            return Resources.Load<Sprite>("ProfileImages/defaultProfileImage");
        }
    }

    [PunRPC]
    private void GameOver_RPC()
    {
        raceFinished = true;
        gameOverPanel.SetActive(true);

        foreach (Player player in PhotonNetwork.PlayerList)
        {
            if (!finishOrder.Contains(player))
            {
                finishOrder.Add(player);
                int playerScore = 1000;
                UpdatePlayerUI_RPC(player.ActorNumber, playerScore);
            }
        }
    }

    private void OnDestroy()
    {
        GeriSayim.OnGameOver -= GameOver_RPC;
    }
}
