using UnityEngine;
using System.Collections.Generic;

public class SwordDamage : MonoBehaviour
{
    public float damageAmount = 20f;
    public bool isSwinging = false;

    private List<GameObject> hitVictims = new List<GameObject>();

    void Update()
    {
        if (!isSwinging)
        {
            hitVictims.Clear();
        }
    }

    void OnTriggerEnter(Collider other)
    {
        // 1. Check if we are swinging
        if (!isSwinging) return;

        // 2. LAYER FILTER (The Fix)
        // If we hit a wall, floor, or the Player's Capsule (PlayerMovement layer),
        // IGNORE IT completely. Only stop for "BodyPart" layer.
        if (other.gameObject.layer != LayerMask.NameToLayer("BodyPart"))
        {
            return;
        }

        // 3. Identify the Root Parent
        GameObject victimRoot = other.transform.root.gameObject;

        // 4. Ignore Self
        if (victimRoot == transform.root.gameObject) return;

        // 5. THE "ONE HIT" CHECK
        if (hitVictims.Contains(victimRoot))
        {
            return;
        }

        // 6. Valid Hit! Add to list.
        hitVictims.Add(victimRoot);

        Debug.Log("Sword hit BodyPart: " + other.name); // Changed log to be clearer

        // 7. Deal Damage
        Hitbox hitbox = other.GetComponent<Hitbox>();
        if (hitbox != null)
        {
            hitbox.OnHit(damageAmount);
        }
    }
}