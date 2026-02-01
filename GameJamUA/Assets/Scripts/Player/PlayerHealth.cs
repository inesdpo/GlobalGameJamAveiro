using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PlayerHealth : MonoBehaviour
{
    [Header("Player Health Amount")]
    public float currentHealth;
    [SerializeField] private float maxHealth;

    [SerializeField] private Image DeadImage;
    public bool alive = true;

    private bool invincible = false;

    private void Start()
    {
        DeadImage.enabled = false;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            invincible = !invincible;
            Debug.Log(invincible ? "Cheat activated: Player is invincible" : "Cheat deactivated: Player can take damage");
        }
    }

    public void TakeDamage(float damage)
    {
        Debug.Log("TakeDamage called with: " + damage);

        if (invincible)
        {
            Debug.Log("Player is invincible. No damage taken.");
            return;
        }

        currentHealth -= damage;

        if (currentHealth <= 0)
        {
            currentHealth = 0;
            DeadImage.enabled = true;
            alive = false;
            Debug.Log("Player is dead");
            StartCoroutine(HandleDeath());
        }
    }

    private IEnumerator HandleDeath()
    {
        yield return new WaitForSeconds(2f);
        SceneManager.LoadScene("LostScene");
    }
}