using System.Collections;
using UnityEngine;

[RequireComponent(typeof(EnemyAI))]
public class EnemyMaskReaction : MonoBehaviour
{
    private EnemyAI enemy;
    private bool isPaused = false;

    void Awake()
    {
        enemy = GetComponent<EnemyAI>();
    }

    // Called by MaskController (or any manager) when the player uses the mask
    public void OnPlayerMasked(float pauseTime)
    {
        if (!isPaused)
            StartCoroutine(PauseAndNotify(pauseTime));
    }

    private IEnumerator PauseAndNotify(float pauseTime)
    {
        isPaused = true;

        // Ensure the enemy fully stops chasing (clears chase coroutines and resets states)
        enemy.stopChase();

        // Tell EnemyAI to ignore player while masked
        enemy.SetIgnorePlayer(true);

        // Stop the NavMeshAgent movement
        if (enemy.ai != null)
            enemy.ai.isStopped = true;

        // Wait for pause duration
        yield return new WaitForSeconds(pauseTime);

        // Resume navmesh movement and stop ignoring player
        if (enemy.ai != null)
            enemy.ai.isStopped = false;

        enemy.SetIgnorePlayer(false);

        isPaused = false;
    }
}
