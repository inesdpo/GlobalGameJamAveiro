using UnityEngine;
using UnityEngine.UI;

public class MaskController : MonoBehaviour
{
    [Header("Mask Settings")]
    public Image maskOverlay;
    public KeyCode toggleKey = KeyCode.E;
    public float maxMaskTime = 5f;
    public float cooldownTime = 5f;

    private bool maskOn = false;
    private float maskTimer = 0f;
    private float cooldownTimer = 0f;

    void Start()
    {
        if (maskOverlay != null)
            maskOverlay.enabled = false;
    }

    void Update()
    {
        if (cooldownTimer > 0f)
            cooldownTimer -= Time.deltaTime;

        if (Input.GetKeyDown(toggleKey) && cooldownTimer <= 0f)
        {
            if (!maskOn)
            {
                maskOn = true;
                maskTimer = maxMaskTime;
                if (maskOverlay != null)
                    maskOverlay.enabled = true;

                NotifyEnemiesMaskUsed();
            }
            else
            {
                DisableMask();
            }
        }

        if (maskOn)
        {
            maskTimer -= Time.deltaTime;

            if (maskTimer <= 0f)
            {
                DisableMask();
            }
        }
    }


    private void DisableMask()
    {
        maskOn = false;
        if (maskOverlay != null)
            maskOverlay.enabled = false;

        cooldownTimer = cooldownTime;
    }

    // --- Notify all enemies that the player used the mask ---
    void NotifyEnemiesMaskUsed()
    {
        EnemyAI[] enemies = Object.FindObjectsByType<EnemyAI>(FindObjectsSortMode.None);

        foreach (EnemyAI enemy in enemies)
        {
            enemy.OnPlayerMasked(3f);
        }
    }


}
