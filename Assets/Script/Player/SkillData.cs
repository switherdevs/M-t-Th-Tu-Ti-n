using UnityEngine;

public class SkillData : ScriptableObject
{
    [Header("Cấu Hình Cơ Bản")]
    public string skillName;
    public GameObject skillPrefab;

    [Tooltip("Tỷ lệ xuất hiện kỹ năng (%)")]
    public float triggerChance = 100f;

    [Header("Cấu Hình Hồi Chiêu & Năng Lượng")]
    [Tooltip("Thời gian hồi chiêu của kỹ năng (tính bằng giây)")]
    public float cooldownTime = 5f;

    [Tooltip("Số năng lượng (Mana) tiêu tốn khi thi triển kỹ năng này")]
    public float manaCost = 20f; // Bổ sung biến năng lượng tiêu tốn

    public virtual void UseSkill(Transform firePoint, Vector2 direction)
    {
        // Class cha làm khung cho các class con override
    }
}