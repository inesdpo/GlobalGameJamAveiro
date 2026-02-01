using UnityEngine;
using UnityEngine.Events;

public class MusicBoxController : MonoBehaviour
{
    [Header("Timing (Seconds)")]
    public float minDelay = 30f;
    public float maxDelay = 50f;

    [Header("Audio")]
    public AudioSource musicBoxAudio;

    [Header("Events")]
    public UnityEvent onMusicStart;
    public UnityEvent onMusicStop;

    [Header("Interaction")]
    [Tooltip("How close the player must be to interact (press C).")]
    public float interactionRadius = 3f;
    [Tooltip("Reference to the player object.")]
    public Transform player;

    private Coroutine timerRoutine;

    private void Start()
    {
        StartTimer();
    }

    private void Update()
    {
        if (player == null) return;

        // Check player distance
        float distance = Vector3.Distance(transform.position, player.position);
        if (distance <= interactionRadius && Input.GetKeyDown(KeyCode.C))
        {
            ResetMusicBox();
        }
    }

    private System.Collections.IEnumerator WaitAndPlay()
    {
        float waitTime = Random.Range(minDelay, maxDelay);
        Debug.Log($"[MusicBox] Waiting {waitTime:F1} seconds before playing...");
        yield return new WaitForSeconds(waitTime);

        if (musicBoxAudio != null)
            musicBoxAudio.Play();

        Debug.Log("[MusicBox] Music started — triggering spawn.");
        onMusicStart?.Invoke();
    }

    public void ResetMusicBox()
    {
        Debug.Log("[MusicBox] Player stopped the music box — resetting.");
        if (musicBoxAudio != null)
            musicBoxAudio.Stop();

        onMusicStop?.Invoke();

        if (timerRoutine != null)
            StopCoroutine(timerRoutine);

        StartTimer();
    }

    private void StartTimer()
    {
        timerRoutine = StartCoroutine(WaitAndPlay());
    }

    // --- Gizmo visualization for interaction range ---
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, interactionRadius);
    }
}
