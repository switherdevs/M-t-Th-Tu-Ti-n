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
        // Lấy component Animator nằm ở các đối tượng con hoặc chính đối tượng này để điều khiển hoạt ảnh
        animator = GetComponentInChildren<Animator>();
    }

    private void Update()
    {
        // 1. Thuật toán quét vùng phát hiện Player bằng hình tròn (OverlapCircle)
        Collider2D playerCollider = Physics2D.OverlapCircle(
            transform.position,
            detectRange,
            playerLayer
        );

        if (playerCollider != null)
        {
            // Lưu lại vị trí của Player khi nằm trong vùng ảnh hưởng
            player = playerCollider.transform;

            // Gọi hàm lật mặt để con nhện luôn hướng mặt về phía Player (trái hoặc phải)
            FlipTowardsPlayer();

            // 2. Kiểm tra tiếp xem Player có đang nằm trong tầm tấn công hay không
            Collider2D attackCollider = Physics2D.OverlapCircle(
                transform.position,
                attackRange,
                playerLayer
            );

            if (attackCollider != null)
            {
                // TRẠNG THÁI: TẤN CÔNG & ĐỨNG YÊN
                // Tắt animation di chuyển để con nhện dừng hẳn lại
                animator.SetBool("isWalking", false);

                // Kích hoạt Trigger "Attack" để thực hiện hoạt ảnh tấn công
                animator.SetTrigger("Attack");

                // Đếm ngược thời gian hồi chiêu dựa trên thời gian thực (Time.deltaTime)
                shootTimer -= Time.deltaTime;

                if (shootTimer <= 0)
                {
                    Shoot();
                    shootTimer = shootCooldown; // Reset lại thời gian hồi chiêu
                }
            }
            else
            {
                // TRẠNG THÁI: DI CHUYỂN
                // Bật animation di chuyển khi đang rượt đuổi Player
                animator.SetBool("isWalking", true);

                // Dùng Vector2.MoveTowards để di chuyển con nhện tiến về phía Player theo khung thời gian thực
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
            // Không thấy Player -> Tắt animation di chuyển, đứng yên chờ đợi
            animator.SetBool("isWalking", false);
        }
    }

    // Hàm xử lý việc lật mặt (xoay hướng) trái/phải theo vị trí của Player
    private void FlipTowardsPlayer()
    {
        // Kiểm tra vị trí X của Player so với vị trí X của con nhện
        if (player.position.x > transform.position.x)
        {
            // Nếu Player ở bên phải, lật scale theo trục X thành dương (hướng nhìn sang phải)
            Vector3 scale = transform.localScale;
            scale.x = Mathf.Abs(scale.x);
            transform.localScale = scale;
        }
        else if (player.position.x < transform.position.x)
        {
            // Nếu Player ở bên trái, lật scale theo trục X thành âm (hướng nhìn sang trái)
            Vector3 scale = transform.localScale;
            scale.x = -Mathf.Abs(scale.x);
            transform.localScale = scale;
        }
    }

    private void Shoot()
    {
        Debug.Log("Spider bắn độc");

        // Tạo ra viên đạn từ Prefab tại vị trí firePoint với góc quay mặc định
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
        // Vẽ vòng tròn màu vàng hiển thị tầm phát hiện trong cửa sổ Scene
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectRange);

        // Vẽ vòng tròn màu đỏ hiển thị tầm tấn công trong cửa sổ Scene
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}