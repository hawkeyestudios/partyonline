using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class AbilitySpawner : MonoBehaviour
{
    public Collider[] spawnAreas; 
    public string[] abilities;
    public float spawnInterval = 10f;

    private void Start()
    {
        StartCoroutine(SpawnAbilities());
    }

    private IEnumerator SpawnAbilities()
    {
        while (true)
        {
            List<Vector3> chosenSpawnPositions = ChooseRandomSpawnPositions(2, 1.5f);
            List<string> chosenAbilities = ChooseRandomAbilities(2);

            for (int i = 0; i < chosenAbilities.Count; i++)
            {
                PhotonNetwork.Instantiate(chosenAbilities[i], chosenSpawnPositions[i], Quaternion.identity);
            }

            yield return new WaitForSeconds(spawnInterval);
        }
    }

    private List<Vector3> ChooseRandomSpawnPositions(int count, float minDistance)
    {
        List<Vector3> chosenPositions = new List<Vector3>();

        for (int i = 0; i < count; i++)
        {
            Vector3 selectedPosition = Vector3.zero;
            bool validPosition = false;

            while (!validPosition)
            {
                Collider selectedCollider = spawnAreas[Random.Range(0, spawnAreas.Length)];

                selectedPosition = GetRandomPositionInCollider(selectedCollider);

                if (i == 0 || IsFarEnough(selectedPosition, chosenPositions, minDistance))
                {
                    validPosition = true;
                    chosenPositions.Add(selectedPosition);
                }
            }
        }

        return chosenPositions;
    }

    private Vector3 GetRandomPositionInCollider(Collider collider)
    {
        Vector3 boundsMin = collider.bounds.min;
        Vector3 boundsMax = collider.bounds.max;

        float x = Random.Range(boundsMin.x, boundsMax.x);
        float y = Random.Range(boundsMin.y, boundsMax.y);
        float z = Random.Range(boundsMin.z, boundsMax.z);

        Vector3 randomPosition = new Vector3(x, y, z);
        if (collider.bounds.Contains(randomPosition))
        {
            return randomPosition;
        }
        else
        {
            randomPosition.y = boundsMin.y;
            return randomPosition;
        }
    }

    private bool IsFarEnough(Vector3 position, List<Vector3> chosenPositions, float minDistance)
    {
        foreach (Vector3 chosenPosition in chosenPositions)
        {
            if (Vector3.Distance(position, chosenPosition) < minDistance)
            {
                return false;
            }
        }
        return true;
    }

    private List<string> ChooseRandomAbilities(int count)
    {
        List<string> chosenAbilities = new List<string>();
        List<string> availableAbilities = new List<string>(abilities);

        for (int i = 0; i < count; i++)
        {
            int randomIndex = Random.Range(0, availableAbilities.Count);
            chosenAbilities.Add(availableAbilities[randomIndex]);
            availableAbilities.RemoveAt(randomIndex);
        }

        return chosenAbilities;
    }
}
