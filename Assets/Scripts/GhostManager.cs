using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;

public class GhostManager : MonoBehaviourPunCallbacks
{
    public Transform[] spawnPoints;
    public Image[] profileImages;
    //ScoreSystem
    public Text[] playerScoreTexts;
    public float scoreIncrement = 0.2f;
    public Image[] scoreImages;
    public Text[] scoreNames;

    public Text[] nickNames;
    public GameObject gameOverPanel;
    public GeriSayým geriSayým;

    private List<Player> frozenPlayers = new List<Player>();
    private bool raceFinished = false;

    private void Start()
    {
        gameOverPanel.SetActive(false);
        SpawnPlayer();
        SetPlayerProfileImage();
        GeriSayým.OnGameOver += GameOver;
        geriSayým.StartCountdown();

        // scoreImages ve scoreNames'leri profileImages ve nickNames'e eþitle
        scoreImages = profileImages;
        scoreNames = nickNames;
    }
    private void Update()
    {
        if (!raceFinished)
        {
            UpdateScores();
            UpdateScoreUI();
        }
    }
    private void UpdateScores()
    {
        foreach (Player player in PhotonNetwork.PlayerList)
        {
            if (!frozenPlayers.Contains(player))
            {
                float currentScore = GetPlayerScore(player);
                currentScore += scoreIncrement * Time.deltaTime;
                SetPlayerScore(player, currentScore);
            }
        }
    }

    private void UpdateScoreUI()
    {
        for (int i = 0; i < PhotonNetwork.PlayerList.Length; i++)
        {
            Player player = PhotonNetwork.PlayerList[i];
            playerScoreTexts[i].text = player.NickName + ": " + GetPlayerScore(player).ToString("F1"); // Skoru UI'ye güncelle
        }
    }
    private float GetPlayerScore(Player player)
    {
        if (player.CustomProperties.TryGetValue("PlayerScore", out object score))
        {
            return (float)score;
        }
        return 0f;
    }

    private void SetPlayerScore(Player player, float score)
    {
        ExitGames.Client.Photon.Hashtable playerProperties = new ExitGames.Client.Photon.Hashtable
        {
            { "PlayerScore", score }
        };
        player.SetCustomProperties(playerProperties);
    }
    private void SpawnPlayer()
    {
        if (PhotonNetwork.IsConnectedAndReady)
        {
            Player localPlayer = PhotonNetwork.LocalPlayer;

            int spawnPointIndex = localPlayer.ActorNumber % spawnPoints.Length;
            Transform spawnPoint = spawnPoints[spawnPointIndex];
            Vector3 spawnPosition = spawnPoint.position;
            Quaternion spawnRotation = spawnPoint.rotation;

            string characterPrefabName = PlayerPrefs.GetString("LastEquippedCharacter", "DefaultCharacter");

            if (!string.IsNullOrEmpty(characterPrefabName))
            {
                GameObject character = PhotonNetwork.Instantiate(characterPrefabName, spawnPosition, spawnRotation);

                if (character != null)
                {
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
        string playerImageName = PlayerPrefs.GetString("LastEquippedCharacter", "defaultProfileImage");
        ExitGames.Client.Photon.Hashtable playerProperties = new ExitGames.Client.Photon.Hashtable();
        playerProperties["profileImage"] = playerImageName;
        PhotonNetwork.LocalPlayer.SetCustomProperties(playerProperties);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (raceFinished) return;

        if (other.CompareTag("Player"))
        {
            Player player = other.GetComponent<PhotonView>().Owner;

            if (!frozenPlayers.Contains(player))
            {
                frozenPlayers.Add(player); // Oyuncunun puan kazanýmýný durdur
                UpdateScoreUI(); // Skoru güncelle
            }

            if (frozenPlayers.Count == PhotonNetwork.PlayerList.Length)
            {
                raceFinished = true;
                geriSayým.StopAllCoroutines();
                GameOver();
            }
        }
    }
    private void GameOver()
    {
        raceFinished = true;
        gameOverPanel.SetActive(true);
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

    private void OnDestroy()
    {
        GeriSayým.OnGameOver -= GameOver;
    }
}
