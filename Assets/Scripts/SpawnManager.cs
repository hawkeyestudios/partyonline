using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.UI;
using System.Collections.Generic;

public class SpawnManager : MonoBehaviourPunCallbacks
{
    public Transform[] spawnPoints;
    public Image[] profileImages;
    public Text[] rewardTexts;
    public Text[] nickNames;
    public GameObject gameOverPanel;
    public GeriSayým geriSayým;

    private List<Player> finishOrder = new List<Player>();
    private bool raceFinished = false;

    private void Start()
    {
        gameOverPanel.SetActive(false);
        SpawnPlayer();
        SetPlayerProfileImage();

        GeriSayým.OnGameOver += GameOver_RPC;
        geriSayým.StartCountdown();
    }

    private void SpawnPlayer()
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

    private void SetPlayerProfileImage()
    {
        string playerImageName = PlayerPrefs.GetString("LastEquippedCharacter");
        ExitGames.Client.Photon.Hashtable playerProperties = new ExitGames.Client.Photon.Hashtable();
        playerProperties["profileImage"] = playerImageName;
        PhotonNetwork.LocalPlayer.SetCustomProperties(playerProperties);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (raceFinished || !PhotonNetwork.IsMasterClient) return;

        if (other.CompareTag("Player"))
        {
            Player player = other.GetComponent<PhotonView>().Owner;

            if (!finishOrder.Contains(player))
            {
                finishOrder.Add(player);

                photonView.RPC("UpdatePlayerUI_RPC", RpcTarget.All, player.ActorNumber, true);

                if (finishOrder.Count == PhotonNetwork.PlayerList.Length)
                {
                    raceFinished = true;
                    photonView.RPC("GameOver_RPC", RpcTarget.All);
                }
            }
        }
    }

    [PunRPC]
    private void UpdatePlayerUI_RPC(int playerActorNumber, bool passedFinish)
    {
        Player player = PhotonNetwork.CurrentRoom.GetPlayer(playerActorNumber);
        if (player == null) return;

        int playerIndex = finishOrder.IndexOf(player);
        if (playerIndex >= 0 && playerIndex < profileImages.Length)
        {
            profileImages[playerIndex].sprite = GetProfileSprite(player);

            int reward = passedFinish ? Mathf.Max(1000 - (playerIndex * 250), 0) : 0;

            rewardTexts[playerIndex].text = $"{reward}";
            //if (reward > 0 && player == PhotonNetwork.LocalPlayer)
            //{
                //CoinManager.Instance.AddCoins(reward);
            //}

            nickNames[playerIndex].text = player.NickName;
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
                UpdatePlayerUI_RPC(player.ActorNumber, false);
            }
        }
    }
    private void OnDestroy()
    {
        GeriSayým.OnGameOver -= GameOver_RPC;
    }
}
