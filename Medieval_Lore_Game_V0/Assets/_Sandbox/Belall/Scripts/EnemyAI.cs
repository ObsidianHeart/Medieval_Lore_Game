using UnityEngine;
using UnityEngine.AI;
using System.Collections;

public class EnemyAI : MonoBehaviour
{
    public Transform player;
    public SwordDamage enemySword;     // <--- NEW: Link to the enemy's sword script
    public float attackRange = 1.5f;
    public float attackCooldown = 2.0f;

    private NavMeshAgent agent;
    private Animator animator;
    private float lastAttackTime;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();

        if (player == null)
            player = GameObject.Find("Y Bot").transform;
    }

    void Update()
    {
        float distance = Vector3.Distance(transform.position, player.position);

        // CHASE
        if (distance > attackRange)
        {
            agent.SetDestination(player.position);
            animator.SetFloat("Speed", agent.velocity.magnitude);
            animator.SetBool("CombatMode", false);
        }
        // ATTACK
        else
        {
            agent.ResetPath();
            animator.SetFloat("Speed", 0);
            FaceTarget();

            if (Time.time > lastAttackTime + attackCooldown)
            {
                StartCoroutine(AttackSequence()); // <--- CHANGED: Uses Coroutine now
                lastAttackTime = Time.time;
            }
        }
    }

    // NEW: The Sequence controls the sword damage timing
    IEnumerator AttackSequence()
    {
        // 1. Start Animation
        animator.SetBool("CombatMode", true);
        animator.SetTrigger("Attack");

        // 2. Wait for windup (sword goes back)
        yield return new WaitForSeconds(0.2f);

        // 3. TURN ON DAMAGE
        if (enemySword != null) enemySword.isSwinging = true;

        // 4. Wait for the slash to finish
        yield return new WaitForSeconds(0.5f);

        // 5. TURN OFF DAMAGE
        if (enemySword != null) enemySword.isSwinging = false;
    }

    void FaceTarget()
    {
        Vector3 direction = (player.position - transform.position).normalized;
        direction.y = 0;
        Quaternion lookRotation = Quaternion.LookRotation(direction);
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 5f);
    }
}