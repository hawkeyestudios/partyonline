using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Collections;
using System.Linq;

public class SumoManager : MonoBehaviourPunCallbacks
{
    public Transform[] spawnPoints;
    public Image[] profileImages;

    // Score system
    public Text[] playerScoreTexts;
    public float scoreIncrement = 1f;

    public Text[] nickNames;
    public GameObject gameOverPanel;
    public GeriSayim geriSayým;

    public Text[] gameOverScoreTexts;
    public Image[] gameOverProfileImages;
    public Text[] gameOverNickNames;

    private bool raceFinished = false;

    public Color[] playerColors = { Color.red, new Color(0.25f, 0.88f, 0.82f), Color.yellow, new Color(0.63f, 0.13f, 0.94f) };
    public PlayerScoreData playerScoreData;
    public LevelData levelData;


    private Dictionary<Player, bool> playerIsActive;

    private void Start()
    {
        PhotonNetwork.AutomaticallySyncScene = true;
        ResetPlayerScores();
        ResetScoreTexts();
        Debug.Log("Start() called");
        gameOverPanel.SetActive(false);

        playerIsActive = new Dictionary<Player, bool>();
        foreach (Player player in PhotonNetwork.PlayerList)
        {
            playerIsActive[player] = true;
        }

        if (PhotonNetwork.IsMasterClient)
        {
            SpawnPlayersForAll();
        }
        SetPlayerProfileImage();
        GeriSayim.OnGameOver += GameOver_RPC;
        geriSayým.StartCountdown();

        StartCoroutine(StartScoreCountingAfterDelay(10f));
    }

    private IEnumerator StartScoreCountingAfterDelay(float delay)
    {
        Debug.Log("Score counting will start after delay: " + delay + " seconds.");
        yield return new WaitForSeconds(delay);
        StartCoroutine(IncrementScoresOverTime());
    }

    private IEnumerator IncrementScoresOverTime()
    {
        while (!raceFinished)
        {
            foreach (Player player in PhotonNetwork.PlayerList)
            {
                if (playerIsActive[player])
                {
                    float currentScore = GetPlayerScore(player);
                    float newScore = currentScore + scoreIncrement;
                    playerScoreData.AddScore(1f);
                    SetPlayerScore(player, newScore);
                }
            }
            photonView.RPC("UpdateScoreUI_RPC", RpcTarget.All);
            yield return new WaitForSeconds(0.1f);
        }
    }

    public void ResetScoreTexts()
    {
        foreach (Text scoreText in playerScoreTexts)
        {
            scoreText.text = "0";
        }
        foreach (Text scoreText in gameOverScoreTexts)
        {
            scoreText.text = "0";
        }
    }

    [PunRPC]
    private void UpdateScoreUI_RPC()
    {
        Debug.Log("Updating UI for player scores.");
        for (int i = 0; i < PhotonNetwork.PlayerList.Length; i++)
        {
            Player player = PhotonNetwork.PlayerList[i];
            playerScoreTexts[i].text = GetPlayerScore(player).ToString("F0");
            Debug.Log("Score updated for " + player.NickName + ": " + playerScoreTexts[i].text);
        }
    }

    private void ResetPlayerScores()
    {
        foreach (Player player in PhotonNetwork.PlayerList)
        {
            SetPlayerScore(player, 0f);
        }

        photonView.RPC("UpdateScoreUI_RPC", RpcTarget.All);
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
        Debug.Log("Setting score for " + player.NickName + " | Score: " + score);
        ExitGames.Client.Photon.Hashtable playerProperties = new ExitGames.Client.Photon.Hashtable
        {
            { "PlayerScore", score }
        };
        player.SetCustomProperties(playerProperties);
    }

    private void SpawnPlayersForAll()
    {
        foreach (var player in PhotonNetwork.PlayerList)
        {
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
        ExitGames.Client.Photon.Hashtable playerProperties = new ExitGames.Client.Photon.Hashtable
        {
            { "profileImage", playerImageName }
        };
        PhotonNetwork.LocalPlayer.SetCustomProperties(playerProperties);
        Debug.Log("Profile image set for player: " + PhotonNetwork.LocalPlayer.NickName);
    }

    [PunRPC]
    private void GameOver_RPC()
    {
        raceFinished = true;
        gameOverPanel.SetActive(true);
        Debug.Log("Game over. Displaying scores.");
        UpdateGameOverScores();
    }

    private void UpdateGameOverScores()
    {
        Debug.Log("Updating game over scores.");
        for (int i = 0; i < PhotonNetwork.PlayerList.Length; i++)
        {
            Player player = PhotonNetwork.PlayerList[i];
            gameOverScoreTexts[i].text = GetPlayerScore(player).ToString("F0");
            gameOverProfileImages[i].sprite = GetProfileSprite(player);
            gameOverNickNames[i].text = player.NickName;
            Debug.Log("Game over score for " + player.NickName + ": " + gameOverScoreTexts[i].text);
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
        GeriSayim.OnGameOver -= GameOver_RPC;
        Debug.Log("SumoManager destroyed. Cleanup done.");
    }


    [PunRPC]
    public void PlayerEliminated_RPC(int actorNumber)
    {
        Player player = PhotonNetwork.CurrentRoom.GetPlayer(actorNumber);
        if (player != null)
        {
            if (playerIsActive.ContainsKey(player) && playerIsActive[player])
            {
                playerIsActive[player] = false;
                Debug.Log("Player " + player.NickName + " has been eliminated.");

                int activePlayerCount = playerIsActive.Values.Count(active => active);
                if (activePlayerCount <= 1)
                {
                    StartCoroutine(EndGameAfterDelay(2f));
                }
            }
        }
    }

    private IEnumerator EndGameAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        photonView.RPC("GameOver_RPC", RpcTarget.All);
    }
}
