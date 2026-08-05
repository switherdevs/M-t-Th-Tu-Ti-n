using UnityEngine;

public class SpiderEnemy : MonoBehaviour
{
    [Header("Đạn độc")]
    [SerializeField] private GameObject enemyBullet;
    [SerializeField] private Transform firePoint;
    [SerializeField] private float shootCooldown = 2f;

    [Header("Phát hiện Player")]
    [SerializeField] private float detectRange = 5f;
    [SerializeField] private LayerMask playerLayer;

    [Header("Di chuyển")]
    [SerializeField] private float moveSpeed = 2f;

    [Header("Tấn công")]
    [SerializeField] private float attackRange = 2f;

    private Transform player;
    private Animator animator;
    private float shootTimer;

    private void Start()
    {
        // Lấy component Animator nằm ở các đối tượng con hoặc chính đối tượng này
        animator = GetComponentInChildren<Animator>();

        // Đặt shootTimer ban đầu bằng 0 để vào tầm là có thể tấn công/bắn ngay lập tức
        shootTimer = 0f;
    }

    private void Update()
    {
        // Cập nhật đếm ngược thời gian hồi chiêu liên tục theo thời gian thực
        if (shootTimer > 0)
        {
            shootTimer -= Time.deltaTime;
        }

        // 1. Quét vùng phát hiện Player bằng OverlapCircle
        Collider2D playerCollider = Physics2D.OverlapCircle(
            transform.position,
            detectRange,
            playerLayer
        );

        if (playerCollider != null)
        {
            // Lưu lại vị trí của Player
            player = playerCollider.transform;

            // Gọi hàm lật mặt để nhện luôn hướng về Player
            FlipTowardsPlayer();

            // 2. Kiểm tra xem Player có nằm trong tầm tấn công không
            Collider2D attackCollider = Physics2D.OverlapCircle(
                transform.position,
                attackRange,
                playerLayer
            );

            if (attackCollider != null)
            {
                // TRẠNG THÁI: TẤN CÔNG & ĐỨNG YÊN (IDLE LAYER DƯỚI)
                // Tắt animation di chuyển để con nhện chuyển về Idle
                animator.SetBool("isWalking", false);

                // Kiểm tra nếu đã hết thời gian hồi chiêu
                if (shootTimer <= 0)
                {
                    // 1. Kích hoạt Trigger "Attack" (Dành cho Animator Layer tấn công riêng)
                    if (animator != null)
                    {
                        animator.SetTrigger("Attack");
                    }

                    // 2. Bắn đạn
                    Shoot();

                    // 3. Reset lại thời gian hồi chiêu
                    shootTimer = shootCooldown;
                }
            }
            else
            {
                // TRẠNG THÁI: DI CHUYỂN
                // Bật animation di chuyển khi đang rượt đuổi Player
                animator.SetBool("isWalking", true);

                // Di chuyển nhện tiến về phía Player
                transform.position = Vector2.MoveTowards(
                    transform.position,
                    player.position,
                    moveSpeed * Time.deltaTime
                );
            }
        }
        else
        {
            // TRẠNG THÁI: ĐỨNG YÊN (IDLE)
            // Không thấy Player -> Tắt animation di chuyển
            animator.SetBool("isWalking", false);
        }
    }

    // Hàm xử lý việc lật mặt (xoay hướng) trái/phải theo vị trí của Player
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

    private void Shoot()
    {
        if (enemyBullet == null || firePoint == null) return;

        Debug.Log("Spider bắn độc");

        // Tạo ra viên đạn từ Prefab tại vị trí firePoint
        GameObject bullet = Instantiate(
            enemyBullet,
            firePoint.position,
            Quaternion.identity
        );

        // Lấy component script điều hướng của viên đạn
        EnemyBullet bulletScript = bullet.GetComponent<EnemyBullet>();

        if (bulletScript != null)
        {
            // Tính toán hướng bay chuẩn hóa từ vị trí bắn đến Player
            Vector2 direction = (player.position - firePoint.position).normalized;
            bulletScript.SetDirection(direction);
        }
    }

    private void OnDrawGizmosSelected()
    {
        // Vẽ vòng tròn màu vàng hiển thị tầm phát hiện
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectRange);

        // Vẽ vòng tròn màu đỏ hiển thị tầm tấn công
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}