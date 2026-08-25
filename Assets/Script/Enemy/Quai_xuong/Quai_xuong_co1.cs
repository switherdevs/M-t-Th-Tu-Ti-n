using UnityEngine;
using System.Collections;
using StatsSystem.Components;

/// <summary>
/// AI Quái cơ bản: Phát hiện người chơi -> Đi tới (có khoảng cách dừng & né tường bằng Tag Wall) -> Tấn công (Bật Hitbox sát thương).
/// Đã bổ sung: Lắng nghe trạng thái chết từ CharacterStats để ngừng di chuyển/tấn công và play animation chết.
/// ĐÃ NÂNG CẤP: Chuyển Hitbox sang dạng mảng GameObject[], hỗ trợ dịch chuyển tâm vùng tấn công và TỰ ĐỘNG ĐIỀU CHỈNH TỐC ĐỘ ĐÁNH (Attack Speed).
/// </summary>
public class BasicEnemyAI : MonoBehaviour
{
    public enum EnemyState { Idle, Chasing, Attacking, Dead }

    [Header("=== STATE ===")]
    [SerializeField] private EnemyState currentState = EnemyState.Idle;

    [Header("=== DETECTION & ATTACK RANGES ===")]
    [Tooltip("Vùng 1: Bán kính nhìn thấy người chơi để bắt đầu rượt")]
    [SerializeField] private float detectionRange = 8f;

    [Tooltip("Vùng 2: Bán kính đủ gần để tung chiêu đánh")]
    [SerializeField] private float attackRange = 1.5f;

    [Tooltip("Tọa độ lệch tâm (X, Y) của vùng tấn công so với gốc của Quái")]
    [SerializeField] private Vector2 attackCenterOffset = Vector2.zero;

    [Tooltip("Khoảng cách giữ an toàn, quái dừng lại không chui vào người Player")]
    [SerializeField] private float stopDistance = 1.2f;

    [Tooltip("Layer chứa GameObject Người chơi")]
    [SerializeField] private LayerMask playerLayer;

    [Header("=== MOVEMENT & WALL AVOIDANCE ===")]
    [Tooltip("Tốc độ di chuyển của quái")]
    [SerializeField] private float moveSpeed = 3f;

    [Tooltip("Khoảng cách tia kiểm tra tường phía trước")]
    [SerializeField] private float wallCheckDistance = 0.8f;

    [Tooltip("Tag dùng để nhận diện Tường/Vật cản (Mặc định là Wall)")]
    [SerializeField] private string wallTag = "Wall";

    [Header("=== ATTACK & DAMAGE GAMEOBJECT ===")]
    [Tooltip("Tốc độ đánh (1 = Mặc định, 2 = Đánh nhanh gấp đôi, 0.5 = Đánh chậm một nửa)")]
    [SerializeField] private float attackSpeed = 1f;

    [Tooltip("Mảng chứa các GameObject Hitbox gây sát thương")]
    [SerializeField] private GameObject[] attackHitboxes;

    [Tooltip("Thời gian chờ gốc trước khi bật Hitbox sát thương (Sẽ tự chia theo Attack Speed)")]
    [SerializeField] private float damageActiveDelay = 0.3f;

    [Tooltip("Thời gian Hitbox sát thương tồn tại gốc (Sẽ tự chia theo Attack Speed)")]
    [SerializeField] private float damageDuration = 0.4f;

    [Tooltip("Thời gian hồi chiêu giữa 2 lần đánh")]
    [SerializeField] private float attackCooldown = 2f;

    [Header("=== ANIMATION PARAMETERS ===")]
    [SerializeField] private string runAnimBool = "IsRunning";
    [SerializeField] private string attackAnimTrigger = "Attack";
    [SerializeField] private string deathAnimTrigger = "Die";

    // Biến riêng tư
    private Transform playerTransform;
    private Animator animator;
    private CharacterStats characterStats;

    private float lastAttackTime = -999f;
    private bool isAttacking = false;
    private Vector2 moveDirection;

    private void Start()
    {
        animator = GetComponentInChildren<Animator>();

        SetAllHitboxesActive(false);

        characterStats = GetComponent<CharacterStats>();
        if (characterStats != null)
        {
            characterStats.OnDeath += HandleDeath;
        }
    }

    private void OnDestroy()
    {
        if (characterStats != null)
        {
            characterStats.OnDeath -= HandleDeath;
        }
    }

    private void Update()
    {
        if (currentState == EnemyState.Dead) return;

        if (isAttacking) return;

        UpdateFacingRotation();

        ScanForPlayer();

        switch (currentState)
        {
            case EnemyState.Idle:
                SetAnimBool(runAnimBool, false);
                break;

            case EnemyState.Chasing:
                MoveTowardsPlayerWithPathfinding();
                break;

            case EnemyState.Attacking:
                SetAnimBool(runAnimBool, false);
                StartCoroutine(Routine_PerformAttack());
                break;
        }
    }

    // ==========================================
    // HÀM LẤY TÂM TẤN CÔNG THEO HƯỚNG XOAY
    // ==========================================
    /// <summary>
    /// Tính toán vị trí tâm tấn công trong không gian thế giới, tự động đảo chiều X khi quái quay mặt.
    /// </summary>
    public Vector2 GetAttackCenterPosition()
    {
        Vector2 offsetVector = transform.right * attackCenterOffset.x + transform.up * attackCenterOffset.y;
        return (Vector2)transform.position + offsetVector;
    }

    // ==========================================
    // XỬ LÝ SỰ KIỆN KHI QUÁI CHẾT
    // ==========================================
    private void HandleDeath()
    {
        currentState = EnemyState.Dead;

        StopAllCoroutines();
        isAttacking = false;

        // Reset lại tốc độ Animator về 1 khi chết
        if (animator != null) animator.speed = 1f;

        SetAllHitboxesActive(false);

        SetAnimBool(runAnimBool, false);
        SetAnimTrigger(deathAnimTrigger);
    }

    // ==========================================
    // CƠ CHẾ QUAY MẶT BẰNG ROTATION (Y-AXIS)
    // ==========================================
    private void UpdateFacingRotation()
    {
        if (playerTransform == null) return;

        if (playerTransform.position.x < transform.position.x)
        {
            transform.eulerAngles = new Vector3(0f, 180f, 0f);
        }
        else if (playerTransform.position.x > transform.position.x)
        {
            transform.eulerAngles = new Vector3(0f, 0f, 0f);
        }
    }

    // ==========================================
    // 1. QUÉT TÌM NGƯỜI CHƠI
    // ==========================================
    private void ScanForPlayer()
    {
        Collider2D hit = Physics2D.OverlapCircle(transform.position, detectionRange, playerLayer);

        if (hit != null)
        {
            playerTransform = hit.transform;

            Vector2 attackCenter = GetAttackCenterPosition();
            float distanceToAttackCenter = Vector2.Distance(attackCenter, playerTransform.position);

            if (distanceToAttackCenter <= attackRange && Time.time >= lastAttackTime + attackCooldown)
            {
                currentState = EnemyState.Attacking;
            }
            else
            {
                currentState = EnemyState.Chasing;
            }
        }
        else
        {
            playerTransform = null;
            currentState = EnemyState.Idle;
        }
    }

    // ==========================================
    // 2. DI CHUYỂN & KHOẢNG CÁCH DỪNG
    // ==========================================
    private void MoveTowardsPlayerWithPathfinding()
    {
        if (playerTransform == null) return;

        float distanceToPlayer = Vector2.Distance(transform.position, playerTransform.position);

        if (distanceToPlayer <= stopDistance)
        {
            SetAnimBool(runAnimBool, false);
            return;
        }

        SetAnimBool(runAnimBool, true);

        Vector2 directPath = (playerTransform.position - transform.position).normalized;

        if (IsHittingWallTag(directPath))
        {
            Vector2 leftPath = Quaternion.Euler(0, 0, 45) * directPath;
            Vector2 rightPath = Quaternion.Euler(0, 0, -45) * directPath;

            if (!IsHittingWallTag(leftPath))
            {
                moveDirection = leftPath;
            }
            else if (!IsHittingWallTag(rightPath))
            {
                moveDirection = rightPath;
            }
            else
            {
                moveDirection = Quaternion.Euler(0, 0, 90) * directPath;
            }
        }
        else
        {
            moveDirection = directPath;
        }

        transform.position += (Vector3)moveDirection * moveSpeed * Time.deltaTime;
    }

    private bool IsHittingWallTag(Vector2 direction)
    {
        RaycastHit2D[] hits = Physics2D.RaycastAll(transform.position, direction, wallCheckDistance);

        foreach (RaycastHit2D hit in hits)
        {
            if (hit.collider.gameObject == gameObject) continue;

            if (hit.collider.CompareTag(wallTag))
            {
                return true;
            }
        }

        return false;
    }

    // ==========================================
    // 3. CHUỖI TẤN CÔNG (ĐÃ TÍNH TỐC ĐỘ ĐÁNH)
    // ==========================================
    private IEnumerator Routine_PerformAttack()
    {
        isAttacking = true;
        lastAttackTime = Time.time;

        UpdateFacingRotation();

        // 1. Đảm bảo Tốc độ đánh không bị <= 0 gây ra lỗi chia cho 0
        float currentAttackSpeed = Mathf.Max(0.1f, attackSpeed);

        // 2. Tăng/Giảm tốc độ múa Animation theo Attack Speed
        if (animator != null)
        {
            animator.speed = currentAttackSpeed;
        }

        SetAnimTrigger(attackAnimTrigger);

        // 3. Tự động thu ngắn thời gian delay/duration của Hitbox theo Attack Speed
        float scaledDelay = damageActiveDelay / currentAttackSpeed;
        float scaledDuration = damageDuration / currentAttackSpeed;

        yield return new WaitForSeconds(scaledDelay);

        SetAllHitboxesActive(true);

        yield return new WaitForSeconds(scaledDuration);

        SetAllHitboxesActive(false);

        // 4. Trả tốc độ Animator về 1 bình thường cho các anim di chuyển
        if (animator != null)
        {
            animator.speed = 1f;
        }

        isAttacking = false;
        currentState = EnemyState.Chasing;
    }

    // ==========================================
    // 4. HELPER FUNCTIONS
    // ==========================================
    private void SetAllHitboxesActive(bool isActive)
    {
        if (attackHitboxes == null || attackHitboxes.Length == 0) return;

        for (int i = 0; i < attackHitboxes.Length; i++)
        {
            if (attackHitboxes[i] != null)
            {
                attackHitboxes[i].SetActive(isActive);
            }
        }
    }

    private void SetAnimBool(string name, bool value)
    {
        if (animator != null && !string.IsNullOrEmpty(name))
            animator.SetBool(name, value);
    }

    private void SetAnimTrigger(string name)
    {
        if (animator != null && !string.IsNullOrEmpty(name))
            animator.SetTrigger(name);
    }

    // ==========================================
    // 5. GIZMOS
    // ==========================================
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        Gizmos.color = Color.red;
        Vector2 attackCenter = Application.isPlaying ? GetAttackCenterPosition() : (Vector2)transform.position + (Vector2)(transform.right * attackCenterOffset.x + transform.up * attackCenterOffset.y);
        Gizmos.DrawWireSphere(attackCenter, attackRange);

        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, stopDistance);

        Gizmos.color = Color.blue;
        Gizmos.DrawRay(transform.position, moveDirection * wallCheckDistance);
    }
}