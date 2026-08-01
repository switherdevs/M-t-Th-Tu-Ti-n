using UnityEngine;

public class BaseSkillEffect : MonoBehaviour
{
    [SerializeField] private float skillSpeed = 12f;
    [SerializeField] private float lifeTime = 3f;

    [Header("Âm thanh & Hiệu ứng khi Va chạm")]
    [SerializeField] private AudioClip hitSound;          // Âm thanh trúng đích
    [SerializeField] private GameObject hitEffectPrefab;  // Hiệu ứng VFX trúng đích
    [SerializeField][Range(0f, 1f)] private float soundVolume = 1f;

    private Vector2 moveDirection;
    private bool hasHit = false; // Cờ khóa va chạm 1 lần

    public void Initialize(Vector2 direction)
    {
        moveDirection = direction;

        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(new Vector3(0, 0, angle));

        Destroy(gameObject, lifeTime);
    }

    void Update()
    {
        transform.position += (Vector3)(moveDirection * skillSpeed * Time.deltaTime);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (hasHit) return;

        if (collision.CompareTag("Enemy") || collision.CompareTag("Wall"))
        {
            hasHit = true; // Khóa va chạm ngay lập tức

            // 1. Phát hiệu ứng hình ảnh VFX
            if (hitEffectPrefab != null)
            {
                Instantiate(hitEffectPrefab, transform.position, transform.rotation);
            }

            // 2. Phát âm thanh 1 lần độc lập tại vị trí va chạm
            if (hitSound != null)
            {
                AudioSource.PlayClipAtPoint(hitSound, transform.position, soundVolume);
            }

            // 3. Hủy ngay lập tức viên đạn/skill
            Destroy(gameObject);
        }
    }
}