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
    public float memoryTime = 3f;

    [Header("Vision Settings")]
    [Range(0, 180)] public float fieldOfView = 70f;

    [Header("Mask Interaction")]
    public bool ignorePlayer = false;

    [Header("Animation")]
    public Animator animator; // single Animator with Walking/Idle animation

    private bool chasing = false;
    private bool canMove = true;
    private bool hasDealtDamage = false;
    private Transform currentDest;
    private Coroutine chaseRoutine;
    private Coroutine idleRoutine;

    void Start()
    {
        if (player == null)
        {
            Debug.LogError("[EnemyAI] Player reference NOT assigned!");
            return;
        }

        playerHealth = player.GetComponent<PlayerHealth>();

        if (playerHealth == null)
            Debug.LogError("[EnemyAI] PlayerHealth component NOT found on player!");

        if (animator == null)
            animator = GetComponent<Animator>();

        PickNewDestination();
    }

    void Update()
    {

        Debug.Log($"Update running | chasing={chasing} | canMove={canMove}");

        if (!canMove) return;

        // Check for attack
        if (!hasDealtDamage && player != null)
        {
            float distanceToPlayer = Vector3.Distance(transform.position, player.position);
            if (distanceToPlayer <= catchDistance)
            {
                StartCoroutine(AttackPlayer());
            }
        }

        bool playerVisible = CanSeePlayer();

        // Start chase
        if (playerVisible && !chasing)
        {
            ResetAllCoroutines();
            chasing = true;
            ai.isStopped = false;
            chaseRoutine = StartCoroutine(ChaseRoutine());
        }

        // Chase behavior
        if (chasing && canMove)
        {
            ai.speed = chaseSpeed;
            if (!ai.pathPending)
                ai.SetDestination(player.position);

            float distanceToPlayer = Vector3.Distance(transform.position, player.position);
            if (!hasDealtDamage && distanceToPlayer <= catchDistance)
                StartCoroutine(AttackPlayer());
        }

        // Patrol behavior
        if (!chasing && canMove)
        {
            ai.speed = walkSpeed;
            if (currentDest != null)
                ai.destination = currentDest.position;

            if (!ai.pathPending && ai.remainingDistance <= ai.stoppingDistance && idleRoutine == null)
            {
                // Randomly idle or move to new destination
                if (Random.value < 0.5f)
                    PickNewDestination();
                else
                    idleRoutine = StartCoroutine(IdleRoutine());
            }
        }

        // Update animation based on movement
        UpdateAnimator();
    }

    void UpdateAnimator()
    {
        if (animator == null) return;

        bool isMoving = ai.remainingDistance > ai.stoppingDistance && ai.velocity.magnitude > 0.05f;
        animator.SetBool("IsWalking", isMoving);
    }

    IEnumerator IdleRoutine()
    {
        ai.isStopped = true;
        float idleTime = Random.Range(minIdleTime, maxIdleTime);
        float timer = 0f;
        while (timer < idleTime)
        {
            if (chasing) yield break;
            timer += Time.deltaTime;
            yield return null;
        }
        ai.isStopped = false;
        PickNewDestination();
        idleRoutine = null;
    }

    IEnumerator ChaseRoutine()
    {
        float chaseTime = Random.Range(minChaseTime, maxChaseTime);
        float elapsed = 0f;
        float timeSinceLastSeen = 0f;

        while (elapsed < chaseTime)
        {
            if (CanSeePlayer())
                timeSinceLastSeen = 0f;
            else
            {
                timeSinceLastSeen += Time.deltaTime;
                if (timeSinceLastSeen > memoryTime) break;
            }

            elapsed += Time.deltaTime;
            yield return null;
        }

        chasing = false;
        PickNewDestination();
        chaseRoutine = null;
    }

    IEnumerator AttackPlayer()
    {
        hasDealtDamage = true;
        canMove = false;
        ai.isStopped = true;

        if (playerHealth != null)
            playerHealth.TakeDamage(damageAmount);

        yield return new WaitForSeconds(attackPauseTime);

        ai.isStopped = false;
        canMove = true;
        hasDealtDamage = false;

        if (CanSeePlayer())
        {   
            chasing = true;
            ResetAllCoroutines();
            chaseRoutine = StartCoroutine(ChaseRoutine());
        }
        else
        {
            chasing = false;
            PickNewDestination();
        }
    }

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
        ai.isStopped = false;
    }

    public void stopChase()
    {
        ResetAllCoroutines();
        chasing = false;
        ai.isStopped = false;
        PickNewDestination();
    }

    public void SetIgnorePlayer(bool state) => ignorePlayer = state;

    public void HandleMaskPause(float pauseTime)
    {
        StartCoroutine(PauseAndReturnToPatrol(pauseTime));
    }

    private IEnumerator PauseAndReturnToPatrol(float pauseTime)
    {
        ResetAllCoroutines();
        ai.isStopped = true;
        canMove = false;
        chasing = false;
        ignorePlayer = true;

        yield return new WaitForSeconds(pauseTime);

        ai.isStopped = false;
        canMove = true;
        PickNewDestination();
        ignorePlayer = false;

        if (CanSeePlayer())
        {
            ResetAllCoroutines();
            chasing = true;
            chaseRoutine = StartCoroutine(ChaseRoutine());
        }
    }

    void PickNewDestination()
    {
        if (destinations == null || destinations.Count == 0) return;
        currentDest = destinations[Random.Range(0, destinations.Count)];
        if (ai != null && currentDest != null)
            ai.destination = currentDest.position;
    }

    bool CanSeePlayer()
    {
        if (player == null) return false;

        Vector3 dir = (player.position - transform.position).normalized;
        float distance = Vector3.Distance(transform.position, player.position);

        if (distance > sightDistance) return false;

        float angle = Vector3.Angle(transform.forward, dir);
        if (angle > fieldOfView * 0.5f) return false;

        if (Physics.Raycast(transform.position + raycastOffset, dir, out RaycastHit hit, sightDistance))
        {
            // Must hit the Player collider
            return hit.collider.CompareTag("Player");
        }

        return false;
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

        Gizmos.color = new Color(0f, 1f, 0f, 0.25f);
        Vector3 leftBoundary = Quaternion.Euler(0, -fieldOfView * 0.5f, 0) * transform.forward;
        Vector3 rightBoundary = Quaternion.Euler(0, fieldOfView * 0.5f, 0) * transform.forward;
        Gizmos.DrawRay(transform.position, leftBoundary * sightDistance);
        Gizmos.DrawRay(transform.position, rightBoundary * sightDistance);
    }
}
