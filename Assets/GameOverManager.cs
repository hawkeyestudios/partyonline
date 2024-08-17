using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.UI;
using System.Collections.Generic;

public class GameOverManager : MonoBehaviourPunCallbacks
{
    public GameObject gameOverPanel;
    public Transform playerDataContainer;

    public GameObject playerDataPrefab1;
    public GameObject playerDataPrefab2;
    public GameObject playerDataPrefab3;
    public GameObject playerDataPrefab4;

    private Dictionary<string, PlayerData> allPlayerData = new Dictionary<string, PlayerData>();
    private List<string> finishOrder = new List<string>();

    [System.Serializable]
    public class PlayerData
    {
        public string nickname;
        public int coins;
        public Sprite profileImage;
        public int rank;
    }

    void Start()
    {
        gameOverPanel.SetActive(false);
    }

    [PunRPC]
    public void SyncPlayerData(string playerId, int coins, string profileImageName, int rank)
    {
        string nickname = PhotonNetwork.NickName;
        Sprite profileImage = Resources.Load<Sprite>("ProfileImages/" + profileImageName);

        if (profileImage == null)
        {
            Debug.LogWarning("Profile image not found: " + profileImageName);
            profileImage = Resources.Load<Sprite>("ProfileImages/defaultProfileImage");
        }

        PlayerData playerData = new PlayerData
        {
            nickname = nickname,
            coins = coins,
            profileImage = profileImage,
            rank = rank
        };

        if (!allPlayerData.ContainsKey(playerId))
        {
            allPlayerData.Add(playerId, playerData);
        }

        UpdateGameOverPanel();
    }

    public void OnPlayerFinish(string playerId)
    {
        if (finishOrder.Contains(playerId)) return;

        finishOrder.Add(playerId);

        int earnedCoins = CalculateCoins(finishOrder.Count);
        string profileImageName = PlayerPrefs.GetString("LastEquippedCharacter", "defaultProfileImage");

        photonView.RPC("SyncPlayerData", RpcTarget.All, playerId, earnedCoins, profileImageName, finishOrder.Count);

        CoinManager.Instance.AddCoins(earnedCoins);

        if (finishOrder.Count == PhotonNetwork.PlayerList.Length)
        {
            OnGameOver();
        }
    }

    private int CalculateCoins(int rank)
    {
        switch (rank)
        {
            case 1: return 3000;
            case 2: return 1500;
            case 3: return 1000;
            case 4: return 500;
            default: return 0;
        }
    }

    public void UpdateGameOverPanel()
    {
        foreach (Transform child in playerDataContainer)
        {
            Destroy(child.gameObject);
        }

        foreach (var data in allPlayerData.Values)
        {
            GameObject playerDataObj;

            switch (data.rank)
            {
                case 1:
                    playerDataObj = Instantiate(playerDataPrefab1, playerDataContainer);
                    break;
                case 2:
                    playerDataObj = Instantiate(playerDataPrefab2, playerDataContainer);
                    break;
                case 3:
                    playerDataObj = Instantiate(playerDataPrefab3, playerDataContainer);
                    break;
                case 4:
                    playerDataObj = Instantiate(playerDataPrefab4, playerDataContainer);
                    break;
                default:
                    playerDataObj = Instantiate(playerDataPrefab1, playerDataContainer);
                    break;
            }

            Text nicknameText = playerDataObj.transform.Find("NicknameText").GetComponent<Text>();
            Text coinsText = playerDataObj.transform.Find("CoinsText").GetComponent<Text>();
            Image profileImage = playerDataObj.transform.Find("ProfileImage").GetComponent<Image>();

            nicknameText.text = data.nickname;
            coinsText.text = data.coins.ToString();
            profileImage.sprite = data.profileImage;
        }

        gameOverPanel.SetActive(true);
    }

    public void OnGameOver()
    {
        foreach (Player player in PhotonNetwork.PlayerList)
        {
            string playerId = player.UserId;
            if (!finishOrder.Contains(playerId))
            {
                photonView.RPC("SyncPlayerData", RpcTarget.All, playerId, 0, PlayerPrefs.GetString("LastEquippedCharacter", "defaultProfileImage"), 0);
            }
        }

        UpdateGameOverPanel();
    }
}
