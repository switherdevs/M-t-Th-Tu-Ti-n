using UnityEngine;
using UnityEngine.UI;
using StatsSystem.Components;

namespace StatsSystem.UI
{
    /// <summary>
    /// Gắn script này trực tiếp lên OBJECT CHA của Quái / Player (Nơi chứa Collider2D và Rigidbody2D).
    /// Kéo Slider con vào ô healthSlider trong Inspector.
    /// </summary>
    public class HealthBarUI : MonoBehaviour
    {
        [Header("=== REFERENCES ===")]
        [Tooltip("Kéo GameObject Slider (UI con) vào đây")]
        [SerializeField] private Slider healthSlider;

        [Tooltip("Kéo GameObject Canvas (UI cha hoặc chứa thanh máu) vào đây")]
        [SerializeField] private GameObject cavan; // 🎯 Bổ sung biến Canvas

        [Tooltip("Stats của nhân vật (Tự tìm trên Object Cha nếu để trống)")]
        [SerializeField] private CharacterStats targetStats;

        [Header("=== SETTINGS ===")]
        [Tooltip("Tích vào nếu đây là Player (Slider hiện ngay từ đầu). Bỏ tích nếu là Quái (Slider ẩn đi, dính kiếm mới hiện).")]
        [SerializeField] private bool isPlayer = false;

        [Tooltip("Tag của Vũ khí / Kiếm gây ra va chạm")]
        [SerializeField] private string weaponTag = "Kiem";

        [Tooltip("Ẩn Slider và Canvas khi quái chết")]
        [SerializeField] private bool hideOnDeath = true;

        private void Awake()
        {
            // Tự động tìm CharacterStats trên Object Cha nếu chưa kéo vào Inspector
            if (targetStats == null)
            {
                targetStats = GetComponent<CharacterStats>();
            }
        }

        private void Start()
        {
            if (targetStats != null)
            {
                KhoiTaoThanhMauLucDau();
            }
        }

        private void OnEnable()
        {
            if (targetStats != null)
            {
                targetStats.OnHealthChanged += UpdateHealthBar;
                targetStats.OnDeath += HandleDeath;

                UpdateHealthBar(targetStats.CurrentHealth, targetStats.MaxHealth.Value);
            }
        }

        private void OnDisable()
        {
            if (targetStats != null)
            {
                targetStats.OnHealthChanged -= UpdateHealthBar;
                targetStats.OnDeath -= HandleDeath;
            }
        }

        /// <summary>
        /// Khởi tạo Slider ban đầu và xử lý Ẩn/Hiện dựa theo biến isPlayer
        /// </summary>
        private void KhoiTaoThanhMauLucDau()
        {
            float maxHP = targetStats.MaxHealth.Value;
            float currentHP = targetStats.CurrentHealth;

            if (healthSlider != null)
            {
                healthSlider.maxValue = maxHP;
                healthSlider.value = currentHP;

                // Nếu là Player -> Bật Slider ngay lập tức. Nếu là Quái -> Tắt Slider đi.
                if (isPlayer)
                {
                    healthSlider.gameObject.SetActive(true);
                    if (cavan != null) cavan.SetActive(true);
                }
                else
                {
                    healthSlider.gameObject.SetActive(false);
                }
            }
        }

        private void UpdateHealthBar(float currentHealth, float maxHealth)
        {
            if (healthSlider == null) return;

            healthSlider.maxValue = maxHealth;
            healthSlider.value = currentHealth;
        }

        private void HandleDeath()
        {
            if (hideOnDeath)
            {
                if (healthSlider != null)
                {
                    healthSlider.gameObject.SetActive(false);
                }

                // 🎯 Tắt Canvas khi quái chết
                if (cavan != null)
                {
                    cavan.SetActive(false);
                }
            }
        }

        // ==========================================
        // XỬ LÝ VA CHẠM TRIGGER TỪ OBJECT CHA
        // ==========================================
        private void OnTriggerEnter2D(Collider2D collision)
        {
            // Nếu không phải Player, Slider đang tắt, và va chạm đúng Tag "Kiem"
            if (!isPlayer && collision.CompareTag(weaponTag))
            {
                if (healthSlider != null && !healthSlider.gameObject.activeSelf)
                {
                    healthSlider.gameObject.SetActive(true);
                }

                if (cavan != null && !cavan.activeSelf)
                {
                    cavan.SetActive(true);
                }
            }
        }
    }
}