using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class TrapPGSpawnSystem : MonoBehaviourPunCallbacks
{
    public Transform[] spawnPoints; // 4 adet spawn point noktasýnýn referansý

    private void Start()
    {
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
        // Oyuncularýn lobiye giriþ yaptýklarýndan emin olmak için PhotonNetwork.PlayerList'i kullanýn
        int localPlayerIndex = GetLocalPlayerIndex();

        if (localPlayerIndex >= 0 && localPlayerIndex < spawnPoints.Length)
        {
            // "LastEquippedCharacter" özelliðini doðru ayarladýðýnýzdan emin olun
            string lastEquippedCharacter = PhotonNetwork.LocalPlayer.CustomProperties.ContainsKey("LastEquippedCharacter")
                ? PhotonNetwork.LocalPlayer.CustomProperties["LastEquippedCharacter"] as string
                : "DefaultCharacterPrefabName";

            GameObject characterPrefab = Resources.Load<GameObject>(lastEquippedCharacter);

            if (characterPrefab != null)
            {
                // `PhotonNetwork.Instantiate` kullanarak karakteri oluþturun
                GameObject characterInstance = PhotonNetwork.Instantiate(characterPrefab.name, spawnPoints[localPlayerIndex].position, Quaternion.identity);
                Debug.Log($"Character instantiated at spawn point {localPlayerIndex}.");
            }
            else
            {
                Debug.LogError($"Character prefab '{lastEquippedCharacter}' not found.");
            }
        }
        else
        {
            Debug.LogError("Spawn point index out of range or invalid player index.");
        }
    }

    // Lokal oyuncunun indeksini almak için yardýmcý fonksiyon
    private int GetLocalPlayerIndex()
    {
        Player[] players = PhotonNetwork.PlayerList;
        for (int i = 0; i < players.Length; i++)
        {
            if (players[i].IsLocal)
            {
                return i; // Lokal oyuncunun indeksini döndür
            }
        }
        return -1; // Lokal oyuncu bulunamadý
    }
}
