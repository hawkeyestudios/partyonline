using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;

public class GhostManager : MonoBehaviourPunCallbacks
{
    public Transform[] spawnPoints;
    public Image[] profileImages;
    public Text[] rewardTexts;
    public Text[] nickNames;
    public GameObject gameOverPanel;
    public GeriSayým geriSayým;

    private List<Player> finishOrder = new List<Player>(); // Yakalananlarýn sýralamasý
    private bool raceFinished = false;

    private void Start()
    {
        gameOverPanel.SetActive(false);
        SpawnPlayer();
        SetPlayerProfileImage();

        // Geri sayým olayýný dinleyin
        GeriSayým.OnGameOver += GameOver;

        // Geri sayýmý baþlat
        geriSayým.StartCountdown();
    }

    private void SpawnPlayer()
    {
        if (PhotonNetwork.IsConnectedAndReady)
        {
            Player localPlayer = PhotonNetwork.LocalPlayer;

            // Oyuncunun ActorNumber'ýna göre spawn noktasýný belirle
            int spawnPointIndex = localPlayer.ActorNumber % spawnPoints.Length;
            Transform spawnPoint = spawnPoints[spawnPointIndex];
            Vector3 spawnPosition = spawnPoint.position;
            Quaternion spawnRotation = spawnPoint.rotation;

            // Son seçilen karakter prefab ismini PlayerPrefs'ten al
            string characterPrefabName = PlayerPrefs.GetString("LastEquippedCharacter", "DefaultCharacter");

            if (!string.IsNullOrEmpty(characterPrefabName))
            {
                // Karakter prefab'ýný PhotonNetwork.Instantiate ile instantiate et
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

            if (!finishOrder.Contains(player))
            {
                finishOrder.Add(player); // Yakalananlarý sýraya ekle
                UpdatePlayerUI(player, 0); // Ýlk yakalandýðýnda henüz ödül vermiyoruz, sadece UI güncelleme

                if (finishOrder.Count == PhotonNetwork.PlayerList.Length)
                {
                    raceFinished = true;
                    geriSayým.StopAllCoroutines(); // Geri sayýmý durdur
                    GameOver(); // Tüm oyuncular yakalandýðýnda oyunu bitir
                }
            }
        }
    }

    private void GameOver()
    {
        raceFinished = true;
        gameOverPanel.SetActive(true);

        // Yakalananlar için sýralý ödül ve yakalanmayanlar için sabit ödül hesaplama
        int caughtReward = 250; // Ýlk yakalanan için baþlangýç ödülü
        List<Player> uncaughtPlayers = new List<Player>();
        List<Player> caughtPlayers = new List<Player>();

        // Yakalanan ve yakalanmayanlarý ayýr
        foreach (Player player in PhotonNetwork.PlayerList)
        {
            if (!finishOrder.Contains(player))
            {
                uncaughtPlayers.Add(player); // Yakalanmayanlar listesine ekle
            }
            else
            {
                caughtPlayers.Add(player); // Yakalananlar listesine ekle
            }
        }

        // Yakalanmayanlara 1000 ödül ver
        foreach (Player player in uncaughtPlayers)
        {
            UpdatePlayerUI(player, 1000); // UI güncelleme (1000 ödül)
            CoinManager.Instance.AddCoins(1000); // 1000 ödül ekle
        }

        // Yakalananlara sýralý ödül ver
        for (int i = 0; i < caughtPlayers.Count; i++)
        {
            Player caughtPlayer = caughtPlayers[i];
            UpdatePlayerUI(caughtPlayer, caughtReward); // Yakalananlar için UI güncelleme
            CoinManager.Instance.AddCoins(caughtReward); // Sýraya göre ödül ekle
            caughtReward += 250; // Her sýradaki oyuncuya ödül 250 artar
        }
    }

    private void UpdatePlayerUI(Player player, int reward)
    {
        int playerIndex = PhotonNetwork.PlayerList.ToList().IndexOf(player);
        if (playerIndex >= 0 && playerIndex < profileImages.Length)
        {
            profileImages[playerIndex].sprite = GetProfileSprite(player);
            rewardTexts[playerIndex].text = $"{reward}"; // Ödül metnini güncelle
            nickNames[playerIndex].text = player.NickName; // Oyuncunun ismini güncelle
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
