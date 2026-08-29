using UnityEngine;
using System.Collections;

public class SpiderEnemy : MonoBehaviour
{
    [Header("Đạn độc")]
    [SerializeField] private GameObject enemyBullet;
    [SerializeField] private Transform firePoint;
    [SerializeField] private float shootCooldown = 2f;

    [Header("Cấu hình Burst (Bắn chùm)")]
    [Tooltip("Tick chọn để bật chế độ bắn chùm nhiều viên liên tiếp")]
    [SerializeField] private bool isBurst = false;

    [Tooltip("Số lượng đạn bắn ra trong một loạt Burst")]
    [SerializeField] private int burstBulletCount = 3;

    [Tooltip("Khoảng thời gian giãn cách giữa các viên đạn trong 1 loạt Burst (giây)")]
    [SerializeField] private float burstDelay = 0.1f;

    [Header("Tên Tham Số Animation (Animator)")]
    [Tooltip("Tên biến Bool kích hoạt trạng thái ngắm bắn (Animator)")]
    [SerializeField] private string aimingAnimBool = "isAiming";

    [Tooltip("Tên biến Trigger kích hoạt Animation bắn 1 lần (Animator)")]
    [SerializeField] private string shootAnimTrigger = "Shoot";

    [Header("Phát hiện Player")]
    [SerializeField] private float detectRange = 5f;
    [SerializeField] private LayerMask playerLayer;

    [Header("Kiểm tra Vật Cản Đường Bắn")]
    [Tooltip("Layer của tường, địa hình hoặc chướng ngại vật cản đạn")]
    [SerializeField] private LayerMask obstacleLayer;

    [Header("Né Đồng Đội (Separation)")]
    [Tooltip("Bán kính quét để phát hiện và né quái đồng đội")]
    [SerializeField] private float avoidDistance = 1.2f;

    [Tooltip("Độ mạnh của lực đẩy né đồng đội")]
    [SerializeField] private float avoidWeight = 1.5f;

    [Tooltip("Tag của quái đồng đội cần né")]
    [SerializeField] private string allyTag = "Enemy";

    [Header("Di chuyển")]
    [SerializeField] private float moveSpeed = 2f;

    [Header("Tấn công")]
    [SerializeField] private float attackRange = 2f;

    [Header("Animation Chết")]
    [SerializeField] private string dieAnimTrigger = "Die";

    private Transform player;
    private Animator animator;
    private CharacterStats characterStats;
    private Collider2D enemyCollider;

    private float shootTimer;
    private bool isDead = false;
    private bool isShootingBurst = false; // Cờ kiểm tra xem nhện có đang trong chu kỳ xả đạn không
    private int strafeDirection = 1;      // Hướng dạt ngang khi bị cản: 1 (Phải/Trên), -1 (Trái/Dưới)

    private void Start()
    {
        animator = GetComponentInChildren<Animator>();
        enemyCollider = GetComponent<Collider2D>();
        shootTimer = 0f;

        // Quyết định hướng dạt ngẫu nhiên ban đầu
        strafeDirection = Random.value > 0.5f ? 1 : -1;

        // Lắng nghe sự kiện chết từ CharacterStats
        characterStats = GetComponent<CharacterStats>();
        if (characterStats != null)
        {
            characterStats.OnDeath += HandleDeath;
        }
    }

    private void OnDestroy()
    {
        // Hủy đăng ký sự kiện khi GameObject bị Destroy để tránh leak bộ nhớ
        if (characterStats != null)
        {
            characterStats.OnDeath -= HandleDeath;
        }
    }

    private void Update()
    {
        // Khóa hoàn toàn Update nếu nhện đã chết
        if (isDead) return;

        if (shootTimer > 0)
        {
            shootTimer -= Time.deltaTime;
        }

        // Nếu đang trong quá trình xả đạn Burst thì dừng di chuyển và quay mặt
        if (isShootingBurst) return;

        Collider2D playerCollider = Physics2D.OverlapCircle(
            transform.position,
            detectRange,
            playerLayer
        );

        if (playerCollider != null)
        {
            player = playerCollider.transform;

            FlipTowardsPlayer();

            // 1. Tính toán lực né đồng đội
            Vector2 avoidanceForce = CalculateAllyAvoidance();

            // 2. Kiểm tra xem đường bắn tới Player có bị vật cản che khuất không
            bool isLineOfSightBlocked = CheckLineOfSightBlocked();

            float distanceToPlayer = Vector2.Distance(transform.position, player.position);

            // BỊ CẢN ĐƯỜNG BẮN: Di chuyển dạt ngang tìm vị trí ngắm mới
            if (isLineOfSightBlocked)
            {
                animator.SetBool("isWalking", true);

                // Tạo vector dạt ngang vuông góc với hướng tới Player
                Vector2 dirToPlayer = (player.position - transform.position).normalized;
                Vector2 strafeDir = new Vector2(-dirToPlayer.y, dirToPlayer.x) * strafeDirection;

                Vector2 finalMoveDir = (strafeDir + avoidanceForce * avoidWeight).normalized;
                transform.position += (Vector3)finalMoveDir * moveSpeed * Time.deltaTime;
            }
            // ĐƯỜNG BẮN THÔNG THOÁNG VÀ ĐÃ TRONG TẦM BẮN
            else if (distanceToPlayer <= attackRange)
            {
                animator.SetBool("isWalking", false);

                if (shootTimer <= 0)
                {
                    if (isBurst)
                    {
                        StartCoroutine(Routine_BurstShoot());
                    }
                    else
                    {
                        if (animator != null)
                        {
                            animator.SetTrigger("Attack");
                        }
                        ShootOneBullet();
                    }

                    shootTimer = shootCooldown;
                }
            }
            // ĐƯỜNG BẮN THÔNG THOÁNG NHƯNG CHƯA ĐẾN TẦM BẮN -> Tiến tới Player + Né đồng đội
            else
            {
                animator.SetBool("isWalking", true);

                Vector2 dirToPlayer = (player.position - transform.position).normalized;
                Vector2 finalMoveDir = (dirToPlayer + avoidanceForce * avoidWeight).normalized;

                transform.position += (Vector3)finalMoveDir * moveSpeed * Time.deltaTime;
            }
        }
        else
        {
            animator.SetBool("isWalking", false);
        }
    }

    /// <summary>
    /// Kiểm tra xem có Collider thuộc obstacleLayer cản từ firePoint đến Player hay không
    /// </summary>
    private bool CheckLineOfSightBlocked()
    {
        if (player == null) return false;

        Vector3 startPos = firePoint != null ? firePoint.position : transform.position;
        Vector2 direction = player.position - startPos;
        float distance = direction.magnitude;

        // Bắn Raycast từ firePoint tới Player trên Layer vật cản
        RaycastHit2D hit = Physics2D.Raycast(startPos, direction.normalized, distance, obstacleLayer);

        return hit.collider != null; // Trả về true nếu chạm phải vật cản
    }

    /// <summary>
    /// Thuật toán Separation: Tạo lực đẩy né đồng đội khi đi quá sát nhau
    /// </summary>
    private Vector2 CalculateAllyAvoidance()
    {
        Vector2 avoidanceVector = Vector2.zero;
        Collider2D[] allies = Physics2D.OverlapCircleAll(transform.position, avoidDistance);

        int neighborCount = 0;
        foreach (Collider2D ally in allies)
        {
            // Bỏ qua bản thân và chỉ tính các Object có Tag đồng đội
            if (ally != null && ally.gameObject != gameObject && ally.CompareTag(allyTag))
            {
                Vector2 diff = (Vector2)(transform.position - ally.transform.position);
                // Khoảng cách càng gần thì lực đẩy càng mạnh
                avoidanceVector += diff.normalized / Mathf.Max(diff.magnitude, 0.1f);
                neighborCount++;
            }
        }

        if (neighborCount > 0)
        {
            avoidanceVector /= neighborCount;
        }

        return avoidanceVector;
    }

    // ==========================================
    // XỬ LÝ KHI NHỆN CHẾT
    // ==========================================
    private void HandleDeath()
    {
        isDead = true;

        // 1. Dừng ngay lập tức các Coroutine (ví dụ: đang bắn dở chùm đạn Burst)
        StopAllCoroutines();

        // 2. Dừng toàn bộ Animation di chuyển/tấn công và kích hoạt Animation Chết
        if (animator != null)
        {
            animator.SetBool("isWalking", false);
            animator.SetBool(aimingAnimBool, false);
            animator.ResetTrigger("Attack");
            animator.ResetTrigger(shootAnimTrigger);

            if (!string.IsNullOrEmpty(dieAnimTrigger))
            {
                animator.SetTrigger(dieAnimTrigger);
            }
        }

        // 3. Vô hiệu hóa Collider để Player không bị vướng/kẹt khi đi qua xác nhện
        if (enemyCollider != null)
        {
            enemyCollider.enabled = false;
        }
    }

    private void FlipTowardsPlayer()
    {
        if (player == null) return;

        if (player.position.x > transform.position.x)
        {
            Vector3 scale = transform.localScale;
            scale.x = Mathf.Abs(scale.x);
            transform.localScale = scale;
        }
        else if (player.position.x < transform.position.x)
        {
            Vector3 scale = transform.localScale;
            scale.x = -Mathf.Abs(scale.x);
            transform.localScale = scale;
        }
    }

    private void ShootOneBullet()
    {
        if (enemyBullet == null || firePoint == null || player == null || isDead) return;

        GameObject bullet = Instantiate(
            enemyBullet,
            firePoint.position,
            Quaternion.identity
        );

        EnemyBullet bulletScript = bullet.GetComponent<EnemyBullet>();

        if (bulletScript != null)
        {
            Vector2 direction = (player.position - firePoint.position).normalized;
            bulletScript.SetDirection(direction);
        }
    }

    /// <summary>
    /// Coroutine xử lý bắn chùm đạn Burst + Chuyển Animation Ngắm & Bắn
    /// </summary>
    private IEnumerator Routine_BurstShoot()
    {
        isShootingBurst = true;

        // 1. Chuyển sang Animation Ngắm
        if (animator != null && !string.IsNullOrEmpty(aimingAnimBool))
        {
            animator.SetBool(aimingAnimBool, true);
        }

        for (int i = 0; i < burstBulletCount; i++)
        {
            // Kiểm tra nếu trong lúc đang bắn dở chùm đạn mà nhện bị Player đánh chết thì dừng ngay
            if (isDead) yield break;

            // 2. Kích hoạt Animation Bắn ở Layer trên (chạy 1 lần cho mỗi viên đạn)
            if (animator != null && !string.IsNullOrEmpty(shootAnimTrigger))
            {
                animator.SetTrigger(shootAnimTrigger);
            }

            // 3. Bắn viên đạn
            ShootOneBullet();

            yield return new WaitForSeconds(burstDelay);
        }

        // 4. Hết chu kỳ bắn -> Tắt Animation Ngắm
        if (animator != null && !string.IsNullOrEmpty(aimingAnimBool))
        {
            animator.SetBool(aimingAnimBool, false);
        }

        isShootingBurst = false;
    }

    private void OnDrawGizmosSelected()
    {
        // Bán kính tầm nhìn
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectRange);

        // Bán kính tầm bắn
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);

        // Bán kính né đồng đội (Separation)
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, avoidDistance);

        // Vẽ đường kiểm tra Raycast tầm bắn nếu đang chạy Play Mode
        if (Application.isPlaying && player != null)
        {
            Vector3 startPos = firePoint != null ? firePoint.position : transform.position;
            Gizmos.color = CheckLineOfSightBlocked() ? Color.red : Color.green;
            Gizmos.DrawLine(startPos, player.position);
        }
    }
}