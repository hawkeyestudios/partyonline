using System.Collections.Generic;
using UnityEngine;

public class GridManager : MonoBehaviour
{
    public List<Transform> spawnPoints;

    private HashSet<Transform> usedSpawnPoints = new HashSet<Transform>();

    public Transform GetRandomSpawnPoint()
    {
        List<Transform> availableSpawnPoints = new List<Transform>();

        // Kullanýlmayan spawn noktalarýný bul
        foreach (Transform spawnPoint in spawnPoints)
        {
            if (!usedSpawnPoints.Contains(spawnPoint))
            {
                availableSpawnPoints.Add(spawnPoint);
            }
        }

        // Eðer kullanýlmayan bir nokta varsa, rastgele bir tane seç
        if (availableSpawnPoints.Count > 0)
        {
            int index = Random.Range(0, availableSpawnPoints.Count);
            Transform selectedSpawnPoint = availableSpawnPoints[index];
            usedSpawnPoints.Add(selectedSpawnPoint);
            return selectedSpawnPoint;
        }
        else
        {
            Debug.LogWarning("Tüm spawn noktalarý dolu!");
            return null;
        }
    }

    public void ReleaseSpawnPoint(Transform spawnPoint)
    {
        if (usedSpawnPoints.Contains(spawnPoint))
        {
            usedSpawnPoints.Remove(spawnPoint);
        }
    }
}

