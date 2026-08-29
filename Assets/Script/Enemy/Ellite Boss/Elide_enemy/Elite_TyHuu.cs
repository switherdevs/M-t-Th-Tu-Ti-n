using System.Collections;
using UnityEngine;

public class Elite_TyHuu : MonoBehaviour
{
    public enum State { Idle, Chasing, Attacking, StoneBreath }

    [Header("--- DI CHUYỂN & TẤN CÔNG ---")]
    [SerializeField] private float attackRange = 1.5f;
    [SerializeField] private float moveSpeed = 2.5f;
    [SerializeField] private LayerMask playerLayer;

    [Header("--- KỸ NĂNG STONE BREATH ---")]
    [SerializeField] private float skillCooldown = 6f;
    [SerializeField] private Transform mouthPoint;
    [SerializeField] private SimpleObjectPool stoneProjectilePool;
    [SerializeField] private float projectileSpeed = 8f;

    private State currentState = State.Idle;
    private Transform playerTransform;
    private Animator animator;
    private float skillTimer;

    private void Start()
    {
        animator = GetComponentInChildren<Animator>();
        skillTimer = skillCooldown;
    }

    private void Update()
    {
        if (skillTimer > 0) skillTimer -= Time.deltaTime;

        FindPlayer();
        if (playerTransform == null) return;

        if (currentState == State.Idle || currentState == State.Chasing)
        {
            float distance = Vector2.Distance(transform.position, playerTransform.position);
            FlipTowards(playerTransform.position);

            if (skillTimer <= 0)
            {
                StartCoroutine(Routine_StoneBreath());
            }
            else if (distance <= attackRange)
            {
                StartCoroutine(Routine_DoubleClaw());
            }
            else
            {
                currentState = State.Chasing;
                animator.SetBool("isWalking", true);
                transform.position = Vector2.MoveTowards(transform.position, playerTransform.position, moveSpeed * Time.deltaTime);
            }
        }
    }

    private void FindPlayer()
    {
        if (playerTransform != null) return;
        Collider2D hit = Physics2D.OverlapCircle(transform.position, 12f, playerLayer);
        if (hit != null) playerTransform = hit.transform;
    }

    private IEnumerator Routine_DoubleClaw()
    {
        currentState = State.Attacking;
        animator.SetBool("isWalking", false);
        
        animator.SetTrigger("Claw1");
        yield return new WaitForSeconds(0.4f);
        
        animator.SetTrigger("Claw2");
        yield return new WaitForSeconds(0.6f);

        currentState = State.Idle;
    }

    private IEnumerator Routine_StoneBreath()
    {
        currentState = State.StoneBreath;
        skillTimer = skillCooldown;
        animator.SetBool("isWalking", false);
        animator.SetTrigger("Roar");

        yield return new WaitForSeconds(0.5f);

        float[] angles = { -15f, 0f, 15f };
        Vector3 spawnPos = mouthPoint != null ? mouthPoint.position : transform.position;
        Vector2 baseDir = playerTransform != null ? (Vector2)(playerTransform.position - spawnPos).normalized : (Vector2)transform.right;

        foreach (float angle in angles)
        {
            Quaternion rotation = Quaternion.Euler(0, 0, angle);
            Vector2 finalDir = rotation * baseDir;

            if (stoneProjectilePool != null)
            {
                GameObject stone = stoneProjectilePool.GetFromPool(spawnPos, Quaternion.identity);
                Rigidbody2D rb = stone.GetComponent<Rigidbody2D>();
                if (rb != null) rb.linearVelocity = finalDir * projectileSpeed;
            }
        }

        yield return new WaitForSeconds(0.8f);
        currentState = State.Idle;
    }

    private void FlipTowards(Vector3 target)
    {
        Vector3 scale = transform.localScale;
        scale.x = target.x > transform.position.x ? Mathf.Abs(scale.x) : -Mathf.Abs(scale.x);
        transform.localScale = scale;
    }

    private void OnDrawGizmosSelected()
    {
        // 1. Tầm cào cận chiến
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);

        // 2. Điểm Phun Đá (Mouth Point)
        if (mouthPoint != null)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(mouthPoint.position, 0.2f);
        }
    }
}