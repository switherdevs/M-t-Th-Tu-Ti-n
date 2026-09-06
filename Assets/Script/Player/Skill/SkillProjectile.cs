using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class SkillProjectile : MonoBehaviour
{
    [Header("=== ÂM THANH ĐẠN KỸ NĂNG ===")]
    [Tooltip("Âm thanh phát ra 1 lần khi đạn vừa bắn ra")]
    [SerializeField] private AudioClip launchSound;

    [Tooltip("Âm thanh khi va chạm vào quái/tường")]
    [SerializeField] private AudioClip hitSound;

    [SerializeField][Range(0f, 1f)] private float soundVolume = 0.8f;

    [Header("=== HIỆU ỨNG KHI VA CHẠM (IMPACT) ===")]
    [Tooltip("Hiệu ứng nổ/tóe lửa khi trúng tường hoặc kẻ địch")]
    [SerializeField] private GameObject hitEffectPrefab;

    private Vector2 moveDirection;
    private float moveSpeed;
    private bool hasHit = false;

    /// <summary>
    /// Khởi tạo thông số cho đạn từ SkillData truyền vào
    /// </summary>
    public void Setup(Vector2 direction, float speed, float lifeTime)
    {
        moveDirection = direction.normalized;
        moveSpeed = speed;

        // Xoay mặt Prefab theo đúng hướng di chuyển
        float angle = Mathf.Atan2(moveDirection.y, moveDirection.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle);

        // Phát âm thanh phát ra khi vừa sinh đạn (Chỉ phát 1 lần)
        if (launchSound != null)
        {
            AudioSource.PlayClipAtPoint(launchSound, transform.position, soundVolume);
        }

        // Hẹn giờ tự hủy
        Destroy(gameObject, lifeTime);
    }

    private void Update()
    {
        // Di chuyển đạn tịnh tiến mượt mà
        transform.position += (Vector3)(moveDirection * moveSpeed * Time.deltaTime);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (hasHit) return;

        // Bỏ qua va chạm với Player, chính nó, hoặc các Collider dạng Trigger khác
        if (collision.CompareTag("Player") || collision.gameObject == gameObject || collision.isTrigger) return;

        // Khi đụng Quái hoặc Tường
        if (collision.CompareTag("Enemy") || collision.CompareTag("Wall"))
        {
            hasHit = true;

            // Sinh hiệu ứng va chạm (Hit Effect)
            if (hitEffectPrefab != null)
            {
                Instantiate(hitEffectPrefab, transform.position, transform.rotation);
            }

            // Phát âm thanh va chạm tại vị trí 1 lần duy nhất
            if (hitSound != null)
            {
                AudioSource.PlayClipAtPoint(hitSound, transform.position, soundVolume);
            }

            // Hủy đạn ngay lập tức
            Destroy(gameObject);
        }
    }
}