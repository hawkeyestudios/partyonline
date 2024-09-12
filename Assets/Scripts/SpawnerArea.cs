    using UnityEngine;
    using Photon.Pun;
    using Photon.Realtime;

    public class SpawnerArea : MonoBehaviourPunCallbacks
    {
        public GameObject objectToSpawn;
        public Collider spawnArea;
        public float spawnInterval = 5f;
        public float initialDelay = 10f;

        private float timer;
        private bool initialDelayPassed = false;

        void Start()
        {
            if (spawnArea == null)
            {
                Debug.LogError("Spawn area collider is not set.");
                return;
            }

            timer = initialDelay;
        }

        void Update()
        {
            timer -= Time.deltaTime;

            if (!initialDelayPassed)
            {
                if (timer <= 0f)
                {
                    initialDelayPassed = true;
                    timer = spawnInterval;
                }
            }
            else
            {
                if (timer <= 0f)
                {
                    SpawnObjects();
                    timer = spawnInterval;
                }
            }
        }
        void SpawnObjects()
        {
            Vector3 spawnPosition1 = GetRandomPositionInCollider(spawnArea);
            Vector3 spawnPosition2 = GetRandomPositionInCollider(spawnArea);

            while (Vector3.Distance(spawnPosition1, spawnPosition2) < 1f)
            {
                spawnPosition2 = GetRandomPositionInCollider(spawnArea);
            }

            Quaternion randomRotation1 = GetRandomRotation();
            Quaternion randomRotation2 = GetRandomRotation();

            PhotonNetwork.Instantiate(objectToSpawn.name, spawnPosition1, randomRotation1);
            PhotonNetwork.Instantiate(objectToSpawn.name, spawnPosition2, randomRotation2);
        }

        Vector3 GetRandomPositionInCollider(Collider collider)
        {
            Vector3 boundsMin = collider.bounds.min;
            Vector3 boundsMax = collider.bounds.max;

            float x = Random.Range(boundsMin.x, boundsMax.x);
            float y = Random.Range(boundsMin.y, boundsMax.y);
            float z = Random.Range(boundsMin.z, boundsMax.z);

            return new Vector3(x, y, z);
        }

        Quaternion GetRandomRotation()
        {
            float randomX = Random.Range(0f, 360f);
            float randomY = Random.Range(0f, 360f);
            float randomZ = Random.Range(0f, 360f);

            return Quaternion.Euler(randomX, randomY, randomZ);
        }
    }
