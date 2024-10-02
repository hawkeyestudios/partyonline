using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using UnityEngine.TextCore.Text;

public class GhostManager : MonoBehaviourPunCallbacks
{
    public Transform[] spawnPoints;
    public Image[] profileImages;
    // ScoreSystem
    public Text[] playerScoreTexts;
    public float scoreIncrement = 1f;

    public Text[] nickNames;
    public GameObject gameOverPanel;
    public GeriSayým geriSayým;

    public Text[] gameOverScoreTexts;

    private List<Player> frozenPlayers = new List<Player>();
    private bool raceFinished = false;
    private bool scoreCountingStarted = false;

    public Color[] playerColors = { Color.red, new Color(0.25f, 0.88f, 0.82f), Color.yellow, new Color(0.63f, 0.13f, 0.94f) };

    public PlayerScoreData playerScoreData;
    public LevelData levelData;
    private void Start()
    {
        PhotonNetwork.AutomaticallySyncScene = true;
        ResetPlayerScores();
        ResetScoreTexts();
        gameOverPanel.SetActive(false);
        if (PhotonNetwork.IsMasterClient)
        {
            SpawnPlayersForAll();
        }
        GeriSayým.OnGameOver += GameOver_RPC;
        SetPlayerProfileImage();
        geriSayým.StartCountdown();

        StartCoroutine(StartScoreCountingAfterDelay(10f));
    }

    private IEnumerator StartScoreCountingAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        scoreCountingStarted = true;
        StartCoroutine(UpdateScores());
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
    private IEnumerator UpdateScores()
    {
        while (!raceFinished && PhotonNetwork.IsMasterClient)
        {
            foreach (Player player in PhotonNetwork.PlayerList)
            {
                if (!frozenPlayers.Contains(player))
                {
                    for (int i = 0; i < 10; i++)
                    {
                        float currentScore = GetPlayerScore(player);
                        currentScore += 1f;
                        playerScoreData.AddScore(0.1f);
                        SetPlayerScore(player, currentScore);
                    }
                }
            }

            photonView.RPC("UpdateScoreUI_RPC", RpcTarget.All);

            yield return new WaitForSeconds(0.1f);
        }
    }

    [PunRPC]
    private void UpdateScoreUI_RPC()
    {
        for (int i = 0; i < PhotonNetwork.PlayerList.Length; i++)
        {
            Player player = PhotonNetwork.PlayerList[i];
            playerScoreTexts[i].text = GetPlayerScore(player).ToString("F0");
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
            if (score is float)
            {
                return (float)score;
            }
            else if (score is int)
            {
                return (int)score;
            }
            else if (score is double)
            {
                return (float)(double)score;
            }
            else
            {
                Debug.LogError("PlayerScore has an unexpected type: " + score.GetType());
                return 0f;
            }
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
        ExitGames.Client.Photon.Hashtable playerProperties = new ExitGames.Client.Photon.Hashtable();
        playerProperties["profileImage"] = playerImageName;
        PhotonNetwork.LocalPlayer.SetCustomProperties(playerProperties);
    }
    public void MarkPlayerAsFinished(Player player)
    {
        if (!frozenPlayers.Contains(player))
        {
            frozenPlayers.Add(player);
        }

        if (frozenPlayers.Count == PhotonNetwork.PlayerList.Length)
        {
            raceFinished = true;
            photonView.RPC("GameOver_RPC", RpcTarget.All);
        }
    }

    [PunRPC]
    private void GameOver_RPC()
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
            gameOverScoreTexts[i].text = GetPlayerScore(player).ToString("F0");
            profileImages[i].sprite = GetProfileSprite(player);
            nickNames[i].text = player.NickName;
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
        GeriSayým.OnGameOver -= GameOver_RPC;
    }
}
