using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemyAI : MonoBehaviour
{
    [Header("References")]
    public NavMeshAgent ai;
    public Transform player;
    private PlayerHealth playerHealth;

    [Header("Patrol")]
    public List<Transform> destinations;
    public float walkSpeed = 2f;
    public float chaseSpeed = 4f;
    public float minIdleTime = 2f;
    public float maxIdleTime = 5f;

    [Header("Chase Settings")]
    public float minChaseTime = 5f;
    public float maxChaseTime = 10f;
    public float sightDistance = 15f;
    public float catchDistance = 2f;
    public Vector3 raycastOffset = Vector3.up;

    [Header("Combat")]
    public float damageAmount = 10f;
    public float attackPauseTime = 5f;

    [Header("Vision Memory")]
    public float memoryTime = 3f; // seconds enemy keeps chasing after losing sight

    [Header("Vision Settings")]
    [Range(0, 180)] public float fieldOfView = 70f;

    // mask-related flag (other scripts can toggle via SetIgnorePlayer)
    [Header("Mask Interaction")]
    public bool ignorePlayer = false;

    private bool walking = true;
    public bool chasing = false;
    private bool canMove = true;
    private bool hasDealtDamage = false;
    private bool isIdle = false;

    private Transform currentDest;
    private Coroutine chaseRoutine;
    private Coroutine idleRoutine;

    void Start()
    {
        if (player != null)
            playerHealth = player.GetComponent<PlayerHealth>();

        PickNewDestination();
        Debug.Log("[EnemyAI] State: START, now Walking");
    }

    void Update()
    {
        if (!canMove) return;

        bool playerVisible = CanSeePlayer();

        // --- CHASE START ---
        if (playerVisible && !chasing)
        {
            // Reset coroutines/states but avoid killing all coroutines (e.g. attack coroutine)
            ResetAllCoroutines();

            chasing = true;
            walking = false;
            isIdle = false;
            ai.isStopped = false;

            chaseRoutine = StartCoroutine(ChaseRoutine());
            Debug.Log("[EnemyAI] State: Player spotted, now Chasing (forced resume)");
        }

        // --- CHASING BEHAVIOR ---
        if (chasing && canMove)
        {
            ai.speed = chaseSpeed;
            ai.destination = player.position;

            if (!hasDealtDamage && ai.remainingDistance <= catchDistance)
            {
                StartCoroutine(AttackPlayer());
            }
        }

        // --- PATROLLING BEHAVIOR ---
        if (walking && canMove)
        {
            ai.speed = walkSpeed;
            if (currentDest != null)
                ai.destination = currentDest.position;

            if (ai.remainingDistance <= ai.stoppingDistance && !isIdle)
            {
                if (Random.value < 0.5f)
                {
                    PickNewDestination();
                    Debug.Log("[EnemyAI] State: Walking, now New destination");
                }
                else
                {
                    idleRoutine = StartCoroutine(StayIdle());
                }
            }
        }
    }

    // --- Vision Check ---
    bool CanSeePlayer()
    {
        if (player == null || ignorePlayer) return false;

        Vector3 directionToPlayer = (player.position - transform.position).normalized;

        // --- FOV check ---
        float angleToPlayer = Vector3.Angle(transform.forward, directionToPlayer);
        if (angleToPlayer > fieldOfView * 0.5f)
            return false; // player is outside vision cone

        // --- Distance check ---
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        if (distanceToPlayer > sightDistance)
            return false; // too far

        // --- Raycast check (line of sight) ---
        if (Physics.Raycast(transform.position + raycastOffset, directionToPlayer, out RaycastHit hit, sightDistance))
        {
            return hit.collider.CompareTag("Player");
        }

        return false;
    }

    // --- Idle Coroutine ---
    IEnumerator StayIdle()
    {
        isIdle = true;
        ai.isStopped = true;

        float idleTime = Random.Range(minIdleTime, maxIdleTime);
        Debug.Log("[EnemyAI] State: Walking, now Idle (" + idleTime + "s)");

        float timer = 0f;
        while (timer < idleTime)
        {
            if (chasing) yield break; // cancel idle immediately if chase starts
            timer += Time.deltaTime;
            yield return null;
        }

        ai.isStopped = false;
        isIdle = false;
        walking = true;
        PickNewDestination();

        Debug.Log("[EnemyAI] State: Idle finished, now Walking");
        idleRoutine = null;
    }

    // --- Chase Coroutine ---
    IEnumerator ChaseRoutine()
    {
        float chaseTime = Random.Range(minChaseTime, maxChaseTime);
        float elapsed = 0f;

        float timeSinceLastSeen = 0f;

        while (elapsed < chaseTime)
        {
            if (CanSeePlayer())
            {
                timeSinceLastSeen = 0f; // reset memory timer
            }
            else
            {
                timeSinceLastSeen += Time.deltaTime;
                if (timeSinceLastSeen > memoryTime)
                    break;
            }

            elapsed += Time.deltaTime;
            yield return null;
        }

        chasing = false;
        walking = true;
        PickNewDestination();

        Debug.Log("[EnemyAI] State: Chase ended, now Walking");
        chaseRoutine = null;
    }

    // --- Attack Sequence ---
    IEnumerator AttackPlayer()
    {
        hasDealtDamage = true;
        canMove = false;
        ai.isStopped = true;

        Debug.Log("[EnemyAI] State: ATTACK, now Pause for " + attackPauseTime + "s");

        if (playerHealth != null)
            playerHealth.TakeDamage(damageAmount);

        yield return new WaitForSeconds(attackPauseTime);

        ai.isStopped = false;
        canMove = true;
        hasDealtDamage = false;

        // Resume correct state depending on visibility
        if (CanSeePlayer())
        {
            chasing = true;
            walking = false;
            ResetAllCoroutines();
            chaseRoutine = StartCoroutine(ChaseRoutine());
            Debug.Log("[EnemyAI] State: Attack pause over, now Chasing again");
        }
        else
        {
            chasing = false;
            walking = true;
            PickNewDestination();
            Debug.Log("[EnemyAI] State: Attack pause over, now Walking");
        }
    }

    // --- Reset all coroutines and movement (safer) ---
    private void ResetAllCoroutines()
    {
        if (chaseRoutine != null)
        {
            StopCoroutine(chaseRoutine);
            chaseRoutine = null;
        }
        if (idleRoutine != null)
        {
            StopCoroutine(idleRoutine);
            idleRoutine = null;
        }
        // intentionally don't StopAllCoroutines() to avoid killing Attack coroutine, etc.
        isIdle = false;
        ai.isStopped = false;
    }

    // --- Public stopChase for external callers (e.g. HidingPlace) ---
    public void stopChase()
    {
        // Stop chase coroutine if running
        if (chaseRoutine != null)
        {
            StopCoroutine(chaseRoutine);
            chaseRoutine = null;
        }

        // Cancel idle as well (we want the enemy to resume patrolling)
        if (idleRoutine != null)
        {
            StopCoroutine(idleRoutine);
            idleRoutine = null;
        }

        chasing = false;
        walking = true;
        isIdle = false;
        ai.isStopped = false;
        canMove = true;
        hasDealtDamage = false;

        PickNewDestination();

        Debug.Log("[EnemyAI] stopChase called, now Walking");
    }

    // --- Called by other scripts to toggle ignore flag (e.g., mask behavior) ---
    public void SetIgnorePlayer(bool state)
    {
        ignorePlayer = state;
    }

    // --- Public API: Handle mask pause (keeps behavior identical to your single-script) ---
    public void HandleMaskPause(float pauseTime)
    {
        StartCoroutine(PauseAndReturnToPatrol(pauseTime));
    }

    private IEnumerator PauseAndReturnToPatrol(float pauseTime)
    {
        Debug.Log("[EnemyAI] Mask effect triggered, enemy stopping for " + pauseTime + "s");

        // Stop all current actions
        ResetAllCoroutines();
        ai.isStopped = true;
        canMove = false;
        chasing = false;

        // Ignore player while masked
        ignorePlayer = true;

        // Wait for mask effect duration
        yield return new WaitForSeconds(pauseTime);

        // Resume patrol movement
        ai.isStopped = false;
        canMove = true;
        walking = true;
        PickNewDestination();

        // Re-enable detection
        ignorePlayer = false;

        Debug.Log("[EnemyAI] Mask pause over, now Walking again");


        if (CanSeePlayer())
        {
            Debug.Log("[EnemyAI] Player spotted right after mask expired!");
            ResetAllCoroutines();
            chasing = true;
            walking = false;
            ai.isStopped = false;
            chaseRoutine = StartCoroutine(ChaseRoutine());
        }
    }

    // --- Pick Random Destination ---
    void PickNewDestination()
    {
        if (destinations == null || destinations.Count == 0) return;
        currentDest = destinations[Random.Range(0, destinations.Count)];
        if (ai != null && currentDest != null)
            ai.destination = currentDest.position;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, sightDistance);

        if (player == null) return;

        Gizmos.color = Color.red;
        Vector3 start = transform.position + raycastOffset;
        Vector3 dir = (player.position - start).normalized;
        Gizmos.DrawLine(start, start + dir * sightDistance);
        Gizmos.DrawWireSphere(start, 0.1f);

        if (!player) return;

        Gizmos.color = new Color(0f, 1f, 0f, 0.25f);
        Vector3 leftBoundary = Quaternion.Euler(0, -fieldOfView * 0.5f, 0) * transform.forward;
        Vector3 rightBoundary = Quaternion.Euler(0, fieldOfView * 0.5f, 0) * transform.forward;
        Gizmos.DrawRay(transform.position, leftBoundary * sightDistance);
        Gizmos.DrawRay(transform.position, rightBoundary * sightDistance);
    }
}
