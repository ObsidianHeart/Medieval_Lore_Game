using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class PlayerCombat : MonoBehaviour
{
    public Animator animator;
    public SwordDamage swordScript; // Link to the script on the sword
    public float attackCooldown = 1f;
    private bool canAttack = true;

    void Update()
    {

        if (Keyboard.current == null) return;

        // Check if R is pressed
        if (Keyboard.current.rKey.wasPressedThisFrame)
        {
            Debug.Log("R Key Pressed! Switching Modes..."); // <--- ADD THIS
            bool currentMode = animator.GetBool("CombatMode");
            animator.SetBool("CombatMode", !currentMode);
        }

        // 2. Attack (Left Mouse Button)
        // Note: We only attack if we are IN combat mode
        if (Mouse.current.leftButton.wasPressedThisFrame && canAttack && animator.GetBool("CombatMode"))
        {
            StartCoroutine(AttackSequence());
        }
    }

    IEnumerator AttackSequence()
    {
        canAttack = false;

        // Play Animation
        animator.SetTrigger("Attack");

        // Wait a tiny bit for the wind-up (e.g., 0.1s)
        yield return new WaitForSeconds(0.1f);

        // Turn ON the damage trigger
        swordScript.isSwinging = true;

        // Wait for the swing to finish (e.g., 0.5s)
        yield return new WaitForSeconds(0.5f);

        // Turn OFF the damage trigger
        swordScript.isSwinging = false;

        // Cooldown
        yield return new WaitForSeconds(attackCooldown);
        canAttack = true;
    }
}