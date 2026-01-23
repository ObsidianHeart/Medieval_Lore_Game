using UnityEngine;

public class SwordDamage : MonoBehaviour
{
    public float damageAmount = 20f;
    public bool isSwinging = false; // We will turn this on/off during animation

    // This function runs automatically when the sword touches another trigger/collider
    void OnTriggerEnter(Collider other)
    {
        // 1. Only deal damage if we are actually swinging
        if (!isSwinging) return;

        // 2. Check if we hit a Body Part (using the Layer we set up earlier)
        if (other.gameObject.layer == LayerMask.NameToLayer("BodyPart"))
        {
            // 3. Find the Hitbox script on the body part
            Hitbox hitbox = other.GetComponent<Hitbox>();

            if (hitbox != null)
            {
                hitbox.OnHit(damageAmount);

                // Optional: Turn off damage immediately so we don't hit the same arm 10 times in one frame
                isSwinging = false;
            }
        }
    }
}