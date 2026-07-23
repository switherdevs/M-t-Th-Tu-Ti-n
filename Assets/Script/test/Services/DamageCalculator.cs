using UnityEngine;

namespace StatsSystem.Services
{
    public static class DamageCalculator
    {
        public const float MAX_DEFENSE_PERCENT = 0.95f; // Giới hạn DEF tối đa 95%

        /// <summary>
        /// Tính toán sát thương thực nhận dựa trên ATK và DEF
        /// Formula: Final Damage = Damage * (1 - DEF%)
        /// </summary>
        public static float CalculateDamage(float rawDamage, float defensePercent)
        {
            // Ép DEF trong khoảng [0%, 95%]
            float clampedDEF = Mathf.Clamp(defensePercent, 0f, MAX_DEFENSE_PERCENT);

            // Tính sát thương thực nhận
            float finalDamage = rawDamage * (1f - clampedDEF);

            // Làm tròn số nguyên cho game xịn xịn (hoặc giữ float tùy bạn)
            return Mathf.Max(0f, Mathf.Round(finalDamage));
        }
    }
}