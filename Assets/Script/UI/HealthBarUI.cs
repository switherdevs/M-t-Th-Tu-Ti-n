using UnityEngine;
using UnityEngine.UI;
using StatsSystem.Components;

namespace StatsSystem.UI
{
    public class HealthBarUI : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Slider healthSlider;
        [SerializeField] private CharacterStats targetStats;

        [Header("Settings")]
        [SerializeField] private bool hideOnDeath = false;

        private void Awake()
        {
            if (healthSlider == null)
            {
                healthSlider = GetComponent<Slider>();
            }
        }

        private void OnEnable()
        {
            if (targetStats != null)
            {
                // Đăng ký nghe sự kiện thay đổi máu
                targetStats.OnHealthChanged += UpdateHealthBar;
                targetStats.OnDeath += HandleDeath;

                // Cập nhật thanh máu lần đầu tiên khi Object được bật
                UpdateHealthBar(targetStats.CurrentHealth, targetStats.MaxHealth.Value);
            }
        }

        private void OnDisable()
        {
            if (targetStats != null)
            {
                // Hủy đăng ký event để tránh memory leak
                targetStats.OnHealthChanged -= UpdateHealthBar;
                targetStats.OnDeath -= HandleDeath;
            }
        }

        /// <summary>
        /// Tự động thiết lập Target Stats thông qua code nếu spawn quái bằng code
        /// </summary>
        public void Setup(CharacterStats stats)
        {
            if (targetStats != null)
            {
                targetStats.OnHealthChanged -= UpdateHealthBar;
                targetStats.OnDeath -= HandleDeath;
            }

            targetStats = stats;

            if (targetStats != null)
            {
                targetStats.OnHealthChanged += UpdateHealthBar;
                targetStats.OnDeath += HandleDeath;
                UpdateHealthBar(targetStats.CurrentHealth, targetStats.MaxHealth.Value);
            }
        }

        private void UpdateHealthBar(float currentHealth, float maxHealth)
        {
            if (healthSlider == null) return;

            // Tính tỷ lệ máu dạng float từ 0.0 đến 1.0
            healthSlider.maxValue = maxHealth;
            healthSlider.value = currentHealth;
        }

        private void HandleDeath()
        {
            if (hideOnDeath && gameObject != null)
            {
                gameObject.SetActive(false);
            }
        }
    }
}