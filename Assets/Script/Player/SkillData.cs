using UnityEngine;

[CreateAssetMenu(fileName = "SkillData", menuName = "Kỹ Năng/Skill Data")]
public class SkillData : ScriptableObject
{
    [Header("Thông Tin Chung")]
    public string skillName = "Tên Kỹ Năng";
    public GameObject skillPrefab;
    [Range(0f, 100f)] public float triggerChance = 20f;

    // Hàm ảo để các loại skill tự viết logic riêng khi kích hoạt
    public virtual void UseSkill(Transform firePoint, Vector2 direction)
    {
        // Mặc định (dùng cho skill bắn đạn bay như Tam Liên Trảm Kiếm)
        if (skillPrefab == null || firePoint == null) return;

        float spreadAngle = 15f;
        float[] angles = { 0f, -spreadAngle, spreadAngle };

        foreach (float angle in angles)
        {
            Quaternion rotation = Quaternion.Euler(0, 0, angle);
            Vector2 spreadDirection = rotation * direction;

            GameObject skillObj = Instantiate(skillPrefab, firePoint.position, Quaternion.identity);

            Rigidbody2D rb = skillObj.GetComponent<Rigidbody2D>();
            if (rb != null) rb.linearVelocity = spreadDirection * 12f;

            Destroy(skillObj, 3f);
        }
    }
}