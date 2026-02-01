using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class EnemyJumpscare : MonoBehaviour
{
    [Header("References")]
    public Transform player;            // Reference to the player
    public Image jumpscareImage;        // UI Image to show for the jumpscare
    public Camera playerCamera;         // Camera to shake

    [Header("Settings")]
    public float triggerDistance = 5f;  // Distance at which the jumpscare triggers
    public float jumpscareDuration = 2f;
    public float shakeDuration = 0.5f;
    public float shakeMagnitude = 0.3f;

    private bool isScaring = false;

    void Start()
    {
        if (jumpscareImage != null)
            jumpscareImage.enabled = false;
    }

    void Update()
    {
        if (isScaring) return;

        // Check if player is within trigger distance
        if (Vector3.Distance(player.position, transform.position) <= triggerDistance)
        {
            StartCoroutine(JumpscareRoutine());
        }
    }

    IEnumerator JumpscareRoutine()
    {
        isScaring = true;

        // Show UI jumpscare
        jumpscareImage.enabled = true;

        // Start camera shake
        StartCoroutine(ShakeCamera());

        // Wait for jumpscare duration
        yield return new WaitForSeconds(jumpscareDuration);

        // Hide image
        jumpscareImage.enabled = false;

        // Small cooldown before it can trigger again
        yield return new WaitForSeconds(2f);

        isScaring = false;
    }

    IEnumerator ShakeCamera()
    {
        Vector3 originalPos = playerCamera.transform.localPosition;
        float elapsed = 0f;

        while (elapsed < shakeDuration)
        {
            float x = Random.Range(-1f, 1f) * shakeMagnitude;
            float y = Random.Range(-1f, 1f) * shakeMagnitude;

            playerCamera.transform.localPosition = new Vector3(x, y, originalPos.z);

            elapsed += Time.deltaTime;
            yield return null;
        }

        playerCamera.transform.localPosition = originalPos;
    }

    void OnDrawGizmosSelected()
    {
        // Draw the trigger distance as a red wire sphere in the Scene view
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, triggerDistance);
    }
}
