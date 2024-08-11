using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class TrapPGSpawnSystem : MonoBehaviourPunCallbacks
{
    public Transform[] spawnPoints; // 4 adet spawn point noktasýnýn referansý
    private bool[] occupiedSpawnPoints; // Hangi spawn pointlerin dolu olduðunu izlemek için

    private void Start()
    {
        occupiedSpawnPoints = new bool[spawnPoints.Length];

        if (PhotonNetwork.IsConnectedAndReady)
        {
            SpawnCharacter();
        }
        else
        {
            Debug.LogError("PhotonNetwork is not connected.");
        }
    }

    private void SpawnCharacter()
    {
        int localPlayerIndex = GetLocalPlayerIndex();

        if (localPlayerIndex >= 0 && localPlayerIndex < spawnPoints.Length)
        {
            int spawnIndex = GetAvailableSpawnPoint(localPlayerIndex);

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
        else
        {
            Debug.LogError("Spawn point index out of range or invalid player index.");
        }
    }

    // Mevcut oyuncunun indeksini almak için yardýmcý fonksiyon
    private int GetLocalPlayerIndex()
    {
        Player[] players = PhotonNetwork.PlayerList;
        for (int i = 0; i < players.Length; i++)
        {
            if (players[i].IsLocal)
            {
                return i;
            }
        }
        return -1;
    }

    // Uygun bir spawn noktasý seçmek için yardýmcý fonksiyon
    private int GetAvailableSpawnPoint(int preferredIndex)
    {
        if (!occupiedSpawnPoints[preferredIndex])
        {
            occupiedSpawnPoints[preferredIndex] = true;
            return preferredIndex;
        }

        for (int i = 0; i < occupiedSpawnPoints.Length; i++)
        {
            if (!occupiedSpawnPoints[i])
            {
                occupiedSpawnPoints[i] = true;
                return i;
            }
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
