using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AbilitySpawner : MonoBehaviour
{
    // Colliderlarýn yer aldýðý boþ gameObject'lerin transform'larý
    public Transform[] spawnPoints;

    // Yetenek prefab'larý
    public GameObject[] abilities;  // Temel yetenekler (Örneðin Hýz, Can, Saldýrý)

    // Her 10 saniyede bir rastgele iki yeteneði spawn eder.
    public float spawnInterval = 10f;

    private void Start()
    {
        // Spawn iþlemini baþlat
        StartCoroutine(SpawnAbilities());
    }

    private IEnumerator SpawnAbilities()
    {
        while (true)
        {
            // Collider'larýn olduðu noktalardan rastgele iki tane seç
            List<Transform> chosenSpawnPoints = ChooseRandomSpawnPoints(2, 1.5f);  // Minimum 1.5 birim mesafe þartý

            // Temel yeteneklerden rastgele iki tanesini seç
            List<GameObject> chosenAbilities = ChooseRandomAbilities(2);

            // Seçilen yetenekleri, seçilen collider'larýn bulunduðu noktalara spawn et
            for (int i = 0; i < chosenAbilities.Count; i++)
            {
                Instantiate(chosenAbilities[i], chosenSpawnPoints[i].position, Quaternion.identity);
            }

            // 10 saniye bekle
            yield return new WaitForSeconds(spawnInterval);
        }
    }

    // Rastgele iki tane spawn point seçer ve aralarýndaki mesafeyi kontrol eder
    private List<Transform> ChooseRandomSpawnPoints(int count, float minDistance)
    {
        List<Transform> chosenPoints = new List<Transform>();
        List<Transform> availablePoints = new List<Transform>(spawnPoints);

        for (int i = 0; i < count; i++)
        {
            Transform selectedPoint = null;
            bool validPoint = false;

            while (!validPoint && availablePoints.Count > 0)
            {
                int randomIndex = Random.Range(0, availablePoints.Count);
                selectedPoint = availablePoints[randomIndex];

                // Ýlk noktayý seçtikten sonra mesafeyi kontrol ederiz
                if (i == 0 || IsFarEnough(selectedPoint, chosenPoints, minDistance))
                {
                    validPoint = true;
                    chosenPoints.Add(selectedPoint);
                    availablePoints.RemoveAt(randomIndex);
                }
            }
        }

        return chosenPoints;
    }

    // Seçilen noktanýn önceki noktalara yeterli uzaklýkta olup olmadýðýný kontrol eder
    private bool IsFarEnough(Transform point, List<Transform> chosenPoints, float minDistance)
    {
        foreach (Transform chosenPoint in chosenPoints)
        {
            if (Vector3.Distance(point.position, chosenPoint.position) < minDistance)
            {
                return false;
            }
        }

        return true;
    }

    // Rastgele iki tane yetenek prefab'ý seçer
    private List<GameObject> ChooseRandomAbilities(int count)
    {
        List<GameObject> chosenAbilities = new List<GameObject>();
        List<GameObject> availableAbilities = new List<GameObject>(abilities);

        for (int i = 0; i < count; i++)
        {
            int randomIndex = Random.Range(0, availableAbilities.Count);
            chosenAbilities.Add(availableAbilities[randomIndex]);
            availableAbilities.RemoveAt(randomIndex);  // Ayný yeteneðin tekrar seçilmemesi için çýkarýyoruz.
        }

        return chosenAbilities;
    }
}
