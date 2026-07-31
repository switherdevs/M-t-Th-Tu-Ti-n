using UnityEngine;

[CreateAssetMenu(fileName = "SkillData", menuName = "Kỹ Năng/Skill Data")]
public class SkillData : ScriptableObject
{
    [Header("Thông Tin Chung")]
    public string skillName = "Tên Kỹ Năng";
    public GameObject skillPrefab;
    [Range(0f, 100f)] public float triggerChance = 20f;

    [Header("Cấu Hình Tốc Độ Đạn")]
    [Tooltip("Tốc độ bay của kiếm/đạn")]
    [SerializeField] public float projectileSpeed = 12f;

    // Hàm ảo để các loại skill tự viết logic riêng khi kích hoạt
    public virtual void UseSkill(Transform firePoint, Vector2 direction)
    {
        // Mặc định (dùng cho skill bắn đạn bay như Tam Liên Trảm Kiếm)
        if (skillPrefab == null || firePoint == null) return;

        float spreadAngle = 15f;
        float[] angles = { 0f, -spreadAngle, spreadAngle };

        foreach (float angle in angles)
        {
            // 1. Tính hướng bay theo góc rẻ quạt
            Quaternion rotationOffset = Quaternion.Euler(0, 0, angle);
            Vector2 spreadDirection = rotationOffset * direction;

            // 2. Tính góc xoay Z tuyệt đối theo hướng spreadDirection để kiếm chĩa mũi về đúng hướng bay
            float swordAngle = Mathf.Atan2(spreadDirection.y, spreadDirection.x) * Mathf.Rad2Deg;
            Quaternion swordRotation = Quaternion.Euler(0, 0, swordAngle);

            // 3. Sinh ra Prefab kèm góc xoay swordRotation chuẩn
            GameObject skillObj = Instantiate(skillPrefab, firePoint.position, swordRotation);

            // 4. Đẩy vận tốc bay dựa trên biến projectileSpeed chỉnh được từ Inspector
            Rigidbody2D rb = skillObj.GetComponent<Rigidbody2D>();
            if (rb != null) rb.linearVelocity = spreadDirection * projectileSpeed;

            Destroy(skillObj, 3f);
        }
    }
}