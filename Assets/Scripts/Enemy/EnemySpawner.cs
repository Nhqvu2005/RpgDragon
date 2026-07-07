using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RPGDragon.Enemy
{
    [System.Serializable]
    public class EnemyWaveData
    {
        public string waveName = "New Wave";
        public WaveEntry[] entries;

        [System.Serializable]
        public struct WaveEntry
        {
            public int enemyPrefabIndex;
            public int spawnPointIndex;
            public float delayBeforeSpawn;
        }
    }

    public class EnemySpawner : MonoBehaviour
    {
        [Header("Prefabs & Points")]
        [SerializeField] private GameObject[] enemyPrefabs;
        [SerializeField] private Transform[] spawnPoints;

        [Header("Object Pooling")]
        [SerializeField] private bool useObjectPool = true;
        [SerializeField] private int defaultPoolSize = 10;

        [Header("Boss Settings")]
        [SerializeField] private GameObject bossPrefab;
        [SerializeField] private Transform bossSpawnPoint;
        [SerializeField] private GameObject bossRoomDoor;

        private Dictionary<int, Queue<GameObject>> objectPools = new Dictionary<int, Queue<GameObject>>();

        private void Awake()
        {
            if (useObjectPool)
                InitializePools();
        }

        #region Object Pool

        private void InitializePools()
        {
            for (int i = 0; i < enemyPrefabs.Length; i++)
            {
                if (enemyPrefabs[i] == null) continue;

                int prefabIndex = i;
                if (!objectPools.ContainsKey(prefabIndex))
                {
                    objectPools[prefabIndex] = new Queue<GameObject>();
                }

                for (int j = 0; j < defaultPoolSize; j++)
                {
                    GameObject pooledObj = CreatePooledObject(prefabIndex);
                    objectPools[prefabIndex].Enqueue(pooledObj);
                }
            }
        }

        private GameObject CreatePooledObject(int prefabIndex)
        {
            if (prefabIndex < 0 || prefabIndex >= enemyPrefabs.Length) return null;
            if (enemyPrefabs[prefabIndex] == null) return null;

            GameObject obj = Instantiate(enemyPrefabs[prefabIndex], transform);
            obj.SetActive(false);
            return obj;
        }

        private GameObject GetFromPool(int prefabIndex)
        {
            if (!useObjectPool)
                return null;

            if (!objectPools.ContainsKey(prefabIndex))
            {
                objectPools[prefabIndex] = new Queue<GameObject>();
            }

            Queue<GameObject> pool = objectPools[prefabIndex];

            while (pool.Count > 0)
            {
                GameObject obj = pool.Dequeue();
                if (obj != null)
                {
                    return obj;
                }
            }

            // Pool exhausted, create a new object
            GameObject newObj = CreatePooledObject(prefabIndex);
            if (newObj != null)
                pool.Enqueue(newObj);

            return newObj;
        }

        private void ReturnToPool(GameObject obj, int prefabIndex)
        {
            if (!useObjectPool) return;

            obj.SetActive(false);
            obj.transform.SetParent(transform);

            if (!objectPools.ContainsKey(prefabIndex))
            {
                objectPools[prefabIndex] = new Queue<GameObject>();
            }

            objectPools[prefabIndex].Enqueue(obj);
        }

        #endregion

        #region Spawning

        /// <summary>
        /// Spawn a single enemy at a specific spawn point.
        /// </summary>
        /// <param name="enemyIndex">Index in the enemyPrefabs array.</param>
        /// <param name="spawnPointIndex">Index in the spawnPoints array.</param>
        /// <returns>The spawned GameObject, or null on failure.</returns>
        public GameObject SpawnEnemy(int enemyIndex, int spawnPointIndex)
        {
            if (!ValidateIndices(enemyIndex, spawnPointIndex))
                return null;

            GameObject enemy = GetOrInstantiate(enemyIndex);

            if (enemy == null) return null;

            Transform spawnPoint = spawnPoints[spawnPointIndex];
            enemy.transform.position = spawnPoint.position;
            enemy.transform.rotation = spawnPoint.rotation;
            enemy.SetActive(true);

            return enemy;
        }

        /// <summary>
        /// Spawn an enemy at a specific world position (no spawn point required).
        /// </summary>
        public GameObject SpawnEnemyAtPosition(int enemyIndex, Vector3 position, Quaternion rotation)
        {
            if (enemyIndex < 0 || enemyIndex >= enemyPrefabs.Length)
            {
                Debug.LogWarning($"EnemySpawner: enemyIndex {enemyIndex} is out of range.");
                return null;
            }

            if (enemyPrefabs[enemyIndex] == null)
            {
                Debug.LogWarning($"EnemySpawner: enemyPrefabs[{enemyIndex}] is null.");
                return null;
            }

            GameObject enemy = GetOrInstantiate(enemyIndex);

            if (enemy == null) return null;

            enemy.transform.position = position;
            enemy.transform.rotation = rotation;
            enemy.SetActive(true);

            return enemy;
        }

        /// <summary>
        /// Spawn a wave of enemies based on EnemyWaveData.
        /// </summary>
        public void SpawnWave(EnemyWaveData wave)
        {
            if (wave == null || wave.entries == null || wave.entries.Length == 0)
            {
                Debug.LogWarning("EnemySpawner: Cannot spawn wave — no entries defined.");
                return;
            }

            StartCoroutine(SpawnWaveCoroutine(wave));
        }

        private IEnumerator SpawnWaveCoroutine(EnemyWaveData wave)
        {
            foreach (EnemyWaveData.WaveEntry entry in wave.entries)
            {
                if (entry.delayBeforeSpawn > 0f)
                    yield return new WaitForSeconds(entry.delayBeforeSpawn);

                SpawnEnemy(entry.enemyPrefabIndex, entry.spawnPointIndex);
            }
        }

        /// <summary>
        /// Spawn a random enemy at a random spawn point.
        /// </summary>
        public GameObject SpawnRandomEnemy()
        {
            if (enemyPrefabs == null || enemyPrefabs.Length == 0 ||
                spawnPoints == null || spawnPoints.Length == 0)
            {
                Debug.LogWarning("EnemySpawner: Not enough prefabs or spawn points for random spawn.");
                return null;
            }

            int randomEnemyIndex = Random.Range(0, enemyPrefabs.Length);
            int randomSpawnIndex = Random.Range(0, spawnPoints.Length);

            return SpawnEnemy(randomEnemyIndex, randomSpawnIndex);
        }

        #endregion

        #region Boss

        /// <summary>
        /// Trigger boss encounter: spawn boss, lock the door.
        /// Called when player enters the boss room trigger area.
        /// </summary>
        public void TriggerBoss()
        {
            if (bossPrefab == null)
            {
                Debug.LogWarning("EnemySpawner: No bossPrefab assigned.");
                return;
            }

            Transform spawnPos = bossSpawnPoint != null ? bossSpawnPoint : transform;

            GameObject boss = Instantiate(bossPrefab, spawnPos.position, spawnPos.rotation);
            boss.SetActive(true);

            // Lock the door (disable interaction or close it)
            if (bossRoomDoor != null)
            {
                // Example: disable the door collider to block passage,
                // or activate a wall object
                Collider2D doorCollider = bossRoomDoor.GetComponent<Collider2D>();
                if (doorCollider != null)
                    doorCollider.isTrigger = false;

                // Alternatively, play a close animation
                Animator doorAnimator = bossRoomDoor.GetComponent<Animator>();
                if (doorAnimator != null)
                    doorAnimator.SetTrigger("Close");
            }

            Debug.Log("EnemySpawner: Boss spawned, door locked.");
        }

        #endregion

        #region Cleanup

        /// <summary>
        /// Despawn all active spawned enemies.
        /// </summary>
        public void DespawnAll()
        {
            EnemyBase[] enemies = FindObjectsOfType<EnemyBase>();

            foreach (EnemyBase enemy in enemies)
            {
                if (useObjectPool)
                {
                    // Find the prefab index for pooling
                    for (int i = 0; i < enemyPrefabs.Length; i++)
                    {
                        if (enemyPrefabs[i] != null &&
                            enemy.gameObject.name.Contains(enemyPrefabs[i].name))
                        {
                            ReturnToPool(enemy.gameObject, i);
                            break;
                        }
                    }
                }
                else
                {
                    Destroy(enemy.gameObject);
                }
            }
        }

        #endregion

        #region Helpers

        private bool ValidateIndices(int enemyIndex, int spawnPointIndex)
        {
            if (enemyPrefabs == null || spawnPoints == null)
            {
                Debug.LogWarning("EnemySpawner: enemyPrefabs or spawnPoints arrays are null.");
                return false;
            }

            if (enemyIndex < 0 || enemyIndex >= enemyPrefabs.Length)
            {
                Debug.LogWarning($"EnemySpawner: enemyIndex {enemyIndex} is out of range (0-{enemyPrefabs.Length - 1}).");
                return false;
            }

            if (spawnPointIndex < 0 || spawnPointIndex >= spawnPoints.Length)
            {
                Debug.LogWarning($"EnemySpawner: spawnPointIndex {spawnPointIndex} is out of range (0-{spawnPoints.Length - 1}).");
                return false;
            }

            if (enemyPrefabs[enemyIndex] == null)
            {
                Debug.LogWarning($"EnemySpawner: enemyPrefabs[{enemyIndex}] is null.");
                return false;
            }

            if (spawnPoints[spawnPointIndex] == null)
            {
                Debug.LogWarning($"EnemySpawner: spawnPoints[{spawnPointIndex}] is null.");
                return false;
            }

            return true;
        }

        private GameObject GetOrInstantiate(int enemyIndex)
        {
            GameObject enemy = null;

            if (useObjectPool)
            {
                enemy = GetFromPool(enemyIndex);
            }

            if (enemy == null)
            {
                enemy = Instantiate(enemyPrefabs[enemyIndex], transform);
            }

            return enemy;
        }

        #endregion
    }
}
