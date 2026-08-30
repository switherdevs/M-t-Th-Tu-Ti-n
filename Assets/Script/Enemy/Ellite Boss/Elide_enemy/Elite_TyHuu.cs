using System.Collections;
using UnityEngine;

public class Elite_TyHuu : MonoBehaviour
{
    [Header("--- TẦM NHÌN & TẤN CÔNG ---")]
    [SerializeField] private float detectionRange = 12f;
    [SerializeField] private float attackRange = 1.5f;
    [SerializeField] private Vector2 attackOffset = Vector2.zero;
    [SerializeField] private float moveSpeed = 2.5f;
    [SerializeField] private LayerMask playerLayer;

    [Header("--- TÌM ĐƯỜNG & NÉ VẬT CẢN ---")]
    [SerializeField] private LayerMask obstacleLayer;
    [SerializeField] private float avoidRadius = 0.5f;

    [Header("--- KỸ NĂNG STONE BREATH ---")]
    [SerializeField] private float skillCooldown = 6f;
    [SerializeField] private float windupTime = 1.5f;
    [SerializeField] private float slowMultiplier = 0.4f;
    [SerializeField] private Transform mouthPoint;
    [SerializeField] private SimpleObjectPool stoneProjectilePool;
    [SerializeField] private float projectileSpeed = 8f;

    [Header("--- ANIMATION STRINGS ---")]
    [SerializeField] private string animWalk = "isWalking";
    [SerializeField] private string animClaw1 = "Claw1";
    [SerializeField] private string animClaw2 = "Claw2";
    [SerializeField] private string animRoar = "Roar";

    private Transform playerTransform;
    private Animator animator;
    private float skillTimer;

    private bool isBusy = false;
    private bool isWindingUp = false;

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

        Vector3 attackCenter = GetAttackCenter();
        float distance = Vector2.Distance(attackCenter, playerTransform.position);
        FlipTowards(playerTransform.position);

        if (isWindingUp)
        {
            MoveSmoothly(playerTransform.position, moveSpeed);
            return;
        }

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
            animator.SetBool(animWalk, true);
            MoveSmoothly(playerTransform.position, moveSpeed);
        }
    }

    private void FindPlayer()
    {
        if (playerTransform != null)
        {
            if (Vector2.Distance(transform.position, playerTransform.position) > detectionRange * 1.5f)
            {
                playerTransform = null;
                animator.SetBool(animWalk, false);
            }
            return;
        }

        Collider2D hit = Physics2D.OverlapCircle(transform.position, detectionRange, playerLayer);
        if (hit != null) playerTransform = hit.transform;
    }

    private void MoveSmoothly(Vector3 targetPosition, float speed)
    {
        Vector2 currentPos = transform.position;
        Vector2 dirToTarget = ((Vector2)targetPosition - currentPos).normalized;
        Vector2 moveDir = dirToTarget;

        RaycastHit2D hit = Physics2D.CircleCast(currentPos, avoidRadius, dirToTarget, 1f, obstacleLayer);
        if (hit.collider != null && !hit.collider.isTrigger)
        {
            Vector2 slideDir = Vector2.Perpendicular(hit.normal).normalized;
            if (Vector2.Dot(dirToTarget, slideDir) < 0) slideDir = -slideDir;
            moveDir = (dirToTarget + slideDir * 1.5f).normalized;
        }

        transform.position += (Vector3)(moveDir * (speed * Time.deltaTime));
    }

    private IEnumerator Routine_DoubleClaw()
    {
        isBusy = true;
        animator.SetBool(animWalk, false);
        
        animator.SetTrigger(animClaw1);
        yield return new WaitForSeconds(0.4f);
        
        animator.SetTrigger(animClaw2);
        yield return new WaitForSeconds(0.6f);

        isBusy = false;
    }

    private IEnumerator Routine_StoneBreath()
    {
        isWindingUp = true;
        skillTimer = skillCooldown;
        
        float originalSpeed = moveSpeed;
        float originalAnimSpeed = animator.speed;
        moveSpeed *= slowMultiplier;
        animator.speed *= slowMultiplier;

        yield return new WaitForSeconds(windupTime);

        isWindingUp = false;
        isBusy = true;
        moveSpeed = originalSpeed;
        animator.speed = originalAnimSpeed;

        animator.SetBool(animWalk, false);
        animator.SetTrigger(animRoar);

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
        isBusy = false;
    }

    private void FlipTowards(Vector3 target)
    {
        Vector3 scale = transform.localScale;
        scale.x = target.x > transform.position.x ? Mathf.Abs(scale.x) : -Mathf.Abs(scale.x);
        transform.localScale = scale;
    }

    public Vector3 GetAttackCenter()
    {
        float direction = transform.localScale.x >= 0 ? 1f : -1f;
        return transform.position + new Vector3(attackOffset.x * direction, attackOffset.y, 0f);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(GetAttackCenter(), attackRange);
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, avoidRadius);
    }
}