using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [Header("Spawn Settings")]
    public GameObject enemyPrefab;
    public Transform spawnPoint;

    private GameObject currentEnemy;

    public void SpawnEnemy()
    {
        if (enemyPrefab != null && spawnPoint != null)
        {
            if (currentEnemy != null)
            {
                Debug.LogWarning("[EnemySpawner] Enemy already active — skipping spawn.");
                return;
            }

            currentEnemy = Instantiate(enemyPrefab, spawnPoint.position, spawnPoint.rotation);
            Debug.Log($"[EnemySpawner] Enemy spawned at {spawnPoint.position} (Object: {currentEnemy.name})");
        }
        else
        {
            Debug.LogWarning("[EnemySpawner] Missing prefab or spawn point — cannot spawn enemy.");
        }
    }

    public void DespawnEnemy()
    {
        if (currentEnemy != null)
        {
            Debug.Log($"[EnemySpawner] Enemy despawned ({currentEnemy.name}).");
            Destroy(currentEnemy);
            currentEnemy = null;
        }
        else
        {
            Debug.LogWarning("[EnemySpawner] Tried to despawn, but no enemy was active.");
        }
    }
}
