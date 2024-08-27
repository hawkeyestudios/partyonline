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
    public GeriSayým geriSayým; // Geri sayým scripti

    private List<Player> finishOrder = new List<Player>();
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
        if (raceFinished)
            return;

        if (other.CompareTag("Player")) // Finish çizgisinden geçen oyunculara göre iþlem yapýlýr
        {
            Player player = other.GetComponent<PhotonView>().Owner;

            // Eðer oyuncu zaten finish sýrasýna eklenmiþse, tekrar ekleme
            if (!finishOrder.Contains(player))
            {
                finishOrder.Add(player);

                // Oyuncu sýrasýný belirleme ve ödülleri verme
                UpdatePlayerUI(player);

                // Eðer tüm oyuncular bitirdiyse yarýþý sonlandýr
                if (finishOrder.Count == PhotonNetwork.PlayerList.Length)
                {
                    raceFinished = true;
                    geriSayým.StopAllCoroutines(); // Geri sayýmý durdur
                    GameOver();
                }
            }
        }
    }

    private void UpdatePlayerUI(Player player)
    {
        int playerIndex = finishOrder.IndexOf(player);
        if (playerIndex >= 0 && playerIndex < profileImages.Length)
        {
            profileImages[playerIndex].sprite = GetProfileSprite(player);

            // Ýlk oyuncuya 1000 ödül ver, sonrakilere sýrasýyla 250 daha az ver
            int reward = Mathf.Max(1000 - (playerIndex * 250), 0); // En düþük ödül 0 olmalý

            rewardTexts[playerIndex].text = $"{reward}";
            CoinManager.Instance.AddCoins(reward);

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

    private void GameOver()
    {
        raceFinished = true;
        gameOverPanel.SetActive(true);

        // Yarýþý bitirmeyen oyuncularý da UI'da göstermek, ancak ödül vermemek için
        foreach (Player player in PhotonNetwork.PlayerList)
        {
            if (!finishOrder.Contains(player))
            {
                finishOrder.Add(player);  // Yarýþý bitirmeyen oyuncularý listeye ekle
                UpdatePlayerUI(player);  // Ancak bunlara sýfýr ödül ver
            }
        }
    }

    private void OnDestroy()
    {
        GeriSayým.OnGameOver -= GameOver;
    }
}
