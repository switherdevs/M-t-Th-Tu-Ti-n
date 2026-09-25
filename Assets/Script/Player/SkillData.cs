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

    public virtual void UseSkill(Transform firePoint, Vector2 direction)
    {
        // Class cha làm khung cho các class con override
    }

    public virtual void UpgradeSkill(float damageBonus, float cooldownReduction)
    {
        currentLevel++;
        baseDamage += damageBonus;
        cooldownTime = Mathf.Max(minCooldownTime, cooldownTime - cooldownReduction);

        Debug.Log($"<color=green>[UPGRADE]</color> {skillName} lên Cấp {currentLevel}! Sát thương: {baseDamage} | Hồi chiêu: {cooldownTime}s");
    }

    // 🎯 HÀM MỚI: Reset Skill về đúng Cấp 1 và Chỉ Số Gốc ban đầu
    public void ResetVeChiSoGoc()
    {
        currentLevel = 1;
        baseDamage = defaultDamage;
        cooldownTime = defaultCooldown;
    }
}