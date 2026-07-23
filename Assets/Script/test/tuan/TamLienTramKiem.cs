using StatsSystem.Components;
using UnityEngine;

public class TamLienTramKiem : MonoBehaviour
{
    // Hàm này sẽ được gọi từ SkillManager khi đến lượt hồi chiêu của skill này
    private CharacterStats skillstat;

    public void Execute(SkillData skillData, Transform firePoint, Vector2 direction)
    {
        if (skillData == null || skillData.skillPrefab == null || firePoint == null) return;

        Debug.Log($"<color=cyan>[SKILL RIÊNG]</color> {skillData.skillName} bộc phát 3 đường dẻ quạt!");

        // Góc lệch 15 độ cho 3 luồng phi kiếm (Trái, Giữa, Phải)
        float spreadAngle = 15f;
        float[] angles = { 0f, -spreadAngle, spreadAngle };

        foreach (float angle in angles)
        {
            // Xoay hướng bay theo góc dẻ quạt
            Quaternion rotation = Quaternion.Euler(0, 0, angle);
            Vector2 spreadDirection = rotation * direction;

            // Sinh ra Prefab kỹ năng tại điểm bắn
            GameObject skillObj = Instantiate(skillData.skillPrefab, firePoint.position, Quaternion.identity);

            // 1. Truyền hướng bay vào BaseSkillEffect (nếu có)
            BaseSkillEffect effect = skillObj.GetComponent<BaseSkillEffect>();
            if (effect != null)
            {
                effect.Initialize(spreadDirection);
            }

            // 2. Truyền hướng bay qua PhiKiem (nếu dùng chung script với đạn thường)
            PhiKiem scriptKiem = skillObj.GetComponent<PhiKiem>();
            if (scriptKiem != null)
            {
                scriptKiem.Setup(spreadDirection, skillstat);
            }

            // 3. Đẩy lực bằng Rigidbody2D
            Rigidbody2D rb = skillObj.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.linearVelocity = spreadDirection * 12f;
            }

            // Tự hủy sau 3 giây để tránh tràn bộ nhớ
            Destroy(skillObj, 3f);
        }
    }
}