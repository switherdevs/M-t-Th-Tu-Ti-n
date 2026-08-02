using StatsSystem.Components;
using UnityEngine;

public class VanKiemBatPhuong : MonoBehaviour
{
    [Header("Cấu Hình Rung Camera")]
    [SerializeField] private float shakeIntensity = 3.5f; // Đỉnh lực rung (Vạn kiếm tỏa ra nên rung mạnh chút)
    [SerializeField] private float shakeDuration = 0.2f;  // Thời gian rung (giây)

    private float damage;
    private float knockbackForce;
    private Vector2 moveDirection;
    private CharacterStats characterStats;

    public void Setup(Vector2 direction, float damageValue, float knockbackValue, CharacterStats stats)
    {
        moveDirection = direction;
        damage = damageValue;
        knockbackForce = knockbackValue;
        characterStats = stats;

        // Xoay hướng của kiếm theo hướng bay tỏa tròn
        float rotZ = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, rotZ);

        // KÍCH HOẠT RUNG CAMERA KHI BẮN KIẾM
        if (CameraShake.Instance != null)
        {
            // Thuật toán Fallback: Nếu ngoài Inspector lỡ để bằng 0, tự động lấy giá trị mặc định an toàn (3.5f và 0.2f)
            float finalIntensity = shakeIntensity > 0f ? shakeIntensity : 3.5f;
            float finalDuration = shakeDuration > 0f ? shakeDuration : 0.2f;

            CameraShake.Instance.Shake(finalIntensity, finalDuration);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Kiểm tra va chạm với kẻ địch (Điều chỉnh Tag "Enemy" theo project của bạn)
        if (collision.CompareTag("Enemy"))
        {
            // 1. Xử lý gây sát thương (Tùy biến theo hệ thống máu của bạn)
            // Ví dụ gọi hàm nhận sát thương của enemy:
            // EnemyHealth enemyHealth = collision.GetComponent<EnemyHealth>();
            // if (enemyHealth != null) { enemyHealth.TakeDamage(damage, characterStats); }

            // 2. Xử lý Đẩy lùi (Knockback)
            Rigidbody2D enemyRb = collision.GetComponent<Rigidbody2D>();
            if (enemyRb != null)
            {
                // Tính hướng đẩy lùi ra xa tâm nhân vật hoặc theo hướng tia kiếm bay
                Vector2 knockbackDir = (collision.transform.position - transform.position).normalized;

                // Nếu muốn đẩy theo hướng bay của kiếm: knockbackDir = moveDirection;
                enemyRb.AddForce(knockbackDir * knockbackForce, ForceMode2D.Impulse);
            }

            // Va chạm xong có thể tự hủy kiếm tùy ý (hoặc để bay xuyên qua)
            // Destroy(gameObject);
        }
    }
}