using UnityEngine;

public class SkillData : ScriptableObject
{
    [Header("Cấu Hình Cơ Bản")]
    public string skillName;
    public GameObject skillPrefab;

    [Header("Cấu Hình Chỉ Số Mặc Định (Cấp 1)")]
    public float defaultDamage = 20f;
    public float defaultCooldown = 5f;

    [Header("Cấu Hình Chỉ Số Kỹ Năng (Hiện Tại)")]
    public float baseDamage = 20f;
    public float cooldownTime = 5f;
    public float minCooldownTime = 1f;
    public float manaCost = 20f;

    [Header("CẤU HÌNH CẤP ĐỘ KỸ NĂNG")]
    public int currentLevel = 1;
    public const int MAX_LEVEL = 10; // 🎯 Giới hạn cấp độ tối đa là 10

    public virtual void UseSkill(Transform firePoint, Vector2 direction)
    {
        // Class cha làm khung cho các class con override
    }

    /// <summary>
    /// 🎯 ĐÃ NÂNG CẤP: Dùng hệ số nhân sát thương và hệ số giảm thời gian hồi chiêu
    /// </summary>
    public virtual void UpgradeSkill(float heSoNhanDamage, float heSoGiamCooldown)
    {
        if (currentLevel >= MAX_LEVEL) return;

        currentLevel++;

        // Sát thương tăng theo hệ số nhân (Ví dụ: baseDamage cũ * hệ số nhân)
        baseDamage *= heSoNhanDamage;

        // Thời gian hồi chiêu giảm theo hệ số (Ví dụ: cooldownTime cũ * hệ số giảm, ko thấp hơn minCooldownTime)
        cooldownTime = Mathf.Max(minCooldownTime, cooldownTime * heSoGiamCooldown);

        Debug.Log($"<color=green>[UPGRADE]</color> {skillName} lên Cấp {currentLevel}! Sát thương: {baseDamage:F1} | Hồi chiêu: {cooldownTime:F1}s");
    }

    // Reset Skill về đúng Cấp 1 và Chỉ Số Gốc ban đầu
    public void ResetVeChiSoGoc()
    {
        currentLevel = 1;
        baseDamage = defaultDamage;
        cooldownTime = defaultCooldown;
    }
}