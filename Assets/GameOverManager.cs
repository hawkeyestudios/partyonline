using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;
using System.Collections.Generic;

public class GameOverManager : MonoBehaviourPunCallbacks
{
    public GameObject gameOverPanel; // Game Over Paneli
    public Transform playerDataContainer; // Player verilerinin gösterileceði UI container

    // Her oyuncu için ayrý prefablar
    public GameObject playerDataPrefab1;
    public GameObject playerDataPrefab2;
    public GameObject playerDataPrefab3;
    public GameObject playerDataPrefab4;

    private Dictionary<string, PlayerData> allPlayerData = new Dictionary<string, PlayerData>();

    [System.Serializable]
    public class PlayerData
    {
        public string nickname;
        public int coins;
        public Sprite profileImage;
        public int rank; // Oyuncunun bitiþ çizgisinden geçiþ sýrasý
    }

    private List<string> finishOrder = new List<string>(); // Bitiþ sýrasý

    void Start()
    {
        gameOverPanel.SetActive(false);
    }

    [PunRPC]
    public void SyncPlayerData(string playerId, int coins, string profileImageName, int rank)
    {
        // PlayerPrefs'ten nickname'i al
        string nickname = PlayerPrefs.GetString("DISPLAYNAME", "Guest");

        Sprite profileImage = Resources.Load<Sprite>("ProfileImages/" + profileImageName);

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

        // Update UI
        UpdateGameOverPanel();
    }

    public void OnPlayerFinish(string playerId)
    {
        if (finishOrder.Contains(playerId)) return; // Oyuncu zaten bitiþ çizgisini geçtiyse tekrar eklenmez

        finishOrder.Add(playerId);

        int earnedCoins = CalculateCoins(finishOrder.Count); // Bitiþ sýrasýna göre coins hesapla
        string profileImageName = "defaultProfileImage"; // Profil resminin adýný buradan belirleyin

        photonView.RPC("SyncPlayerData", RpcTarget.All, playerId, earnedCoins, profileImageName, finishOrder.Count);

        // Oyuncunun kazandýðý coinleri güncelle
        CoinManager.Instance.AddCoins(earnedCoins);

        // Eðer tüm oyuncular bitiþ çizgisine ulaþtýysa, oyunu bitir
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
            default: return 0; // 4. sýradan sonrasý veya hiç bitiþ çizgisine ulaþamayanlar için 0 coins
        }
    }

    void UpdateGameOverPanel()
    {
        // Eski UI verilerini temizle
        foreach (Transform child in playerDataContainer)
        {
            Destroy(child.gameObject);
        }

        // Yeni verileri ekle
        foreach (var data in allPlayerData.Values)
        {
            GameObject playerDataObj;

            // Prefab seçiminde oyuncunun sýrasýna göre arka plan belirleniyor
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
                    playerDataObj = Instantiate(playerDataPrefab1, playerDataContainer); // Varsayýlan
                    break;
            }

            // Prefab içerisindeki UI elemanlarýný doldur
            Text nicknameText = playerDataObj.transform.Find("NicknameText").GetComponent<Text>();
            Text coinsText = playerDataObj.transform.Find("CoinsText").GetComponent<Text>();
            Image profileImage = playerDataObj.transform.Find("ProfileImage").GetComponent<Image>();

            nicknameText.text = data.nickname;
            coinsText.text = data.coins.ToString();
            profileImage.sprite = data.profileImage;
        }

        // Game Over panelini göster
        gameOverPanel.SetActive(true);
    }

    public void OnGameOver()
    {
        // Bitiþ çizgisine ulaþamayan oyuncular için coins deðerini 0 olarak belirleyin
        foreach (Player player in PhotonNetwork.PlayerList)
        {
            string playerId = player.UserId; // Oyuncu ID'sini alýn
            if (!finishOrder.Contains(playerId))
            {
                photonView.RPC("SyncPlayerData", RpcTarget.All, playerId, 0, "defaultProfileImage", 0);
            }
        }

        // Game Over panelini güncelle ve göster
        UpdateGameOverPanel();
    }
}
