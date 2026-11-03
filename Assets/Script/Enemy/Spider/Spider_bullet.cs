using UnityEngine;

public class EnemyBullet : MonoBehaviour
{
    [Header("=== CẤU HÌNH ĐẠN CƠ BẢN ===")]
    [SerializeField] private float speed = 5f;
    [SerializeField] private int damage = 10;
    [Tooltip("Thời gian tự hủy tối đa nếu đạn bay ra ngoài map không trúng gì")]
    [SerializeField] private float lifeTime = 3f;

    [Header("=== HIỆU ỨNG & ÂM THANH KHI HỦY / TRÚNG ĐÍCH ===")]
    [SerializeField] private GameObject hitVFX;          // Prefab hiệu ứng nổ/trúng đích
    [SerializeField] private float vfxDestroyTime = 1f;  // Thời gian tự hủy của hiệu ứng VFX
    [SerializeField] private AudioClip sfxHit;           // Âm thanh phát ra khi hủy
    [SerializeField, Range(0f, 1f)] private float sfxVolume = 1f; // Âm lượng âm thanh

    [Header("=== HOMING (ĐẠN ĐUỔI) SETTINGS ===")]
    [Tooltip("Bật tick này để viên đạn đổi hướng rượt đuổi theo người chơi")]
    [SerializeField] private bool isFollow = false;
    [SerializeField] private float rotationSpeed = 200f;

    [Header("=== VA CHẠM & MIỄN TRỪ ===")]
    [SerializeField] private string[] destroyTags = new string[] { "Player", "Wall", "Ground" };
    [Tooltip("Thời gian miễn dịch va chạm tường khi vừa xuất hiện để tránh kẹt")]
    [SerializeField] private float ignoreWallTime = 0.2f;

    [Header("=== POISON SETTINGS ===")]
    [SerializeField] private bool isPoisonous = false;
    [SerializeField] private float poisonDuration = 5f;
    [SerializeField] private float poisonDamagePerSecond = 2f;

    private Vector2 direction;
    private Transform playerTransform;
    private float spawnTimer;
    private bool hasDestroyed = false; // Cờ chống gọi hủy nhiều lần

    private void Start()
    {
        spawnTimer = 0f;
        hasDestroyed = false;

        // Tìm kiếm Player trong scene tự động
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            playerTransform = playerObj.transform;
        }

        // Đếm ngược thời gian tự hủy (Thay thế cho AutoDestroy)
        Invoke(nameof(DestroyBullet), lifeTime);
    }

    private void Update()
    {
        // 1. Đếm thời gian miễn trừ va chạm ban đầu
        if (spawnTimer < ignoreWallTime)
        {
            spawnTimer += Time.deltaTime;
        }

        // 2. Xử lý logic đạn đuổi (Homing) nếu được bật tick isFollow
        if (isFollow && playerTransform != null)
        {
            Vector2 targetDir = ((Vector2)playerTransform.position - (Vector2)transform.position).normalized;
            if (targetDir != Vector2.zero)
            {
                float targetAngle = Mathf.Atan2(targetDir.y, targetDir.x) * Mathf.Rad2Deg;
                Quaternion targetRotation = Quaternion.Euler(0f, 0f, targetAngle);
                transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
            }
        }

        // 3. Di chuyển đạn về phía trước theo hướng mũi tên
        transform.Translate(Vector3.right * speed * Time.deltaTime, Space.Self);
    }

    /// <summary>
    /// Thiết lập hướng bay ban đầu cho đạn và xoay góc Z chĩa thẳng đầu đạn
    /// </summary>
    public void SetDirection(Vector2 dir)
    {
        direction = dir.normalized;
        if (direction != Vector2.zero)
        {
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0f, 0f, angle);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (hasDestroyed) return;
        if (spawnTimer < ignoreWallTime) return;

        // Kiểm tra xem va chạm có khớp với danh sách tag cần hủy không
        if (!IsMatchingTag(other.gameObject.tag)) return;

        // Gây sát thương nếu đối tượng có component CharacterStats
        CharacterStats targetStats = other.GetComponent<CharacterStats>();
        if (targetStats != null)
        {
            targetStats.TakeDamage(damage);

            if (isPoisonous && targetStats.IsPlayer)
            {
                targetStats.ApplyPoison(poisonDuration, poisonDamagePerSecond);
            }
        }

        // Tiến hành hủy đạn và gọi hiệu ứng
        DestroyBullet();
    }

    private bool IsMatchingTag(string otherTag)
    {
        if (destroyTags == null || destroyTags.Length == 0) return false;
        for (int i = 0; i < destroyTags.Length; i++)
        {
            if (destroyTags[i] == otherTag) return true;
        }
        return false;
    }

    /// <summary>
    /// Hàm trung tâm chịu trách nhiệm sinh hiệu ứng VFX, phát âm thanh SFX và xóa đạn
    /// </summary>
    public void DestroyBullet()
    {
        if (hasDestroyed) return;
        hasDestroyed = true;

        // Hủy lệnh đếm ngược cũ nếu có
        CancelInvoke(nameof(DestroyBullet));

        // 1. Sinh hiệu ứng VFX tại vị trí hiện tại của đạn
        if (hitVFX != null)
        {
            GameObject vfxInstance = Instantiate(hitVFX, transform.position, Quaternion.identity);
            Destroy(vfxInstance, vfxDestroyTime);
        }

        // 2. Phát âm thanh va chạm 1 lần độc lập
        if (sfxHit != null)
        {
            AudioSource.PlayClipAtPoint(sfxHit, transform.position, sfxVolume);
        }

        // 3. Xóa viên đạn khỏi game
        Destroy(gameObject);
    }
}