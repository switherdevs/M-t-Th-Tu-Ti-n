using StatsSystem.Components;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSkillManager : MonoBehaviour
{
    [Header("Danh Sách Kỹ Năng Đang Trang Bị")]
    [SerializeField] private List<SkillData> equippedSkills = new List<SkillData>();

    [Header("Chỉ Số Nhân Vật (Dùng cho Skill)")]
    [SerializeField] private CharacterStats skillStat;

    public void TriggerAllSkills(Transform firePoint, Vector2 direction)
    {
        if (equippedSkills == null || equippedSkills.Count == 0) return;

        foreach (var skill in equippedSkills)
        {
            if (skill == null || skill.skillPrefab == null) continue;

            // Dùng Random.Range chuẩn của Unity
            float roll = Random.Range(0f, 100f);

            if (roll <= skill.triggerChance)
            {
                // Gọi hàm UseSkill (truyền đủ tham số hoặc gọi trực tiếp qua skill con)
                skill.UseSkill(firePoint, direction);
            }
        }
    }
}