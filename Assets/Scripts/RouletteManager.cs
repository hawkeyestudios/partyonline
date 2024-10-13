using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.UI;

public class RouletteManager : MonoBehaviourPunCallbacks
{
    public Image[] profileImages; // Oyuncu profil resimleri
    public Text[] rewardTexts; // Oyuncu puanlar�
    public Text[] nickNames; // Oyuncu nicknameleri
    public GameObject gameOverPanel; // GameOver ekran�

    public Transform[] spawnPoints; // Oyuncu spawn noktalar�
    public Transform tableTransform; // Masa pozisyonu
    public GameObject revolverPrefab; // Alt�patlar silah� prefab�
    public Text countdownText; // Geri say�m UI Text

    private GameObject revolverInstance; // Silah instance'�
    private List<Player> finishOrder = new List<Player>(); // Oyuncular�n elenme s�ras�
    private bool raceFinished = false;
    private int currentPlayerIndex = 0; // S�ras� gelen oyuncu
    private float timer;
    private GameObject selectedTarget; // Se�ilen oyuncu
    private bool isShootingPhase = false;

    private int[] scores = { 1000, 750, 500, 250 };
    public Color[] playerColors = { Color.red, new Color(0.25f, 0.88f, 0.82f), Color.yellow, new Color(0.63f, 0.13f, 0.94f) };

    void Start()
    {
        PhotonNetwork.AutomaticallySyncScene = true;
        gameOverPanel.SetActive(false);

        SpawnPlayerForSelf();
        SpawnRevolver();
        StartTurn();
    }

    void Update()
    {
        if (!raceFinished)
        {
            UpdateTimer();
        }
    }
    private void SpawnPlayerForSelf()
    {
        int playerActorNumber = PhotonNetwork.LocalPlayer.ActorNumber;

        int spawnPointIndex = (playerActorNumber - 1) % spawnPoints.Length;
        Transform spawnPoint = spawnPoints[spawnPointIndex];
        Vector3 spawnPosition = spawnPoint.position;
        Quaternion spawnRotation = spawnPoint.rotation;

        // Karakter prefab'ini al
        string characterPrefabName = PlayerPrefs.GetString("LastEquippedCharacter", "DefaultCharacter");

        if (!string.IsNullOrEmpty(characterPrefabName))
        {
            GameObject character = PhotonNetwork.Instantiate(characterPrefabName, spawnPosition, spawnRotation, 0);

            if (character != null)
            {
                // Oyuncu rengini ayarla
                Renderer circleRenderer = character.transform.Find("Circle").GetComponent<Renderer>();
                int playerIndex = playerActorNumber - 1;

                if (circleRenderer != null && playerIndex >= 0 && playerIndex < playerColors.Length)
                {
                    circleRenderer.material.color = playerColors[playerIndex];
                }

                // Karakteri oyuncuya atanm�� bir tag olarak sakla
                PhotonNetwork.LocalPlayer.TagObject = character;

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

    void SpawnRevolver()
    {
        if (revolverPrefab != null)
        {
            revolverInstance = PhotonNetwork.Instantiate(revolverPrefab.name, tableTransform.position, tableTransform.rotation);
            Debug.Log("Alt�patlar masaya spawn edildi.");
        }
        else
        {
            Debug.LogError("Revolver prefab eksik!");
        }
    }
    void UpdateTimer()
    {
        if (timer > 0)
        {
            timer -= Time.deltaTime; 
            countdownText.text = Mathf.Ceil(timer).ToString(); 
        }
        else
        {
            EliminatePlayer(PhotonNetwork.PlayerList[currentPlayerIndex]); 
        }
    }

    [PunRPC]
    void StartTurn()
    {
        if (finishOrder.Count == PhotonNetwork.PlayerList.Length - 1)
        {
            Player lastPlayer = PhotonNetwork.PlayerList[0];
            AddScore(lastPlayer, scores[0]);
            raceFinished = true;
            photonView.RPC("GameOver_RPC", RpcTarget.All);
            return;
        }

        GameObject currentPlayer = PhotonNetwork.PlayerList[currentPlayerIndex].TagObject as GameObject;
        timer = 30f;
        isShootingPhase = true; 

        GiveWeaponToPlayer(currentPlayer);
        photonView.RPC("StartTurn", RpcTarget.Others);
    }

    void GiveWeaponToPlayer(GameObject player)
    {
        if (revolverInstance != null)
        {
            Transform playerHand = player.transform.Find("HandPosition");
            if (playerHand != null)
            {
                revolverInstance.transform.SetParent(playerHand);
                revolverInstance.transform.localPosition = Vector3.zero;
                revolverInstance.transform.localRotation = Quaternion.identity;
                Debug.Log(player.name + " silah� ald�.");
            }
            else
            {
                Debug.LogError("Oyuncunun el pozisyonu bulunamad�.");
            }
        }
    }

    public void OnSelfShootButtonPressed()
    {
        if (!isShootingPhase) return;

        GameObject currentPlayer = PhotonNetwork.PlayerList[currentPlayerIndex].TagObject as GameObject;
        selectedTarget = currentPlayer;

        Debug.Log(currentPlayer.name + " kendine ate� etti.");
        Shoot();
    }

    public void SelectTarget(GameObject target)
    {
        if (isShootingPhase)
        {
            selectedTarget = target;
            Debug.Log(target.name + " hedef olarak se�ildi.");
        }
    }

    public void Shoot()
    {
        if (selectedTarget == null) return;

        Debug.Log("Ate� edildi: " + selectedTarget.name);

        if (IsPlayerDead(selectedTarget))
        {
            EliminatePlayer(selectedTarget.GetComponent<PhotonView>().Owner);
        }

        isShootingPhase = false; 
        NextPlayer(); 
    }

    bool IsPlayerDead(GameObject player)
    {
        Animator animator = player.GetComponent<Animator>();
        animator.SetTrigger("Die");
        Destroy(player, 2f);
        return true; 
    }

    void EliminatePlayer(Player player)
    {
        if (!finishOrder.Contains(player))
        {
            finishOrder.Add(player);
            int playerIndex = finishOrder.Count - 1; 
            int reward = Mathf.Max(1000 - (playerIndex * 250), 0); 
            AddScore(player, reward);

            photonView.RPC("UpdatePlayerUI_RPC", RpcTarget.All, player.ActorNumber, reward);
        }

        NextPlayer();
    }

    void NextPlayer()
    {
        currentPlayerIndex = (currentPlayerIndex + 1) % PhotonNetwork.PlayerList.Length;
        StartTurn();
    }

    void AddScore(Player player, int score)
    {
        ExitGames.Client.Photon.Hashtable playerProperties = new ExitGames.Client.Photon.Hashtable
        {
            { "PlayerScore", score }
        };
        player.SetCustomProperties(playerProperties);
    }

    [PunRPC]
    private void UpdatePlayerUI_RPC(int playerActorNumber, int reward)
    {
        Player player = PhotonNetwork.CurrentRoom.GetPlayer(playerActorNumber);
        if (player == null) return;

        int playerIndex = finishOrder.IndexOf(player);
        if (playerIndex >= 0 && playerIndex < profileImages.Length)
        {
            profileImages[playerIndex].sprite = GetProfileSprite(player); // Profil resmi ayarlan�yor
            rewardTexts[playerIndex].text = reward.ToString(); // Puan
            nickNames[playerIndex].text = player.NickName; // �sim
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

        // Oyun bitti�inde t�m oyuncular�n nick ve puanlar�n� g�ncelle
        foreach (Player player in PhotonNetwork.PlayerList)
        {
            if (!finishOrder.Contains(player))
            {
                finishOrder.Add(player);
                int playerScore = (int)player.CustomProperties["PlayerScore"];
                UpdatePlayerUI_RPC(player.ActorNumber, playerScore);
            }
        }
    }
}
