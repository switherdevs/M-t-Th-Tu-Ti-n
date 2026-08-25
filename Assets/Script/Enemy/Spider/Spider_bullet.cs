using UnityEngine;

public class EnemyBullet : MonoBehaviour
{
    [Header("=== BULLET SETTINGS ===")]
    [SerializeField] private float speed = 5f;
    [SerializeField] private int damage = 10;

    [Tooltip("Danh sách các Tag mà đạn sẽ va chạm và tự huỷ")]
    [SerializeField] private string[] destroyTags = new string[] { "Player", "Wall", "Ground" };

    [Header("=== POISON SETTINGS ===")]
    [SerializeField] private bool isPoisonous = false;
    [SerializeField] private float poisonDuration = 5f;
    [SerializeField] private float poisonDamagePerSecond = 2f;

    private Vector2 direction;

    /// <summary>
    /// Nhận hướng bay từ quái, tính toán góc Atan2 để xoay trục Z chĩa thẳng đầu mũi tên về Player
    /// </summary>
    public void SetDirection(Vector2 dir)
    {
        direction = dir.normalized;

        if (direction != Vector2.zero)
        {
            // Thuật toán tính góc xoay Z dựa trên Vector hướng bay
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

            // Xoay Mũi tên theo trục Z
            // LƯU Ý: Nếu Sprite gốc của Mũi tên chĩa sang Phải (Right) -> dùng Quaternion.Euler(0, 0, angle)
            // Nếu Sprite gốc chĩa lên Trên (Up) -> dùng Quaternion.Euler(0, 0, angle - 90f)
            transform.rotation = Quaternion.Euler(0f, 0f, angle);
        }
    }

    private void Update()
    {
        // Di chuyển đạn về phía trước theo góc Z đã xoay (transform.right là hướng đầu mũi tên)
        transform.Translate(Vector3.right * speed * Time.deltaTime, Space.Self);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!IsMatchingTag(other.gameObject.tag))
        {
            return;
        }

        CharacterStats targetStats = other.GetComponent<CharacterStats>();

        if (targetStats != null)
        {
            targetStats.TakeDamage(damage);

            if (isPoisonous && targetStats.IsPlayer)
            {
                targetStats.ApplyPoison(poisonDuration, poisonDamagePerSecond);
            }
        }

        Destroy(gameObject);
    }

    private bool IsMatchingTag(string otherTag)
    {
        if (destroyTags == null || destroyTags.Length == 0) return false;

        for (int i = 0; i < destroyTags.Length; i++)
        {
            if (destroyTags[i] == otherTag)
            {
                return true;
            }
        }

        return false;
    }
}