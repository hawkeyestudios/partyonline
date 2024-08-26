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
            List<Transform> chosenSpawnPoints = ChooseRandomSpawnPoints(2);

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

    // Rastgele iki tane spawn point seçer
    private List<Transform> ChooseRandomSpawnPoints(int count)
    {
        List<Transform> chosenPoints = new List<Transform>();
        List<Transform> availablePoints = new List<Transform>(spawnPoints);

        for (int i = 0; i < count; i++)
        {
            int randomIndex = Random.Range(0, availablePoints.Count);
            chosenPoints.Add(availablePoints[randomIndex]);
            availablePoints.RemoveAt(randomIndex);  // Ayný spawn point'in tekrar seçilmemesi için çýkarýyoruz.
        }

        return chosenPoints;
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
