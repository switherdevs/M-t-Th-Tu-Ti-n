using System;
using UnityEngine;
using StatsSystem.Core;
using StatsSystem.Interfaces;
using StatsSystem.Services;

namespace StatsSystem.Components
{
    [DisallowMultipleComponent]
    public class CharacterStats : MonoBehaviour, IDamageable
    {
        [Header("=== BASE STATS ===")]
        [SerializeField, Tooltip("Máu tối đa ban đầu")]
        private Stat maxHealth = new Stat(100f);

        [SerializeField, Tooltip("Sát thương gây ra ban đầu")]
        private Stat attack = new Stat(20f);

        [SerializeField, Tooltip("Tỷ lệ giảm sát thương (0.2 = 20%, tối đa 0.95)")]
        private Stat defense = new Stat(0.1f);

        [Header("=== CURRENT STATE ===")]
        [SerializeField, ReadOnlyInspector]
        private float currentHealth;

        // Properties
        public Stat MaxHealth => maxHealth;
        public Stat Attack => attack;
        public Stat Defense => defense;

        public float CurrentHealth => currentHealth;
        public bool IsDead => currentHealth <= 0;

        // EVENTS (UI & Game Logic chỉ đăng ký nghe Event, không can thiệp biến trực tiếp)
        public event Action<float, float> OnHealthChanged; // (currentHP, maxHP)
        public event Action<float> OnDamaged;              // (damageTaken)
        public event Action<float> OnHealed;               // (healAmount)
        public event Action OnDeath;

        private void Awake()
        {
            currentHealth = MaxHealth.Value;
        }

        private void OnEnable()
        {
            MaxHealth.OnValueChanged += HandleMaxHealthChanged;
        }

        private void OnDisable()
        {
            MaxHealth.OnValueChanged -= HandleMaxHealthChanged;
        }

        private void HandleMaxHealthChanged(Stat stat)
        {
            // Đảm bảo CurrentHealth không vượt quá MaxHealth khi MaxHealth giảm
            currentHealth = Mathf.Min(currentHealth, stat.Value);
            OnHealthChanged?.Invoke(currentHealth, stat.Value);
        }

        public void TakeDamage(float rawDamage)
        {
            if (IsDead || rawDamage <= 0) return;

            // Tính toán sát thương thông qua DamageCalculator Service
            float finalDamage = DamageCalculator.CalculateDamage(rawDamage, Defense.Value);

            currentHealth -= finalDamage;
            currentHealth = Mathf.Max(0f, currentHealth);

            OnDamaged?.Invoke(finalDamage);
            OnHealthChanged?.Invoke(currentHealth, MaxHealth.Value);

            if (IsDead)
            {
                Die();
            }
        }

        public void Heal(float amount)
        {
            if (IsDead || amount <= 0) return;

            currentHealth += amount;
            currentHealth = Mathf.Min(currentHealth, MaxHealth.Value);

            OnHealed?.Invoke(amount);
            OnHealthChanged?.Invoke(currentHealth, MaxHealth.Value);
        }

        protected virtual void Die()
        {
            OnDeath?.Invoke();
            // KHÔNG Destroy GameObject ở đây để Player/Enemy tự xử lý animation/audio/logic riêng.
        }

        /// <summary>
        /// Phương thức mở rộng để lấy Stat theo StatType (Rất hữu ích cho Hệ thống Buff/Trang bị sau này)
        /// </summary>
        public Stat GetStat(StatType type)
        {
            return type switch
            {
                StatType.MaxHealth => maxHealth,
                StatType.Attack => attack,
                StatType.Defense => defense,
                _ => null
            };
        }
    }

    /// <summary>
    /// Attribute nhỏ giúp hiển thị ReadOnly trên Inspector (Cho CurrentHealth)
    /// </summary>
    public class ReadOnlyInspectorAttribute : PropertyAttribute { }
}