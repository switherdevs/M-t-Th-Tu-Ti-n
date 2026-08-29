using System.Collections;
using UnityEngine;

public class Boss_MocGiaoYeuVuong : MonoBehaviour
{
    [Header("--- ĐÁNH THƯỜNG (SWING) ---")]
    [SerializeField] private float attackRange = 3f;
    [SerializeField] private float moveSpeed = 2f;
    [SerializeField] private LayerMask playerLayer;

    [Header("--- SKILL: TRIPLE WOOD ORB LINE ---")]
    [SerializeField] private float skillCooldown = 7f;
    [SerializeField] private SimpleObjectPool orbPool;
    [SerializeField] private Transform mouthPoint;
    [SerializeField] private float orbSpeed = 10f;

    private Transform playerTransform;
    private Animator animator;
    private float skillTimer;
    private bool isBusy = false;

    private void Start()
    {
        animator = GetComponentInChildren<Animator>();
        skillTimer = skillCooldown;
    }

    private void Update()
    {
        if (isBusy) return;
        FindPlayer();
        if (playerTransform == null) return;

        if (skillTimer > 0) skillTimer -= Time.deltaTime;

        float distance = Vector2.Distance(transform.position, playerTransform.position);

        if (skillTimer <= 0)
        {
            StartCoroutine(Routine_TripleWoodOrbLine());
        }
        else if (distance <= attackRange)
        {
            StartCoroutine(Routine_TailSwing());
        }
        else
        {
            animator.SetBool("isMoving", true);
            transform.position = Vector2.MoveTowards(transform.position, playerTransform.position, moveSpeed * Time.deltaTime);
        }
    }

    private void FindPlayer()
    {
        if (playerTransform != null) return;
        Collider2D hit = Physics2D.OverlapCircle(transform.position, 15f, playerLayer);
        if (hit != null) playerTransform = hit.transform;
    }

    private IEnumerator Routine_TailSwing()
    {
        isBusy = true;
        animator.SetBool("isMoving", false);
        animator.SetTrigger("TailSwing");

        yield return new WaitForSeconds(1.0f);
        isBusy = false;
    }

    private IEnumerator Routine_TripleWoodOrbLine()
    {
        isBusy = true;
        skillTimer = skillCooldown;
        animator.SetBool("isMoving", false);
        animator.SetTrigger("SpitStart");

        yield return new WaitForSeconds(0.4f);

        Vector3 spawnPos = mouthPoint != null ? mouthPoint.position : transform.position;
        Vector2 targetDirection = playerTransform != null ? (Vector2)(playerTransform.position - spawnPos).normalized : (Vector2)transform.right;

        for (int i = 0; i < 3; i++)
        {
            if (orbPool != null)
            {
                GameObject orb = orbPool.GetFromPool(spawnPos, Quaternion.identity);
                orb.GetComponent<Rigidbody2D>().linearVelocity = targetDirection * orbSpeed;
            }
            yield return new WaitForSeconds(0.25f);
        }

        yield return new WaitForSeconds(0.5f);
        isBusy = false;
    }

    private void OnDrawGizmosSelected()
    {
        // 1. Tầm quật đuôi diện rộng (AoE Swing)
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);

        // 2. Vị trí miệng phun Cầu Gỗ
        if (mouthPoint != null)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(mouthPoint.position, 0.2f);
        }
    }
}