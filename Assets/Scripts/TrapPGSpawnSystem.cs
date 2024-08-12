using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using System.Collections.Generic;

public class TrapPGSpawnSystem : MonoBehaviourPunCallbacks
{
    public Transform[] spawnPoints; // 4 adet spawn point noktasýnýn referansý
    private bool[] occupiedSpawnPoints; // Hangi spawn pointlerin dolu olduðunu izlemek için

    private void Start()
    {
        occupiedSpawnPoints = new bool[spawnPoints.Length];

        if (PhotonNetwork.IsConnectedAndReady)
        {
            if (PhotonNetwork.IsMasterClient)
            {
                // Ana sunucu sahne yüklendiðinde tüm oyuncularý bekliyor
                PhotonNetwork.CurrentRoom.SetCustomProperties(new ExitGames.Client.Photon.Hashtable { { "AllPlayersReady", false } });
                photonView.RPC("CheckAllPlayersReady", RpcTarget.AllBuffered);
            }
        }
        else
        {
            Debug.LogError("PhotonNetwork is not connected.");
        }
    }

    [PunRPC]
    private void CheckAllPlayersReady()
    {
        if (PhotonNetwork.CurrentRoom.PlayerCount == PhotonNetwork.PlayerListOthers.Length + 1)
        {
            // Herkes sahneye geçti, karakterleri spawn et
            PhotonNetwork.CurrentRoom.SetCustomProperties(new ExitGames.Client.Photon.Hashtable { { "AllPlayersReady", true } });
            SpawnCharacter();
        }
    }

    private void SpawnCharacter()
    {
        int spawnIndex = GetRandomAvailableSpawnPoint();

        if (spawnIndex != -1)
        {
            string lastEquippedCharacter = PhotonNetwork.LocalPlayer.CustomProperties.ContainsKey("LastEquippedCharacter")
                ? PhotonNetwork.LocalPlayer.CustomProperties["LastEquippedCharacter"] as string
                : "DefaultCharacterPrefabName";

            GameObject characterPrefab = Resources.Load<GameObject>(lastEquippedCharacter);

            if (characterPrefab != null)
            {
                GameObject characterInstance = PhotonNetwork.Instantiate(characterPrefab.name, spawnPoints[spawnIndex].position, Quaternion.identity);
                Debug.Log($"Character instantiated at spawn point {spawnIndex}.");
            }
            else
            {
                Debug.LogError($"Character prefab '{lastEquippedCharacter}' not found.");
            }
        }
        else
        {
            Debug.LogError("No available spawn points.");
        }
    }

    // Uygun bir rastgele spawn noktasý seçmek için yardýmcý fonksiyon
    private int GetRandomAvailableSpawnPoint()
    {
        List<int> availableSpawnIndices = new List<int>();

        for (int i = 0; i < occupiedSpawnPoints.Length; i++)
        {
            if (!occupiedSpawnPoints[i])
            {
                availableSpawnIndices.Add(i);
            }
        }

        if (availableSpawnIndices.Count > 0)
        {
            int randomIndex = Random.Range(0, availableSpawnIndices.Count);
            int chosenSpawnIndex = availableSpawnIndices[randomIndex];
            occupiedSpawnPoints[chosenSpawnIndex] = true;
            return chosenSpawnIndex;
        }

        return -1; // Uygun bir spawn point bulunamadý
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        int playerIndex = GetPlayerIndex(otherPlayer);

        if (playerIndex != -1 && playerIndex < occupiedSpawnPoints.Length)
        {
            occupiedSpawnPoints[playerIndex] = false;
            Debug.Log($"Spawn point {playerIndex} is now available.");
        }
    }

    private int GetPlayerIndex(Player player)
    {
        Player[] players = PhotonNetwork.PlayerList;
        for (int i = 0; i < players.Length; i++)
        {
            if (players[i] == player)
            {
                return i;
            }
        }
        return -1;
    }
}
