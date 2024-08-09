using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class TrapPGSpawnSystem : MonoBehaviourPunCallbacks
{
    public Transform[] spawnPoints; // 4 adet spawn point noktasýnýn referansý

    private void Start()
    {
        SpawnCharacter();
    }

    private void SpawnCharacter()
    {
        // Her oyuncunun kendine ait bir spawn noktasý olmasý gerekiyor
        int spawnIndex = PhotonNetwork.LocalPlayer.ActorNumber - 1;

        if (spawnIndex < spawnPoints.Length)
        {
            string lastEquippedCharacter = PhotonNetwork.LocalPlayer.CustomProperties["LastEquippedCharacter"] as string ?? "DefaultCharacterPrefabName";

            GameObject characterPrefab = Resources.Load<GameObject>(lastEquippedCharacter);
            if (characterPrefab != null)
            {
                GameObject characterInstance = PhotonNetwork.Instantiate(characterPrefab.name, spawnPoints[spawnIndex].position, Quaternion.identity);
            }
            else
            {
                Debug.LogError("Character prefab not found.");
            }
        }
        else
        {
            Debug.LogError("Spawn point index out of range.");
        }
    }
}
