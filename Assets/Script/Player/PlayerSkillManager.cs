using StatsSystem.Components;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSkillManager : MonoBehaviour
{
    [Header("Danh Sách Kỹ Năng Đang Trang Bị")]
    [SerializeField] private List<SkillData> equippedSkills = new List<SkillData>();
    private CharacterStats skillStat;


    public void TriggerAllSkills(Transform firePoint, Vector2 direction)
    {
        if (equippedSkills == null || equippedSkills.Count == 0) return;

        foreach (var skill in equippedSkills)
        {
            if (skill == null || skill.skillPrefab == null) continue;

            // Quay số ngẫu nhiên từ 0 đến 100 để kiểm tra tỉ lệ %
            float roll = Random.Range(0f, 100f);

            // Nếu con số quay được nhỏ hơn hoặc bằng tỉ lệ triggerChance thì kích hoạt
            if (roll <= skill.triggerChance)
            {
                ExecuteTripleBlade(skill, firePoint, direction);
            }
        }
    }

    private void ExecuteTripleBlade(SkillData skill, Transform firePoint, Vector2 direction)
    {
        Debug.Log($"<color=cyan>[TỈ LỆ KÍCH HOẠT THÀNH CÔNG]</color> {skill.skillName} bộc phát 3 đường!");

        // Góc lệch 15 độ cho 3 luồng phi kiếm (Trái, Giữa, Phải)
        float spreadAngle = 15f;
        float[] angles = { 0f, -spreadAngle, spreadAngle };

        foreach (float angle in angles)
        {
            // Xoay hướng bay theo góc dẻ quạt
            Quaternion rotation = Quaternion.Euler(0, 0, angle);
            Vector2 spreadDirection = rotation * direction;

            // Sinh ra Prefab kỹ năng tại điểm bắn
            GameObject skillObj = Instantiate(skill.skillPrefab, firePoint.position, Quaternion.identity);

            // Truyền hướng bay vào BaseSkillEffect (hoặc PhiKiem)
            BaseSkillEffect effect = skillObj.GetComponent<BaseSkillEffect>();
            if (effect != null)
            {
                effect.Initialize(spreadDirection);
            }

            PhiKiem scriptKiem = skillObj.GetComponent<PhiKiem>();
            if (scriptKiem != null)
            {
                scriptKiem.Setup(spreadDirection, skillStat);
            }

            // Đảm bảo có Rigidbody2D để bay
            Rigidbody2D rb = skillObj.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.linearVelocity = spreadDirection * 12f;
            }

            // Tự hủy sau 3 giây
            Destroy(skillObj, 3f);
        }
    }
}