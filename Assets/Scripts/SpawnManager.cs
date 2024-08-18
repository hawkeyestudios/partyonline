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

    private List<Player> finishOrder = new List<Player>();
    private bool raceFinished = false;

    private void Start()
    {
        gameOverPanel.SetActive(false);
        SpawnPlayer();
    }

    private void SpawnPlayer()
    {
        if (PhotonNetwork.IsConnectedAndReady)
        {
            Player localPlayer = PhotonNetwork.LocalPlayer;
            int spawnPointIndex = localPlayer.ActorNumber % spawnPoints.Length;
            Transform spawnPoint = spawnPoints[spawnPointIndex];
            Vector3 spawnPosition = spawnPoint.position;
            Quaternion spawnRotation = spawnPoint.rotation;

            string characterPrefab = PlayerPrefs.GetString("LastEquippedCharacter");
            if (!string.IsNullOrEmpty(characterPrefab))
            {
                PhotonNetwork.Instantiate(characterPrefab, spawnPosition, spawnRotation);
            }
            else
            {
                Debug.LogError("Character prefab not found in PlayerPrefs.");
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (raceFinished)
            return;

        if (other.CompareTag("Player"))
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
                    gameOverPanel.SetActive(true);
                    raceFinished = true;
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

            int[] rewards = { 3000, 1500, 1000, 500 };
            if (playerIndex < rewards.Length)
            {
                rewardTexts[playerIndex].text = $"{rewards[playerIndex]}";
            }

            nickNames[playerIndex].text = player.NickName;
        }
    }

    private Sprite GetProfileSprite(Player player)
    {
        string playerImageName = PlayerPrefs.GetString("LastEquippedCharacter", "defaultProfileImage");
        Sprite profileSprite = Resources.Load<Sprite>("ProfileImages/" + playerImageName);

        if (profileSprite == null)
        {
            profileSprite = Resources.Load<Sprite>("ProfileImages/defaultProfileImage");
        }

        return profileSprite;
    }
}
