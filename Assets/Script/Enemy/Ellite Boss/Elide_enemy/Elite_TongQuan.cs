using System.Collections;
using UnityEngine;

public class Elite_TongQuan : MonoBehaviour
{
    [Header("--- TẦM NHÌN & TẤN CÔNG ---")]
    [SerializeField] private float detectionRange = 10f;
    [SerializeField] private float attackRange = 1.8f;
    [SerializeField] private Vector2 attackOffset = Vector2.zero;
    [SerializeField] private float moveSpeed = 3.5f;
    [SerializeField] private LayerMask playerLayer;

    [Header("--- TÌM ĐƯỜNG & NÉ VẬT CẢN ---")]
    [SerializeField] private LayerMask obstacleLayer;
    [SerializeField] private float avoidRadius = 0.6f;

    [Header("--- KỸ NĂNG JUMP SLAM ---")]
    [SerializeField] private float skillCooldown = 8f;
    [SerializeField] private float windupTime = 1.5f;
    [SerializeField] private float slowMultiplier = 0.4f;
    [SerializeField] private float slamRadius = 2.5f;
    [SerializeField] private float jumpHeight = 3f;
    [SerializeField] private float jumpDuration = 0.5f;
    [SerializeField] private float damage = 25f;
    [SerializeField] private Transform slamHitboxPoint;

    [Header("--- ANIMATION STRINGS ---")]
    [SerializeField] private string animWalk = "isWalking";
    [SerializeField] private string animAttack = "Attack";
    [SerializeField] private string animPrepareJump = "PrepareJump";
    [SerializeField] private string animJumpAir = "JumpAir";
    [SerializeField] private string animLand = "Land";

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
            StartCoroutine(Routine_JumpSlam());
        }
        else if (distance <= attackRange)
        {
            StartCoroutine(Routine_NormalAttack());
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

    private IEnumerator Routine_NormalAttack()
    {
        isBusy = true;
        animator.SetBool(animWalk, false);
        animator.SetTrigger(animAttack);

        yield return new WaitForSeconds(1.0f);
        isBusy = false;
    }

    private IEnumerator Routine_JumpSlam()
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
        animator.SetTrigger(animPrepareJump);

        yield return new WaitForSeconds(0.3f);
        animator.SetTrigger(animJumpAir);

        Vector3 startPos = transform.position;
        Vector3 targetPos = playerTransform.position;
        float elapsed = 0f;

        while (elapsed < jumpDuration)
        {
            elapsed += Time.deltaTime;
            float percent = elapsed / jumpDuration;
            Vector3 currentPos = Vector3.Lerp(startPos, targetPos, percent);
            currentPos.y += Mathf.Sin(percent * Mathf.PI) * jumpHeight;
            transform.position = currentPos;
            yield return null;
        }

        Vector3 hitPoint = slamHitboxPoint != null ? slamHitboxPoint.position : transform.position;
        Collider2D[] targets = Physics2D.OverlapCircleAll(hitPoint, slamRadius, playerLayer);
        foreach (var target in targets)
        {
            target.SendMessage("TakeDamage", damage * 1.5f, SendMessageOptions.DontRequireReceiver);
        }

        animator.SetTrigger(animLand);
        yield return new WaitForSeconds(0.5f);
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