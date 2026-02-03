using UnityEngine;

public class Health : MonoBehaviour
{
    public float maxHealth = 100f;
    private float currentHealth;

    void Start()
    {
        currentHealth = maxHealth;
    }

    public void TakeDamage(float amount)
    {
        currentHealth -= amount;
        Debug.Log(name + " took damage! HP Left: " + currentHealth);

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        Debug.Log(name + " died!");

        // Check if this is the Player
        if (gameObject.CompareTag("Player"))
        {
            // If the Player dies, we usually reload the scene, but for now just log it
            Debug.Log("GAME OVER! (Player would restart here)");
        }
        else
        {
            // If it's an Enemy, DELETE him instantly
            Destroy(gameObject);
        }
    }
}