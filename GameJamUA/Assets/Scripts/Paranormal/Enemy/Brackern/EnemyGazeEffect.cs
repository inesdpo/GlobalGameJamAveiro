using UnityEngine;
using UnityEngine.UI;

public class EnemyGazeEffect : MonoBehaviour
{
    [Header("Proximity Settings")]
    public Image blackoutOverlay;     // UI Image to fade to black
    public float blackoutDistance = 5f; // Distance at which blackout starts
    public float timeToFullBlack = 3f;  // Time to fully black out
    public float fadeSpeed = 2f;        // Speed to fade back when player moves away

    private Transform player;
    private MaskController maskController;
    private float blackoutTimer = 0f;

    void Start()
    {
        // Find player and mask script
        player = GameObject.FindWithTag("Player")?.transform;
        maskController = player?.GetComponent<MaskController>();

        if (blackoutOverlay != null)
        {
            Color c = blackoutOverlay.color;
            c.a = 0f;
            blackoutOverlay.color = c; // start transparent
        }
    }

    void Update()
    {
        if (player == null || blackoutOverlay == null) return;

        float distance = Vector3.Distance(transform.position, player.position);

        // Only increase blackout if player is within range and mask is OFF
        if (distance <= blackoutDistance && (maskController == null || !maskController.MaskActive()))
        {
            blackoutTimer += Time.deltaTime;
        }
        else
        {
            blackoutTimer -= Time.deltaTime * fadeSpeed;
        }

        blackoutTimer = Mathf.Clamp(blackoutTimer, 0f, timeToFullBlack);

        // Apply alpha based on timer
        Color color = blackoutOverlay.color;
        color.a = blackoutTimer / timeToFullBlack;
        blackoutOverlay.color = color;
    }

    // Call when mask is used to instantly reset blackout
    public void ResetBlackout()
    {
        blackoutTimer = 0f;
        if (blackoutOverlay != null)
        {
            Color c = blackoutOverlay.color;
            c.a = 0f;
            blackoutOverlay.color = c;
        }
    }

    // Draw gizmo in editor
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, blackoutDistance);
    }
}
