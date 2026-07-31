using StatsSystem.Components;
using UnityEngine;

public class VanKiemBatPhuong : MonoBehaviour
{
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