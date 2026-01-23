using UnityEngine;

public class Hitbox : MonoBehaviour
{
    // The main health script this body part belongs to
    public Health healthScript;

    // Multiplier: 1 = Normal, 2 = Double Damage, 10 = Instant Kill, 0.5 = Half Damage
    public float damageMultiplier = 1f;

    public void OnHit(float baseDamage)
    {
        float finalDamage = baseDamage * damageMultiplier;
        healthScript.TakeDamage(finalDamage);
    }
}