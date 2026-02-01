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

    private void Start()
    {
        DeadImage.enabled = false;
    }
    public void TakeDamage(float damage)
    {
        Debug.Log("TakeDamage called with: " + damage);
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
