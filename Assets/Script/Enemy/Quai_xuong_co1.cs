using UnityEngine;
using System.Collections;

/// <summary>
/// AI Quái cơ bản: Phát hiện người chơi -> Đi tới (có khoảng cách dừng & né tường bằng Tag Wall) -> Tấn công (Bật Hitbox sát thương).
/// Đã sửa lỗi mất Animation Attack & lỗi đi đè vào người Player.
/// </summary>
public class BasicEnemyAI : MonoBehaviour
{
    public enum EnemyState { Idle, Chasing, Attacking }

    [Header("=== STATE ===")]
    [SerializeField] private EnemyState currentState = EnemyState.Idle;

    [Header("=== DETECTION & ATTACK RANGES ===")]
    [Tooltip("Vùng 1: Bán kính nhìn thấy người chơi để bắt đầu rượt")]
    [SerializeField] private float detectionRange = 8f;

    [Tooltip("Vùng 2: Bán kính đủ gần để tung chiêu đánh")]
    [SerializeField] private float attackRange = 1.5f;

    [Tooltip("BỔ SUNG: Khoảng cách giữ an toàn, quái dừng lại không chui vào người Player (Nên đặt nhỏ hơn hoặc bằng attackRange)")]
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
    [Tooltip("GameObject chứa Collider gây sát thương (Mặc định sẽ bị tắt)")]
    [SerializeField] private GameObject attackHitbox;

    [Tooltip("Thời gian chờ trước khi bật Hitbox sát thương (Ví dụ: vung tay)")]
    [SerializeField] private float damageActiveDelay = 0.3f;

    [Tooltip("Thời gian Hitbox sát thương tồn tại (Ví dụ: thời gian ra đòn)")]
    [SerializeField] private float damageDuration = 0.4f;

    [Tooltip("Thời gian hồi chiêu giữa 2 lần đánh")]
    [SerializeField] private float attackCooldown = 2f;

    [Header("=== ANIMATION PARAMETERS ===")]
    [SerializeField] private string runAnimBool = "IsRunning";
    [SerializeField] private string attackAnimTrigger = "Attack";

    // Biến riêng tư
    private Transform playerTransform;
    private Animator animator;

    private float lastAttackTime = -999f;
    private bool isAttacking = false;
    private Vector2 moveDirection;

    private void Start()
    {
        animator = GetComponent<Animator>();

        if (attackHitbox != null)
        {
            attackHitbox.SetActive(false);
        }
    }

    private void Update()
    {
        // KHẮC PHỤC LỖI ANIMATION: Nếu đang tấn công thì ngưng không can thiệp Rotation hay di chuyển
        if (isAttacking) return;

        // Cập nhật hướng quay mặt khi không tấn công
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
    // CƠ CHẾ QUAY MẶT BẰNG ROTATION (Y-AXIS)
    // ==========================================
    private void UpdateFacingRotation()
    {
        if (playerTransform == null) return;

        // Player ở bên Trái -> Xoay Y = 180
        if (playerTransform.position.x < transform.position.x)
        {
            transform.eulerAngles = new Vector3(0f, 180f, 0f);
        }
        // Player ở bên Phải -> Xoay Y = 0
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
            float distanceToPlayer = Vector2.Distance(transform.position, playerTransform.position);

            if (distanceToPlayer <= attackRange && Time.time >= lastAttackTime + attackCooldown)
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
    // 2. DI CHUYỂN & KHOẢNG CÁCH DỪNG (STOP DISTANCE)
    // ==========================================
    private void MoveTowardsPlayerWithPathfinding()
    {
        if (playerTransform == null) return;

        float distanceToPlayer = Vector2.Distance(transform.position, playerTransform.position);

        // KHẮC PHỤC LỖI XÔNG VÀO NGƯỜI: Nếu đã đi vào khoảng cách dừng thì đứng lại và tắt Anim Chạy
        if (distanceToPlayer <= stopDistance)
        {
            SetAnimBool(runAnimBool, false);
            return;
        }

        // Nếu còn xa hơn stopDistance thì bật Anim Chạy và tiến tới
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
    // 3. CHUỖI TẤN CÔNG (ĐÃ FIX ANIMATION)
    // ==========================================
    private IEnumerator Routine_PerformAttack()
    {
        isAttacking = true;
        lastAttackTime = Time.time;

        // Quay mặt về phía Player một lần cuối chuẩn xác trước khi tung animation đánh
        UpdateFacingRotation();

        // Kích hoạt Trigger animation đánh
        SetAnimTrigger(attackAnimTrigger);

        // Chờ vung đòn
        yield return new WaitForSeconds(damageActiveDelay);

        if (attackHitbox != null)
        {
            attackHitbox.SetActive(true);
        }

        yield return new WaitForSeconds(damageDuration);

        if (attackHitbox != null)
        {
            attackHitbox.SetActive(false);
        }

        isAttacking = false;
        currentState = EnemyState.Chasing;
    }

    // ==========================================
    // 4. HELPER FUNCTIONS
    // ==========================================
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
        // Vùng nhìn (Vàng)
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        // Vùng đánh (Đỏ)
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);

        // Khoảng cách dừng (Xanh lá - Bổ sung mới)
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, stopDistance);

        // Tia kiểm tra tường (Xanh dương)
        Gizmos.color = Color.blue;
        Gizmos.DrawRay(transform.position, moveDirection * wallCheckDistance);
    }
}