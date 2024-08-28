using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Collections;

public class CrownManager : MonoBehaviourPunCallbacks
{
    public Transform[] spawnPoints;
    public Image[] profileImages;
    public GameObject crownPrefab;

    // Score system
    public Text[] playerScoreTexts;
    public float scoreIncrement = 0.2f;

    public Text[] nickNames;
    public GameObject gameOverPanel;
    public GeriSayým geriSayým;

    public Text[] gameOverScoreTexts;
    public Image[] gameOverProfileImages;
    public Text[] gameOverNickNames;

    private Player currentCrownHolder = null;
    private Dictionary<Player, Transform> playerTransforms = new Dictionary<Player, Transform>();
    private bool raceFinished = false;
    private bool scoreCountingStarted = false;

    private void Start()
    {
        gameOverPanel.SetActive(false);
        SpawnPlayer();
        SetPlayerProfileImage();
        GeriSayým.OnGameOver += GameOver;
        geriSayým.StartCountdown();

        StartCoroutine(StartScoreCountingAfterDelay(10f));
    }

    private IEnumerator StartScoreCountingAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        scoreCountingStarted = true;
    }

    private void Update()
    {
        if (!raceFinished && scoreCountingStarted)
        {
            UpdateScores();
            UpdateScoreUI();
            UpdateCrownPosition();
        }
    }

    private void UpdateScores()
    {
        if (currentCrownHolder != null)
        {
            float currentScore = GetPlayerScore(currentCrownHolder);
            currentScore += scoreIncrement * Time.deltaTime;
            SetPlayerScore(currentCrownHolder, currentScore);
        }
    }

    private void UpdateScoreUI()
    {
        for (int i = 0; i < PhotonNetwork.PlayerList.Length; i++)
        {
            Player player = PhotonNetwork.PlayerList[i];
            playerScoreTexts[i].text = player.NickName + ": " + GetPlayerScore(player).ToString("F1");
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
                playerTransforms[localPlayer] = character.transform;

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

    private void UpdateCrownPosition()
    {
        if (currentCrownHolder != null && playerTransforms.ContainsKey(currentCrownHolder))
        {
            Transform crownTransform = crownPrefab.transform;
            Transform playerTransform = playerTransforms[currentCrownHolder];

            crownTransform.position = playerTransform.position + new Vector3(0, 2.5f, 0); // Taç pozisyonu, oyuncunun üzerinde olacak þekilde ayarlanýr
        }
    }

    public void OnPlayerInteractWithCrown(Player player)
    {
        if (currentCrownHolder == null || player != currentCrownHolder)
        {
            currentCrownHolder = player;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Player player = other.GetComponent<PhotonView>().Owner;
            OnPlayerInteractWithCrown(player);
        }
    }

    private void GameOver()
    {
        raceFinished = true;
        gameOverPanel.SetActive(true);
        UpdateGameOverScores();
    }

    private void UpdateGameOverScores()
    {
        for (int i = 0; i < PhotonNetwork.PlayerList.Length; i++)
        {
            Player player = PhotonNetwork.PlayerList[i];
            gameOverScoreTexts[i].text = player.NickName + ": " + GetPlayerScore(player).ToString("F1");
            gameOverProfileImages[i].sprite = GetProfileSprite(player);
            gameOverNickNames[i].text = player.NickName;
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

    private void OnDestroy()
    {
        GeriSayým.OnGameOver -= GameOver;
    }
}
