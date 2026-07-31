using UnityEngine;

public class CuuThienTramDao : MonoBehaviour
{
    private Transform targetEnemy;
    private Vector2 targetPosition;
    private float damage;
    private float stunDuration;
    private float aoeRadius;

    private bool isFalling = false;
    private float fallSpeed = 35f; // Tăng tốc độ rơi cho dứt khoát

    public void Setup(Transform target, float dmg, float stunTime, float radius)
    {
        targetEnemy = target;
        damage = dmg;
        stunDuration = stunTime;
        aoeRadius = radius;

        if (targetEnemy != null)
        {
            targetPosition = targetEnemy.position;

            // Đặt vị trí xuất hiện ban đầu tít trên trời (Cách vị trí quái 12 đơn vị theo trục Y)
            Vector3 spawnPosition = new Vector3(targetPosition.x, targetPosition.y + 12f, 0f);
            transform.position = spawnPosition;

            // Xoay đầu kiếm hướng xuống dưới
            transform.rotation = Quaternion.Euler(0, 0, -90f);

            isFalling = true;
            Debug.Log("<color=cyan>[CỬU THIÊN TRẢM]</color> Đã spawn kiếm trên trời, bắt đầu rơi xuống!");
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Update()
    {
        if (!isFalling) return;

        // Nếu quái di chuyển, cập nhật lại điểm đến (tùy chọn, giúp kiếm đuổi theo quái chính xác hơn)
        if (targetEnemy != null)
        {
            targetPosition = targetEnemy.position;
        }

        // Cho kiếm lao thẳng xuống vị trí mục tiêu
        transform.position = Vector2.MoveTowards(transform.position, targetPosition, fallSpeed * Time.deltaTime);

        // Khi kiếm chạm đến vị trí đích (hoặc rất gần)
        if (Vector2.Distance(transform.position, targetPosition) <= 0.2f)
        {
            ImpactExplosion();
        }
    }

    private void ImpactExplosion()
    {
        isFalling = false;

        Debug.Log("<color=orange>[CỬU THIÊN TRẢM ĐAO]</color> Cự kiếm đã chạm đất, gây nổ AoE!");

        // Quét toàn bộ kẻ địch trong vùng bán kính AoE quanh điểm rơi
        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(transform.position, aoeRadius);

        foreach (var hit in hitEnemies)
        {
            if (hit.CompareTag("Enemy"))
            {
                var victim = hit.GetComponentInParent<StatsSystem.Interfaces.IDamageable>();
                if (victim != null)
                {
                    victim.TakeDamage(damage);
                }
            }
        }

        // Hủy cự kiếm ngay lập tức sau 0.1 giây để tránh bị kẹt không biến mất
        Destroy(gameObject, 0.1f);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, aoeRadius);
    }
}