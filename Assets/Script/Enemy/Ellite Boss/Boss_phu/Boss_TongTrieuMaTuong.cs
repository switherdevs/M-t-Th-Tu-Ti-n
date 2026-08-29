using System.Collections;
using UnityEngine;

public class Boss_TongTrieuMaTuong : MonoBehaviour
{
    public enum BossPhase { Phase1_Charge, Phase2_BowSkill }

    [Header("--- PHASE 1: CỰC TỐC CHARGE ---")]
    [SerializeField] private float chargeSpeed = 9f;
    [SerializeField] private float attackRange = 2f;
    [SerializeField] private LayerMask playerLayer;

    [Header("--- PHASE 2: CUNG MA KHÍ ---")]
    [SerializeField] private float skillCooldown = 5f;
    [SerializeField] private SimpleObjectPool arrowPool;
    [SerializeField] private Transform bowPoint;
    [SerializeField] private float arrowSpeed = 15f;

    private BossPhase currentPhase = BossPhase.Phase1_Charge;
    private Transform playerTransform;
    private Animator animator;
    private float skillTimer;
    private bool isExecutingSkill = false;

    private void Start()
    {
        animator = GetComponentInChildren<Animator>();
        skillTimer = skillCooldown;
    }

    private void Update()
    {
        if (isExecutingSkill) return;
        FindPlayer();
        if (playerTransform == null) return;

        if (skillTimer > 0) skillTimer -= Time.deltaTime;

        if (skillTimer <= 0)
        {
            StartCoroutine(Routine_GhostBowFanShot());
        }
        else
        {
            ExecuteChargePhase();
        }
    }

    private void FindPlayer()
    {
        if (playerTransform != null) return;
        Collider2D hit = Physics2D.OverlapCircle(transform.position, 15f, playerLayer);
        if (hit != null) playerTransform = hit.transform;
    }

    private void ExecuteChargePhase()
    {
        currentPhase = BossPhase.Phase1_Charge;
        float distance = Vector2.Distance(transform.position, playerTransform.position);
        
        animator.SetBool("isCharging", true);
        transform.position = Vector2.MoveTowards(transform.position, playerTransform.position, chargeSpeed * Time.deltaTime);

        if (distance <= attackRange)
        {
            StartCoroutine(Routine_SpearThrust());
        }
    }

    private IEnumerator Routine_SpearThrust()
    {
        isExecutingSkill = true;
        animator.SetTrigger("SpearThrust");
        yield return new WaitForSeconds(0.6f);
        isExecutingSkill = false;
    }

    private IEnumerator Routine_GhostBowFanShot()
    {
        isExecutingSkill = true;
        currentPhase = BossPhase.Phase2_BowSkill;
        skillTimer = skillCooldown;

        animator.SetBool("isCharging", false);
        animator.SetTrigger("DrawBow");

        yield return new WaitForSeconds(0.6f);

        float[] angles = { -20f, 0f, 20f };
        Vector3 spawnPos = bowPoint != null ? bowPoint.position : transform.position;
        Vector2 baseDir = playerTransform != null ? (Vector2)(playerTransform.position - spawnPos).normalized : (Vector2)transform.right;

        foreach (float angle in angles)
        {
            Quaternion rotation = Quaternion.Euler(0, 0, angle);
            Vector2 finalDir = rotation * baseDir;

            if (arrowPool != null)
            {
                GameObject arrow = arrowPool.GetFromPool(spawnPos, Quaternion.identity);
                arrow.GetComponent<Rigidbody2D>().linearVelocity = finalDir * arrowSpeed;
            }
        }

        yield return new WaitForSeconds(0.8f);
        isExecutingSkill = false;
    }

    private void OnDrawGizmosSelected()
    {
        // 1. Tầm đâm thương khi cưỡi ngựa
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);

        // 2. Vị trí bắn Cung Ma Khí
        if (bowPoint != null)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(bowPoint.position, 0.2f);
        }
    }
}