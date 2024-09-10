using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Collections;
using System.Linq;

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

    public Color[] playerColors = { Color.red, new Color(0.25f, 0.88f, 0.82f), Color.yellow, new Color(0.63f, 0.13f, 0.94f) };
    private void Start()
    {
        gameOverPanel.SetActive(false);
        if (PhotonNetwork.IsMasterClient)
        {
            StartCoroutine(SpawnPlayersForAll());
        }
        SetPlayerProfileImage();

        GeriSayým.OnGameOver += GameOver_RPC;
        geriSayým.StartCountdown();
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

                int playerIndex = finishOrder.IndexOf(player);
                int reward = Mathf.Max(1000 - (playerIndex * 250), 0);
                SetPlayerScore(player, reward);

                photonView.RPC("UpdatePlayerUI_RPC", RpcTarget.All, player.ActorNumber, reward);

                if (finishOrder.Count == PhotonNetwork.PlayerList.Length)
                {
                    raceFinished = true;
                    photonView.RPC("GameOver_RPC", RpcTarget.All);
                }
            }
        }
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
    }
    private void SetPlayerScore(Player player, int score)
    {
        ExitGames.Client.Photon.Hashtable playerProperties = new ExitGames.Client.Photon.Hashtable
    {
        { "PlayerScore", score }
    };
        player.SetCustomProperties(playerProperties);
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
                int playerScore = (int)player.CustomProperties["PlayerScore"];
                UpdatePlayerUI_RPC(player.ActorNumber, playerScore);
            }
        }
    }


    private void OnDestroy()
    {
        GeriSayým.OnGameOver -= GameOver_RPC;
    }

}
