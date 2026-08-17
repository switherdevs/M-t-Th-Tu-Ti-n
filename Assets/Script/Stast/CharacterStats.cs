using System;
using System.Collections;
using UnityEngine;
using StatsSystem.Core;
using StatsSystem.Interfaces;
using StatsSystem.Services;
using PersistenceSystem;

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

        // =========================================================
        // BỔ SUNG CONFIGURATION PLAYER
        // =========================================================
        [Header("=== PLAYER CONFIGURATION ===")]
        [SerializeField, Tooltip("Tick vào đây nếu GameObject này là Player")]
        private bool isPlayer = false;

        [SerializeField, Tooltip("Tên Trigger/Bool Animation Chết")]
        private string dieAnimName = "Die";

        [SerializeField, Tooltip("Thời gian chờ (giây) trước khi dừng game và hiện UI")]
        private float deathDelay = 2.0f;

        [SerializeField, Tooltip("GameObject Canvas UI GameOver hiển thị khi người chơi chết")]
        private GameObject gameOverUI;

        // =========================================================
        // THÊM TÍNH NĂNG BOSS (CHECKBOX INSPECTOR)
        // =========================================================
        [Header("=== BOSS CONFIGURATION ===")]
        [SerializeField, Tooltip("Tick vào đây nếu GameObject này là Boss để kích hoạt cơ chế nhận X3 sát thương khi mệt")]
        private bool isBoss = false;

        [SerializeField, Tooltip("Hệ số nhân sát thương khi Boss bị mệt (Mặc định x3)")]
        private float tiredDamageMultiplier = 3f;

        // Tham chiếu tự động tới script Boss & Animator
        private BossDaSatMaQuan bossController;
        private Animator anim;

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
            anim = GetComponentInChildren<Animator>();

            // Ẩn bảng UI GameOver lúc bắt đầu nếu chưa ẩn
            if (gameOverUI != null)
            {
                gameOverUI.SetActive(false);
            }

            // Nếu là Boss, tự động lấy Script BossDaSatMaQuan gắn trên cùng GameObject
            if (isBoss)
            {
                bossController = GetComponent<BossDaSatMaQuan>();
                if (bossController == null)
                {
                    Debug.LogWarning($"⚠️ [CharacterStats] {gameObject.name} được tick là Boss nhưng không tìm thấy script BossDaSatMaQuan!");
                }
            }
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

            // KIỂM TRA ĐIỀU KIỆN BOSS VÀ TRẠNG THÁI MỆT
            if (isBoss && bossController != null)
            {
                if (bossController.IsTired)
                {
                    rawDamage *= tiredDamageMultiplier; // Nhân 3 sát thương khi mệt
                    Debug.Log($"💥 BOSS ĐANG MỆT! Sát thương nhận vào bị nhân {tiredDamageMultiplier} lần: {rawDamage}");
                }
            }

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

            // XỬ LÝ KHI OBJECT LÀ PLAYER CHẾT
            if (isPlayer)
            {
                // 1. Đổi Tag và Layer thành "Default"
                gameObject.tag = "Untagged";
                gameObject.layer = LayerMask.NameToLayer("Default");

                // 2. Chạy Animation chết (nếu có)
                if (anim != null && !string.IsNullOrEmpty(dieAnimName))
                {
                    anim.SetTrigger(dieAnimName);
                }

                // 3. Khởi chạy đếm ngược gian chờ dừng game và mở UI
                StartCoroutine(Routine_PlayerDeathSequence());
            }
        }

        /// <summary>
        /// Coroutine đếm ngược sau khi Player chết
        /// </summary>
        private IEnumerator Routine_PlayerDeathSequence()
        {
            // Chờ hết khoảng thời gian delay thiết lập trên Inspector
            yield return new WaitForSeconds(deathDelay);

            // Bật UI GameOver (nếu đã kéo vào Inspector)
            if (gameOverUI != null)
            {
                gameOverUI.SetActive(true);
            }

            // Tạm dừng toàn bộ Game
            Time.timeScale = 0f;
        }

        /// <summary>
        /// Phương thức mở rộng để lấy Stat theo StatType (Rất hữu ích cho Hệ thống Buff/Trang bị sau me)
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
        #region PERSISTENCE SYSTEM

        #endregion
    }

    /// <summary>
    /// Attribute nhỏ giúp hiển thị ReadOnly trên Inspector (Cho CurrentHealth)
    /// </summary>
    public class ReadOnlyInspectorAttribute : PropertyAttribute { }
}