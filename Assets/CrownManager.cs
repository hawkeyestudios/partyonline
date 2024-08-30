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
    public float scoreIncrement = 1f;

    public Text[] nickNames;
    public GameObject gameOverPanel;
    public GeriSayým geriSayým;

    public Text[] gameOverScoreTexts;
    public Image[] gameOverProfileImages;
    public Text[] gameOverNickNames;

    private Player currentCrownHolder = null;
    private Dictionary<Player, Transform> playerTransforms = new Dictionary<Player, Transform>();
    private bool raceFinished = false;

    private void Start()
    {
        ResetPlayerScores();
        Debug.Log("Start() called");
        gameOverPanel.SetActive(false);
        SpawnPlayer();
        SetPlayerProfileImage();
        GeriSayým.OnGameOver += GameOver_RPC;
        geriSayým.StartCountdown();

        StartCoroutine(StartScoreCountingAfterDelay(10f));
        StartCoroutine(UpdateScores());
    }

    private IEnumerator StartScoreCountingAfterDelay(float delay)
    {
        Debug.Log("Score counting will start after delay: " + delay + " seconds.");
        yield return new WaitForSeconds(delay);
        StartCoroutine(UpdateScores());
    }

    private void Update()
    {
        photonView.RPC("UpdateCrownPosition_RPC", RpcTarget.All);
    }

    private IEnumerator UpdateScores()
    {
        while (!raceFinished && PhotonNetwork.IsMasterClient)
        {
            if (currentCrownHolder != null)
            {
                for (int i = 0; i < 10; i++)
                {
                    float currentScore = GetPlayerScore(currentCrownHolder);
                    currentScore += 1f;  
                    SetPlayerScore(currentCrownHolder, currentScore); 

                } 
                photonView.RPC("UpdateScoreUI_RPC", RpcTarget.All);
            }
            yield return new WaitForSeconds(0.1f); 
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

    private void SpawnPlayer()
    {
        Debug.Log("Spawning player...");
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
                Debug.Log("Character successfully spawned for player: " + localPlayer.NickName);
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
    private void UpdateCrownPosition_RPC()
    {
        if (currentCrownHolder != null && playerTransforms.ContainsKey(currentCrownHolder))
        {
            Transform crownTransform = crownPrefab.transform;
            Transform playerTransform = playerTransforms[currentCrownHolder];

            crownTransform.position = playerTransform.position + new Vector3(0, 2.5f, 0);
            Debug.Log("Crown position updated for " + currentCrownHolder.NickName);
        }
    }

    public void OnPlayerInteractWithCrown(Player player)
    {
        Debug.Log(player.NickName + " interacted with the crown.");
        if (currentCrownHolder == null || player != currentCrownHolder)
        {
            Debug.Log("Crown transferred to " + player.NickName);
            photonView.RPC("SetCrownHolder_RPC", RpcTarget.All, player.ActorNumber);
        }
    }

    [PunRPC]
    private void SetCrownHolder_RPC(int playerActorNumber)
    {
        Player player = PhotonNetwork.CurrentRoom.GetPlayer(playerActorNumber);
        if (player != null)
        {
            currentCrownHolder = player;
            Debug.Log("Crown holder set to " + player.NickName);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PhotonView playerPhotonView = other.GetComponent<PhotonView>();
            if (playerPhotonView != null)
            {
                Player player = playerPhotonView.Owner;
                Debug.Log("Player " + player.NickName + " triggered crown interaction.");
                OnPlayerInteractWithCrown(player);
            }
        }
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
        GeriSayým.OnGameOver -= GameOver_RPC;
        Debug.Log("CrownManager destroyed. Cleanup done.");
    }
}
