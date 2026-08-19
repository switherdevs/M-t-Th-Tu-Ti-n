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

    public void Setup(Transform target, float dmg, float stunTime, float radius)
    {
        targetEnemy = target;
        damage = dmg;
        stunDuration = stunTime;
        aoeRadius = radius;

        if (targetEnemy != null)
        {
            targetPosition = targetEnemy.position;
            Vector3 spawnPosition = new Vector3(targetPosition.x, targetPosition.y + 12f, 0f);
            transform.position = spawnPosition;
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

        if (targetEnemy != null)
        {
            targetPosition = targetEnemy.position;
        }

        transform.position = Vector2.MoveTowards(transform.position, targetPosition, fallSpeed * Time.deltaTime);

        if (Vector2.Distance(transform.position, targetPosition) <= 0.2f)
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

        // Rung camera khi cự kiếm cắm đất
        if (CameraShake.Instance != null)
        {
            CameraShake.Instance.Shake(shakeIntensity, shakeTime);
        }

        // 1. Quét gây sát thương diện rộng
        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(transform.position, aoeRadius);
        foreach (var hit in hitEnemies)
        {
            if (hit.CompareTag("Enemy"))
            {
                // Đã sửa lại: Gọi thẳng IDamageable không qua namespace cũ
                var victim = hit.GetComponentInParent<IDamageable>();
                if (victim != null)
                {
                    victim.TakeDamage(damage);
                }
            }
        }

        // 2. Xử lý âm thanh nổ và hiệu ứng vụ nổ diện rộng
        if (impactEffectPrefab != null)
        {
            Instantiate(impactEffectPrefab, transform.position, Quaternion.identity);
        }

        if (impactSound != null)
        {
            AudioSource.PlayClipAtPoint(impactSound, transform.position, soundVolume);
        }

        // 3. Hủy cự kiếm
        Destroy(gameObject, 0.1f);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, aoeRadius);
    }
}