using UnityEngine;
using System.Collections;

public class CurtainController : MonoBehaviour
{
    [Header("Curtain Objects")]
    public GameObject curtain1Closed;
    public GameObject curtain2Closed;
    public GameObject curtain1Open;
    public GameObject curtain2Open;

    [Header("Timing Settings")]
    [Tooltip("Minimum delay before curtains open (seconds)")]
    public float minDelay = 10f;
    [Tooltip("Maximum delay before curtains open (seconds)")]
    public float maxDelay = 20f;

    [Header("Player Interaction")]
    [Tooltip("The player GameObject")]
    public Transform player;
    [Tooltip("How close the player must be to interact")]
    public float interactDistance = 3f;

    [Header("Enemy Spawn Settings")]
    [Tooltip("The enemy prefab to spawn")]
    public GameObject enemyPrefab;
    [Tooltip("Where the enemy should appear when the window opens")]
    public Transform enemySpawnPoint;
    [Tooltip("Should a new enemy spawn every time the curtains open?")]
    public bool spawnEveryTime = false;

    private bool curtainsOpened = false;
    private Coroutine openCoroutine;
    private GameObject spawnedEnemy;

    void Start()
    {
        // Start with curtains closed
        CloseCurtains(false);

        // Start the first auto-open timer
        StartOpenTimer();
    }

    void Update()
    {
        // Player closes curtains if near and presses C
        if (curtainsOpened && player != null)
        {
            float distance = Vector3.Distance(player.position, transform.position);
            if (distance <= interactDistance && Input.GetKeyDown(KeyCode.C))
            {
                CloseCurtains(true); // true = restart open timer
            }
        }
    }

    void StartOpenTimer()
    {
        if (openCoroutine != null) StopCoroutine(openCoroutine);
        openCoroutine = StartCoroutine(OpenAfterDelay());
    }

    IEnumerator OpenAfterDelay()
    {
        float delay = Random.Range(minDelay, maxDelay);
        yield return new WaitForSeconds(delay);
        OpenCurtains();
    }

    void OpenCurtains()
    {
        if (curtainsOpened) return;

        // Swap curtain states
        if (curtain1Closed) curtain1Closed.SetActive(false);
        if (curtain2Closed) curtain2Closed.SetActive(false);
        if (curtain1Open) curtain1Open.SetActive(true);
        if (curtain2Open) curtain2Open.SetActive(true);

        curtainsOpened = true;

        // Spawn the enemy
        TrySpawnEnemy();
    }

    void CloseCurtains(bool restartTimer)
    {
        if (!curtainsOpened) return;

        // Swap curtain states
        if (curtain1Closed) curtain1Closed.SetActive(true);
        if (curtain2Closed) curtain2Closed.SetActive(true);
        if (curtain1Open) curtain1Open.SetActive(false);
        if (curtain2Open) curtain2Open.SetActive(false);

        curtainsOpened = false;

        // Despawn enemy if it exists
        if (spawnedEnemy != null)
        {
            Destroy(spawnedEnemy);
            spawnedEnemy = null;
        }

        // Restart the auto-open timer if desired
        if (restartTimer)
            StartOpenTimer();
    }

    void TrySpawnEnemy()
    {
        if (enemyPrefab == null || enemySpawnPoint == null)
            return;

        // If spawnEveryTime is false, only spawn once
        if (spawnedEnemy != null && !spawnEveryTime)
            return;

        // Spawn a new enemy
        spawnedEnemy = Instantiate(enemyPrefab, enemySpawnPoint.position, enemySpawnPoint.rotation);
    }

    // Draw interaction and spawn gizmos
    void OnDrawGizmosSelected()
    {
        // Player interaction radius
        Gizmos.color = new Color(0f, 0.6f, 1f, 0.5f);
        Gizmos.DrawWireSphere(transform.position, interactDistance);

        // Enemy spawn point
        if (enemySpawnPoint != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(enemySpawnPoint.position, 0.3f);
            Gizmos.DrawLine(transform.position, enemySpawnPoint.position);
        }
    }
}
