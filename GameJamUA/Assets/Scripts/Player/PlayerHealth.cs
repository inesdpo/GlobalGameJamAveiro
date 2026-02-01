using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class PlayerHealth : MonoBehaviour
{
    [Header("Player Health Amount")]
    public float currentHealth;
    [SerializeField] private float maxHealth;

    [SerializeField] private Image DeadImage = null;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        currentHealth = maxHealth;
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
            Debug.Log("Player is dead");
        }
    }

    private void Update()
    {
 
    }
}
