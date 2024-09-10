using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Collections;
using System.Linq;

public class BarrelManager : MonoBehaviourPunCallbacks
{
    public Transform[] spawnPoints;
    public Image[] profileImages;

    // Score system
    public Text[] playerScoreTexts;
    public float scoreIncrement = 1f;

    public Text[] nickNames;
    public GameObject gameOverPanel;
    public GeriSayým geriSayým;

    public Text[] gameOverScoreTexts;
    public Image[] gameOverProfileImages;
    public Text[] gameOverNickNames;

    private List<Player> frozenPlayers = new List<Player>();
    private bool raceFinished = false;
    private bool scoreCountingStarted = false;

    public Color[] playerColors = { Color.red, new Color(0.25f, 0.88f, 0.82f), Color.yellow, new Color(0.63f, 0.13f, 0.94f) };
    private void Start()
    {
        ResetPlayerScores();
        ResetScoreTexts();
        gameOverPanel.SetActive(false);
        if (PhotonNetwork.IsMasterClient)
        {
            StartCoroutine(SpawnPlayersForAll());
        }
        SetPlayerProfileImage();
        GeriSayým.OnGameOver += GameOver_RPC;
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
        while (!raceFinished)
        {
            foreach (Player player in PhotonNetwork.PlayerList)
            {
                if (!frozenPlayers.Contains(player))
                {
                    // Sadece oyuncu kendi skorunu arttýrýr
                    if (player == PhotonNetwork.LocalPlayer)
                    {
                        for (int i = 0; i < 10; i++)
                        {
                            float currentScore = GetPlayerScore(player);
                            currentScore += 1f;
                            SetPlayerScore(player, currentScore);
                        }
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

    public void OnPlayerDeath(Player player)
    {
        if (!frozenPlayers.Contains(player))
        {
            frozenPlayers.Add(player);

            // Sadece ölen oyuncunun skorunu durdurmak için bir RPC çaðrýsý yapýyoruz.
            photonView.RPC("FreezePlayerScore_RPC", RpcTarget.All, player.ActorNumber);

            CheckForGameOver();
        }
    }
    [PunRPC]
    private void FreezePlayerScore_RPC(int playerActorNumber)
    {
        Player player = PhotonNetwork.CurrentRoom.GetPlayer(playerActorNumber);

        // Eðer bu player o anki yerel oyuncuysa, onun skorunu durdur
        if (player != null && player == PhotonNetwork.LocalPlayer)
        {
            scoreCountingStarted = false;
            Debug.Log("Score counting stopped for player: " + player.NickName);
        }
    }
    private void CheckForGameOver()
    {

        if (frozenPlayers.Count == PhotonNetwork.CurrentRoom.PlayerCount)
        {
            Debug.Log("Oyun bitti fakat gameoverpanel açýlmadýysa oyuncu sayýsý hesaplanamadýðý maç bitmedi");
            photonView.RPC("GameOver_RPC", RpcTarget.All);
        }
    }
    private IEnumerator DelayedGameOver()
    {
        yield return new WaitForSeconds(2f);
        photonView.RPC("GameOver_RPC", RpcTarget.All);
    }
    private IEnumerator SpawnPlayersForAll()
    {
        foreach (Player player in PhotonNetwork.PlayerList)
        {
            photonView.RPC("SpawnPlayerForAll", RpcTarget.AllBuffered, player.ActorNumber);
            yield return new WaitForSeconds(0.5f);  // Spawn iþlemleri arasýnda kýsa bir gecikme
        }
    }

    [PunRPC]
    private void SpawnPlayerForAll(int playerActorNumber)
    {
        Player player = PhotonNetwork.PlayerList.FirstOrDefault(p => p.ActorNumber == playerActorNumber);

        if (player != null && player == PhotonNetwork.LocalPlayer)
        {
            int spawnPointIndex = player.ActorNumber % spawnPoints.Length;
            Transform spawnPoint = spawnPoints[spawnPointIndex];
            Vector3 spawnPosition = spawnPoint.position;
            Quaternion spawnRotation = spawnPoint.rotation;

            string characterPrefabName = PlayerPrefs.GetString("LastEquippedCharacter", "DefaultCharacter");

            if (!string.IsNullOrEmpty(characterPrefabName))
            {
                GameObject character = PhotonNetwork.Instantiate(characterPrefabName, spawnPosition, spawnRotation);

                if (character != null)
                {
                    // Oyuncunun rengi ayarlanýyor
                    int playerIndex = PhotonNetwork.LocalPlayer.ActorNumber - 1;
                    Renderer circleRenderer = character.transform.Find("Circle").GetComponent<Renderer>(); // Circle prefabýna eriþ
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
        GeriSayým.OnGameOver -= GameOver_RPC;
    }
}
