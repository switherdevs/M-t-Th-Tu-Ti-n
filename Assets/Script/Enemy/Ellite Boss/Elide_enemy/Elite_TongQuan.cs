using System.Collections;
using UnityEngine;

public class Elite_TongQuan : MonoBehaviour
{
    public enum State { Idle, Chasing, Attacking, JumpSlam }

    [Header("--- THÔNG SỐ DI CHUYỂN & TẦM ---")]
    [Tooltip("Khoảng cách bắt đầu vung đao đánh thường")]
    [SerializeField] private float attackRange = 1.8f;
    [SerializeField] private float moveSpeed = 3.5f;
    [Tooltip("Layer nhận diện Player")]
    [SerializeField] private LayerMask playerLayer;

    [Header("--- KỸ NĂNG JUMP SLAM ---")]
    [SerializeField] private float skillCooldown = 8f;
    [SerializeField] private float slamRadius = 2.5f;
    [SerializeField] private float jumpHeight = 3f;
    [SerializeField] private float jumpDuration = 0.5f;
    [SerializeField] private float damage = 25f;

    [Header("--- CẤU HÌNH TRỰC QUAN ---")]
    [SerializeField] private Transform slamHitboxPoint;

    private State currentState = State.Idle;
    private Transform playerTransform;
    private Animator animator;
    private float skillTimer;
    private bool isDead = false;

    private void Start()
    {
        animator = GetComponentInChildren<Animator>();
        skillTimer = skillCooldown;
    }

    private void Update()
    {
        if (isDead) return;

        if (skillTimer > 0) skillTimer -= Time.deltaTime;

        FindPlayer();
        if (playerTransform == null) return;

        switch (currentState)
        {
            case State.Idle:
            case State.Chasing:
                HandleMovementAndState();
                break;
        }
    }

    private void FindPlayer()
    {
        if (playerTransform != null) return;
        Collider2D hit = Physics2D.OverlapCircle(transform.position, 10f, playerLayer);
        if (hit != null) playerTransform = hit.transform;
    }

    private void HandleMovementAndState()
    {
        float distance = Vector2.Distance(transform.position, playerTransform.position);
        FlipTowards(playerTransform.position);

        if (skillTimer <= 0)
        {
            StartCoroutine(Routine_JumpSlam());
            return;
        }

        if (distance <= attackRange)
        {
            StartCoroutine(Routine_NormalAttack());
        }
        else
        {
            currentState = State.Chasing;
            animator.SetBool("isWalking", true);
            transform.position = Vector2.MoveTowards(transform.position, playerTransform.position, moveSpeed * Time.deltaTime);
        }
    }

    private IEnumerator Routine_NormalAttack()
    {
        currentState = State.Attacking;
        animator.SetBool("isWalking", false);
        animator.SetTrigger("Attack");

        yield return new WaitForSeconds(1.0f);
        currentState = State.Idle;
    }

    private IEnumerator Routine_JumpSlam()
    {
        currentState = State.JumpSlam;
        skillTimer = skillCooldown;
        animator.SetBool("isWalking", false);
        animator.SetTrigger("PrepareJump");

        yield return new WaitForSeconds(0.3f);

        Vector3 startPos = transform.position;
        Vector3 targetPos = playerTransform.position;
        float elapsed = 0f;

        animator.SetTrigger("JumpAir");

        while (elapsed < jumpDuration)
        {
            elapsed += Time.deltaTime;
            float percent = elapsed / jumpDuration;
            Vector3 currentPos = Vector3.Lerp(startPos, targetPos, percent);
            currentPos.y += Mathf.Sin(percent * Mathf.PI) * jumpHeight;
            transform.position = currentPos;
            yield return null;
        }

        ExecuteSlamDamage();
        animator.SetTrigger("Land");

        yield return new WaitForSeconds(0.5f);
        currentState = State.Idle;
    }

    private void ExecuteSlamDamage()
    {
        Vector3 point = slamHitboxPoint != null ? slamHitboxPoint.position : transform.position;
        Collider2D[] targets = Physics2D.OverlapCircleAll(point, slamRadius, playerLayer);
        
        foreach (var target in targets)
        {
            var stats = target.GetComponent<CharacterStats>();
            if (stats != null) stats.TakeDamage(damage * 1.5f);
        }
    }

    private void FlipTowards(Vector3 target)
    {
        Vector3 scale = transform.localScale;
        scale.x = target.x > transform.position.x ? Mathf.Abs(scale.x) : -Mathf.Abs(scale.x);
        transform.localScale = scale;
    }

    private void OnDrawGizmosSelected()
    {
        // 1. Tầm đánh thường
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, attackRange);

        // 2. Bán kính Dậm Đất (Slam Radius)
        Gizmos.color = Color.red;
        Vector3 slamPoint = slamHitboxPoint != null ? slamHitboxPoint.position : transform.position;
        Gizmos.DrawWireSphere(slamPoint, slamRadius);
    }
}