using UnityEngine;
using UnityEngine.AI;

public class SlowChaser : MonoBehaviour
{
    [Header("Settings")]
    public Transform player;       // Reference to the player
    public float speed = 1.5f;     // Movement speed
    public float stoppingDistance = 0.5f; // Optional: distance to stop near player

    private NavMeshAgent agent;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.speed = speed;
        agent.stoppingDistance = stoppingDistance;
        agent.angularSpeed = 120f; // Smooth turning
        agent.acceleration = 2f;   // Slow acceleration for creepiness
    }

    void Update()
    {
        if (player != null)
        {
            // Always move toward the player's current position
            agent.SetDestination(player.position);
        }
    }
}
