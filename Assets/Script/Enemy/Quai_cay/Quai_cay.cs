using UnityEngine;

public class Boss_DocLienYeu : MonoBehaviour
{
    [Header("Tham chiếu")]
    public Transform firePoint;
    public GameObject bulletPrefab;

    [Header("Cài đặt")]
    public float detectRange = 8f;
    public float attackCooldown = 2f;

    [Header("Tự động quét Player")]
    [Tooltip("Kéo LayerMask chứa Layer của Player vào đây (ví dụ: Layer 'Player')")]
    public LayerMask playerLayer;

    [Header("Animation Settings")]
    [Tooltip("Tên Trigger Animation tấn công trong Animator")]
    public string attackTriggerName = "Attack";

    private Transform playerTransform; // Tự động cập nhật khi phát hiện Player
    private float attackTimer;
    private Animator animator;

    void Start()
    {
        // Tự động tìm Animator ở các GameObject con
        animator = GetComponentInChildren<Animator>();

        attackTimer = attackCooldown;
    }

    void Update()
    {
        // 1. TỰ ĐỘNG QUÉT TÌM PLAYER TRONG VÙNG DETECT RANGE BẰNG LAYER
        FindPlayerInDetectionRange();

        // Nếu không phát hiện thấy Player trong tầm hoạt động thì bỏ qua
        if (playerTransform == null)
            return;

        // 2. LẬT HƯỚNG TRÁI / PHẢI THEO VỊ TRÍ PLAYER
        FlipTowardsPlayer();

        // 3. ĐẾM THỜI GIAN BẮN
        attackTimer -= Time.deltaTime;

        if (attackTimer <= 0f)
        {
            Shoot();
            attackTimer = attackCooldown;
        }
    }

    /// <summary>
    /// Hàm tự động quét tìm vị trí Player trong bán kính detectRange dựa trên LayerMask
    /// </summary>
    private void FindPlayerInDetectionRange()
    {
        // Quét hình tròn bán kính detectRange xem có Collider nào thuộc playerLayer không
        Collider2D playerCollider = Physics2D.OverlapCircle(transform.position, detectRange, playerLayer);

        if (playerCollider != null)
        {
            playerTransform = playerCollider.transform;
        }
        else
        {
            playerTransform = null; // Mất dấu Player khi ra khỏi tầm quét
        }
    }

    // Hàm lật hướng Trái / Phải bằng Rotation
    private void FlipTowardsPlayer()
    {
        if (playerTransform == null) return;

        // Nếu Player ở bên phải con quái -> Xoay góc 0 độ (quay sang phải)
        if (playerTransform.position.x > transform.position.x)
        {
            transform.rotation = Quaternion.Euler(0f, 0f, 0f);
        }
        // Nếu Player ở bên trái con quái -> Xoay Y 180 độ (quay sang trái)
        else if (playerTransform.position.x < transform.position.x)
        {
            transform.rotation = Quaternion.Euler(0f, 180f, 0f);
        }
    }

    void Shoot()
    {
        if (playerTransform == null) return;

        // 1. KÍCH HOẠT ANIMATION TẤN CÔNG (TRIGGER)
        if (animator != null)
        {
            animator.SetTrigger(attackTriggerName);
        }

        // 2. BẮN ĐẠN
        if (bulletPrefab == null || firePoint == null)
            return;

        GameObject bullet = Instantiate(bulletPrefab, firePoint.position, Quaternion.identity);

        Vector2 dir = (playerTransform.position - firePoint.position).normalized;

        Bullet bulletScript = bullet.GetComponent<Bullet>();

        if (bulletScript != null)
        {
            bulletScript.SetDirection(dir);
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(transform.position, detectRange);
    }
}