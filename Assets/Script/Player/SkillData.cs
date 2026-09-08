using UnityEngine;

public class SkillData : ScriptableObject
{
    [Header("Cấu Hình Cơ Bản")]
    public string skillName;
    public GameObject skillPrefab;

    [Header("Cấu Hình Chỉ Số Kỹ Năng")]
    [Tooltip("Sát thương cơ bản của kỹ năng")]
    public float baseDamage = 20f;

    [Tooltip("Thời gian hồi chiêu của kỹ năng (tính bằng giây)")]
    public float cooldownTime = 5f;

    [Tooltip("Thời gian hồi chiêu tối thiểu (tránh hồi chiêu <= 0s)")]
    public float minCooldownTime = 1f;

    [Tooltip("Số năng lượng (Mana) tiêu tốn khi thi triển kỹ năng này")]
    public float manaCost = 20f;

    [Header("CẤU HÌNH CẤP ĐỘ KỸ NĂNG")]
    public int currentLevel = 1;

    public virtual void UseSkill(Transform firePoint, Vector2 direction)
    {
        // Class cha làm khung cho các class con override
    }

    /// <summary>
    /// Hàm xử lý nâng cấp chỉ số trực tiếp khi người chọn skill này
    /// </summary>
    public virtual void UpgradeSkill(float damageBonus, float cooldownReduction)
    {
        currentLevel++;
        baseDamage += damageBonus;

        // Giảm thời gian hồi nhưng không vượt quá mốc tối thiểu
        cooldownTime = Mathf.Max(minCooldownTime, cooldownTime - cooldownReduction);

        Debug.Log($"<color=green>[UPGRADE]</color> {skillName} lên Cấp {currentLevel}! Sát thương: {baseDamage} | Hồi chiêu: {cooldownTime}s");
    }
}