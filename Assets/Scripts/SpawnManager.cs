using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class SpawnManager : MonoBehaviourPunCallbacks
{
    public Transform[] spawnPoints; // TrapPG sahnesindeki baþlangýç noktalarý

    private void Awake()
    {
        SpawnPlayers();   
    }

    private void SpawnPlayers()
    {
        int spawnPointIndex = PhotonNetwork.LocalPlayer.ActorNumber % spawnPoints.Length;
        Transform spawnPoint = spawnPoints[spawnPointIndex];
        Vector3 spawnPosition = spawnPoint.position;
        Quaternion spawnRotation = spawnPoint.rotation;

        // Oyuncu prefab'ýný baþlatýn
        PhotonNetwork.Instantiate(PlayerPrefs.GetString("LastEquippedCharacter"), spawnPosition, spawnRotation);
    }
}
