using UnityEngine;

public class CuuThienTramDao : MonoBehaviour
{
    private Transform targetEnemy;
    private Vector2 targetPosition;
    private float damage;
    private float stunDuration;
    private float aoeRadius;

    private bool isFalling = false;
    private float fallSpeed = 35f;

    [Header("Cấu Hình Rung Camera Khi Chạm Đất")]
    [SerializeField] private float shakeIntensity = 6f;
    [SerializeField] private float shakeTime = 0.35f;

    [Header("Âm thanh & Hiệu ứng khi Trúng/Chạm đất")]
    [SerializeField] private AudioClip impactSound;          // Âm thanh nổ cự kiếm
    [SerializeField] private GameObject impactEffectPrefab;  // Prefab hiệu ứng nổ AoE
    [SerializeField][Range(0f, 1f)] private float soundVolume = 1f;

    private bool hasImpacted = false; // Đảm bảo hiệu ứng chạm đất nổ AoE chỉ phát 1 lần

    /// <summary>
    /// Khởi tạo vị trí và kích hoạt rơi cự kiếm từ trên trời xuống điểm chỉ định.
    /// </summary>
    public void Setup(Transform target, Vector2 fallbackPos, float dmg, float stunTime, float radius)
    {
        targetEnemy = target;
        damage = dmg;
        stunDuration = stunTime;
        aoeRadius = radius;

        // Xác định vị trí đích
        targetPosition = targetEnemy != null ? (Vector2)targetEnemy.position : fallbackPos;

        // Đặt vị trí xuất phát trên cao (Y + 12f)
        Vector3 spawnPosition = new Vector3(targetPosition.x, targetPosition.y + 12f, 0f);
        transform.position = spawnPosition;

        // Xoay lưỡi kiếm hướng xuống đất
        transform.rotation = Quaternion.Euler(0, 0, -90f);

        isFalling = true;
        Debug.Log("<color=cyan>[CỬU THIÊN TRẢM]</color> Đã spawn kiếm trên trời, bắt đầu rơi xuống!");
    }

    void Update()
    {
        if (!isFalling) return;

        // Nếu kẻ địch vẫn còn sống, liên tục bám theo vị trí của nó
        if (targetEnemy != null)
        {
            targetPosition = targetEnemy.position;
        }

        // Lao thẳng xuống vị trí mục tiêu
        transform.position = Vector2.MoveTowards(transform.position, targetPosition, fallSpeed * Time.deltaTime);

        // Khi tiến gần sát mặt đất/mục tiêu thì kích hoạt nổ AoE
        if (Vector2.Distance(transform.position, targetPosition) <= 0.3f)
        {
            ImpactExplosion();
        }
    }

    private void ImpactExplosion()
    {
        if (hasImpacted) return;
        hasImpacted = true; // Khóa không cho hàm nổ này gọi lại thêm lần nào nữa

        isFalling = false;
        Debug.Log("<color=orange>[CỬU THIÊN TRẢM ĐAO]</color> Cự kiếm đã chạm đất, gây nổ AoE!");

        // 1. Rung camera khi cự kiếm cắm đất
        if (CameraShake.Instance != null)
        {
            CameraShake.Instance.Shake(shakeIntensity, shakeTime);
        }

        // 2. Quét gây sát thương diện rộng tại vị trí chạm đất
        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(transform.position, aoeRadius);
        foreach (var hit in hitEnemies)
        {
            if (hit.CompareTag("Enemy"))
            {
                var victim = hit.GetComponentInParent<IDamageable>();
                if (victim != null)
                {
                    victim.TakeDamage(damage);
                }
            }
        }

        // 3. Xử lý âm thanh nổ và hiệu ứng vụ nổ diện rộng
        if (impactEffectPrefab != null)
        {
            Instantiate(impactEffectPrefab, transform.position, Quaternion.identity);
        }

        if (impactSound != null)
        {
            AudioSource.PlayClipAtPoint(impactSound, transform.position, soundVolume);
        }

        // 4. Hủy cự kiếm ngay lập tức sau khi hoàn thành hiệu ứng
        Destroy(gameObject);
    }

    private void OnDrawGizmosSelected()
    {
        // Vẽ vòng tròn tầm nổ AoE của cự kiếm
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, aoeRadius);
    }
}