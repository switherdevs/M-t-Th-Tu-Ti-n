using System.Collections;
using UnityEngine;

public class Boss_UMinhThiDe : MonoBehaviour
{
    [Header("--- THÔNG SỐ CƠ BẢN ---")]
    [SerializeField] private float attackRange = 2.2f;
    [SerializeField] private float moveSpeed = 1.8f;
    [SerializeField] private LayerMask playerLayer;

    [Header("--- KỸ NĂNG SUMMON SOULS ---")]
    [SerializeField] private float skillCooldown = 12f;
    [SerializeField] private SimpleObjectPool soulPool;
    [SerializeField] private float summonDistance = 6f;

    private Transform playerTransform;
    private Animator animator;
    private float skillTimer;
    private bool isCasting = false;

    private void Start()
    {
        animator = GetComponentInChildren<Animator>();
        skillTimer = skillCooldown;
    }

    private void Update()
    {
        if (isCasting) return;
        FindPlayer();
        if (playerTransform == null) return;

        if (skillTimer > 0) skillTimer -= Time.deltaTime;

        float distance = Vector2.Distance(transform.position, playerTransform.position);

        if (skillTimer <= 0)
        {
            StartCoroutine(Routine_SummonSouls());
        }
        else if (distance <= attackRange)
        {
            StartCoroutine(Routine_SwordSlash());
        }
        else
        {
            animator.SetBool("isWalking", true);
            transform.position = Vector2.MoveTowards(transform.position, playerTransform.position, moveSpeed * Time.deltaTime);
        }
    }

    private void FindPlayer()
    {
        if (playerTransform != null) return;
        Collider2D hit = Physics2D.OverlapCircle(transform.position, 18f, playerLayer);
        if (hit != null) playerTransform = hit.transform;
    }

    private IEnumerator Routine_SwordSlash()
    {
        isCasting = true;
        animator.SetBool("isWalking", false);
        animator.SetTrigger("Slash");

        yield return new WaitForSeconds(0.9f);
        isCasting = false;
    }

    private IEnumerator Routine_SummonSouls()
    {
        isCasting = true;
        skillTimer = skillCooldown;
        animator.SetBool("isWalking", false);
        animator.SetTrigger("SummonCast");

        yield return new WaitForSeconds(0.8f);

        Vector3[] spawnOffsets = {
            Vector3.up * summonDistance,
            Vector3.down * summonDistance,
            Vector3.left * summonDistance,
            Vector3.right * summonDistance
        };

        foreach (Vector3 offset in spawnOffsets)
        {
            Vector3 spawnPos = transform.position + offset;
            if (soulPool != null)
            {
                GameObject soul = soulPool.GetFromPool(spawnPos, Quaternion.identity);
                HomingSoul homingScript = soul.GetComponent<HomingSoul>();
                if (homingScript != null)
                {
                    homingScript.SetTarget(playerTransform);
                }
            }
        }

        yield return new WaitForSeconds(0.6f);
        isCasting = false;
    }

    private void OnDrawGizmosSelected()
    {
        // 1. Tầm chém cận chiến
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);

        // 2. Vòng tròn khoảng cách xuất hiện 4 Oán Linh
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, summonDistance);

        // 3. Đánh dấu 4 điểm Spawn Oán Linh (Trên, Dưới, Trái, Phải)
        Vector3[] points = {
            transform.position + Vector3.up * summonDistance,
            transform.position + Vector3.down * summonDistance,
            transform.position + Vector3.left * summonDistance,
            transform.position + Vector3.right * summonDistance
        };

        Gizmos.color = Color.magenta;
        foreach (var p in points)
        {
            Gizmos.DrawSphere(p, 0.3f);
        }
    }
}