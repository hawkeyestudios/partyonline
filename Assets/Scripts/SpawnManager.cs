using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class SpawnManager : MonoBehaviourPunCallbacks
{
    public Transform[] spawnPoints; // TrapPG sahnesindeki baþlangýç noktalarý

    private void Start()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            // Master Client, oyuncularý spawn eder
            SpawnPlayers();
        }
    }

    private void SpawnPlayers()
    {
        foreach (Player player in PhotonNetwork.PlayerList)
        {
            int spawnPointIndex = player.ActorNumber % spawnPoints.Length;
            Transform spawnPoint = spawnPoints[spawnPointIndex];
            Vector3 spawnPosition = spawnPoint.position;
            Quaternion spawnRotation = spawnPoint.rotation;

            // Oyuncu prefab'ýný baþlatýn
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
}
