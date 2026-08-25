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

    private void Start()
    {
        animator = GetComponentInChildren<Animator>();
        enemyCollider = GetComponent<Collider2D>();
        shootTimer = 0f;

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

            Collider2D attackCollider = Physics2D.OverlapCircle(
                transform.position,
                attackRange,
                playerLayer
            );

            if (attackCollider != null)
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
            else
            {
                animator.SetBool("isWalking", true);

                transform.position = Vector2.MoveTowards(
                    transform.position,
                    player.position,
                    moveSpeed * Time.deltaTime
                );
            }
        }
        else
        {
            animator.SetBool("isWalking", false);
        }
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
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectRange);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}